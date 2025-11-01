using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using EvoConnect.Common;
using EvoConnect.UI.ViewModels;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;

namespace EvoConnect.UI
{
    public class StartCollecting { }
    public class RestartApplicationMessage { }
    public class ShowApplicationMessage { }
    public class ExitApplicationMessage { }
    public class ServerStatusChangedMessage 
    { 
        public bool IsRunning { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public partial class App : Application
    {
        private IContainer Container { get; set; }
        private MainWindow? Window { get; set; }
        private ServerManager? _serverManager;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public App()
        {
            MessageBus.Current.Listen<RestartApplicationMessage>().Subscribe(_ => RestartApplication());
            MessageBus.Current.Listen<ExitApplicationMessage>().Subscribe((_) => HideApplication());
            MessageBus.Current.Listen<ShowApplicationMessage>().Subscribe(_ => ShowApplication());
            
            _serverManager = new ServerManager();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();
            RegisterTrayIcon();
            
            var token = AppData.GetApiKey();
            if (string.IsNullOrEmpty(token))
            {
                ShowApplication();
            }
            else
            {
                // In production, start the server automatically
                #if !DEBUG
                _ = Task.Run(async () => await _serverManager.StartServerAsync());
                #endif
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownRequested += OnShutdownRequested;
            }
        }

        private void ShowApplication()
        {
            if (Window != null)
            {
                Window.Show();
            }
            else if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Window = new MainWindow
                {
                    DataContext = new MainWindowViewModel(_serverManager),
                };
                desktop.MainWindow = Window;
                Window.Show();
            }
        }

        private void HideApplication()
        {
            Window?.Hide();
        }

        private async void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            e.Cancel = true;
            
            // Stop the server gracefully
            if (_serverManager != null)
            {
                await _serverManager.StopServerAsync();
            }
            
            Environment.Exit(0);
        }

        private void RegisterTrayIcon()
        {
            var icon = AssetLoader.Open(new Uri("avares://EvoConnect.UI/Assets/icon.ico"));
            var trayIcon = new TrayIcon
            {
                IsVisible = true,
                ToolTipText = "Dentalevo Connect",
                Command = ReactiveCommand.Create(ShowApplication),
                Icon = new WindowIcon(icon)
            };
        }

        public void RestartApplication()
        {
            var oldwindow = Window;
            Window = new MainWindow
            {
                DataContext = new MainWindowViewModel(_serverManager),
            };
            if (Window.ShowActivated)
                oldwindow?.Hide();
        }
    }

    public class ServerManager
    {
        private Process? _serverProcess;
        private IHost? _host;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly HttpClient _httpClient;
        private const int ServerPort = 6222;
        private readonly string ServerUrl = $"http://localhost:{ServerPort}";

        public bool IsRunning { get; private set; }
        public string Status { get; private set; } = "Stopped";

        public ServerManager()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> StartServerAsync()
        {
            try
            {
                #if DEBUG
                // In debug mode, start as external process for debugging
                return await StartServerInProcessAsync();
                #else
                // In production, run server in-process
                return await StartServerProcessAsync();
                #endif
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
                MessageBus.Current.SendMessage(new ServerStatusChangedMessage { IsRunning = false, Status = Status });
                return false;
            }
        }

        private async Task<bool> StartServerProcessAsync()
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
                return true;

            try
            {
                var serverPath = GetServerExecutablePath();
                if (!File.Exists(serverPath))
                {
                    Status = "Server executable not found";
                    MessageBus.Current.SendMessage(new ServerStatusChangedMessage { IsRunning = false, Status = Status });
                    return false;
                }

                _serverProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = serverPath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                _serverProcess.Start();
                
                // Wait a moment for the server to start
                await Task.Delay(3000);
                
                // Check if server is responding
                var isHealthy = await CheckServerHealthAsync();
                if (isHealthy)
                {
                    IsRunning = true;
                    Status = "Running (External Process)";
                    MessageBus.Current.SendMessage(new ServerStatusChangedMessage { IsRunning = true, Status = Status });
                    return true;
                }
                else
                {
                    Status = "Failed to start server process";
                    MessageBus.Current.SendMessage(new ServerStatusChangedMessage { IsRunning = false, Status = Status });
                    return false;
                }
            }
            catch (Exception ex)
            {
                Status = $"Error starting process: {ex.Message}";
                MessageBus.Current.SendMessage(new ServerStatusChangedMessage { IsRunning = false, Status = Status });
                return false;
            }
        }

        private async Task<bool> StartServerInProcessAsync()
        {
            if (_host != null)
                return true;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                
                var builder = CreateServerHostBuilder();
                _host = builder.Build();
                
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _host.RunAsync(_cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancellation is requested
                    }
                });

                // Wait for server to start
                await Task.Delay(2000);
                
                var isHealthy = await CheckServerHealthAsync();
                if (isHealthy)
                {
                    IsRunning = true;
                    Status = "Running (In-Process)";
                    MessageBus.Current.SendMessage(new ServerStatusChangedMessage { IsRunning = true, Status = Status });
                    return true;
                }
                else
                {
                    Status = "Failed to start server";
                    MessageBus.Current.SendMessage(new ServerStatusChangedMessage { IsRunning = false, Status = Status });
                    return false;
                }
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
                MessageBus.Current.SendMessage(new ServerStatusChangedMessage { IsRunning = false, Status = Status });
                return false;
            }
        }

        public async Task StopServerAsync()
        {
            try
            {
                #if DEBUG
                if (_serverProcess != null && !_serverProcess.HasExited)
                {
                    _serverProcess.Kill();
                    _serverProcess.Dispose();
                    _serverProcess = null;
                }
                #else
                if (_host != null)
                {
                    _cancellationTokenSource?.Cancel();
                    await _host.StopAsync();
                    _host.Dispose();
                    _host = null;
                }
                #endif

                IsRunning = false;
                Status = "Stopped";
                MessageBus.Current.SendMessage(new ServerStatusChangedMessage { IsRunning = false, Status = Status });
            }
            catch (Exception ex)
            {
                Status = $"Error stopping: {ex.Message}";
                MessageBus.Current.SendMessage(new ServerStatusChangedMessage { IsRunning = false, Status = Status });
            }
        }

        private async Task<bool> CheckServerHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ServerUrl}/device.xml", 
                    new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private string GetServerExecutablePath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "EvoConnect.Server.exe");
        }

        private IHostBuilder CreateServerHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(ServerUrl);
                    webBuilder.UseStartup<ServerStartup>();
                });
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _serverProcess?.Dispose();
            _host?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}