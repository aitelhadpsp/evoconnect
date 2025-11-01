using FirebirdSql.Data.FirebirdClient;
using ReactiveUI;
using System.Net;
using System.Net.Sockets;
using QRCoder;
using System.Drawing.Imaging;
using Avalonia.Controls;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EvoConnect.Common;
using EvoConnect.Common.Models;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System;
using System.Reactive.Disposables;

namespace EvoConnect.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IActivatableViewModel
{
    private readonly ServerManager? _serverManager;
    private CompositeDisposable? _disposables;

    public ViewModelActivator Activator { get; } = new();

    [ObservableProperty]
    private string? _ipAddress = null;

    [ObservableProperty]
    private string? _partnerName = null;
    
    [ObservableProperty]
    private string? _subscriptionType = null;

    [ObservableProperty]
    private Control _qrContentControl = new StackPanel { };

    [ObservableProperty]
    private bool _homeScreen = true;

    [ObservableProperty]
    private bool _qrScreen = false;

    [ObservableProperty]
    private bool _regKeyExist = false;

    [ObservableProperty]
    private bool _allSet = false;

    [ObservableProperty]
    private bool _dbConnected = false;

    [ObservableProperty]
    private bool _dbError = false;

    [ObservableProperty]
    private bool _serverConnecting = false;

    [ObservableProperty]
    private bool _serverConnected = false;

    [ObservableProperty]
    private bool _serverWrongKey = false;

    [ObservableProperty]
    private bool _serverConnectFail = false;

    [ObservableProperty]
    private bool _syncing = false;

    [ObservableProperty]
    private string? _apiKey;

    // Server Management Properties
    [ObservableProperty]
    private string _serverStatus = "Unknown";

    [ObservableProperty]
    private bool _isServerRunning;

    [ObservableProperty]
    private bool _showServerControls = false;

    public MainWindowViewModel(ServerManager? serverManager = null)
    {
        _serverManager = serverManager;
        Init();

     
            
            // Listen for server status changes
            MessageBus.Current.Listen<ServerStatusChangedMessage>()
                .Subscribe(msg =>
                {
                    ServerStatus = msg.Status;
                    IsServerRunning = msg.IsRunning;
                })
               ;
 

        // Set initial server status
        if (_serverManager != null)
        {
            ServerStatus = _serverManager.Status;
            IsServerRunning = _serverManager.IsRunning;
        }
    }

    public void ChangeSyncState(bool state)
    {
        Syncing = state;
    }

    public static string GetPhysicalNetworkIP()
    {
        // Get all network interfaces
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface adapter in interfaces)
        {
            // Skip virtual adapters
            if (adapter.Name.Contains("vEthernet") ||
                adapter.Name.Contains("WSL") ||
                adapter.Description.Contains("Virtual") ||
                adapter.Description.Contains("Hyper-V") ||
                adapter.Description.Contains("VirtualBox"))
            {
                continue;
            }

            // Check if the adapter is up and is a physical interface
            if (adapter.OperationalStatus == OperationalStatus.Up &&
                (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                 adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();

                // Get IPv4 addresses
                foreach (UnicastIPAddressInformation ip in properties.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        // Exclude loopback and virtual addresses
                        if (!IPAddress.IsLoopback(ip.Address) &&
                            !ip.Address.ToString().StartsWith("172."))  // Exclude common virtual network ranges
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
        }

        return null;
    }

    public async Task Init()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var ip = GetPhysicalNetworkIP();

        if (ip != null)
        {
            IpAddress = "flashEvo:" + ip.ToString();
            var content = new Image
            {
                Height = 100,
                Source = GenerateQRCode(IpAddress)
            };
            QrContentControl = content;
        }

        if (AppData.IsServer())
        {
            TestDb();
        }
        else
        {
            RegKeyExist = true;
        }

        var config = AppData.GetApiKey();
        if (!string.IsNullOrEmpty(config))
        {
            ApiKey = config;
            _ = GetPartner();
        }
    }

    async Task<Partner?> GetPartner()
    {
        try
        {
            var key = ApiKey;
            if (key == null)
                return null;

            var response = await Getters.GetPartnerByCode(key);
            if (response != null)
            {
                PartnerName = response.Name;
                SubscriptionType = response.SubscriptionType == 0 ? "Essential" : "Pro";
                HomeScreen = false;
                QrScreen = true;

                // Auto-start server in production after successful partner validation
                #if !DEBUG
                if (_serverManager != null && !_serverManager.IsRunning)
                {
                    _ = Task.Run(async () => await _serverManager.StartServerAsync());
                }
                #endif
            }
            else
            {
                ServerWrongKey = true;
                HomeScreen = true;
                QrScreen = false;
            }
            return response;
        }
        catch
        {
            ServerConnectFail = true;
            HomeScreen = true;
            QrScreen = false;
        }
        return null;
    }

    private void TestDb()
    {
        string? connectionString = AppData.ConnectionString();
        if (connectionString is null)
        {
            RegKeyExist = false;
            return;
        }
        RegKeyExist = true;

        using FbConnection connection = new(connectionString);
        try
        {
            DbError = false;
            connection.Open();
        }
        catch
        {
            DbError = true;
        }
        finally
        {
            connection.Close();
        }
    }

    [RelayCommand]
    public void Connect()
    {
        ConnectToServer();
    }

    [RelayCommand]
    public void InitScreen()
    {
        HomeScreen = true;
        QrScreen = false;
        ShowServerControls = false;
    }

    [RelayCommand]
    public void ShowServerStatus()
    {
        ShowServerControls = !ShowServerControls;
    }

    [RelayCommand]
    public async Task StartServer()
    {
        if (_serverManager != null)
        {
            await _serverManager.StartServerAsync();
        }
    }

    [RelayCommand]
    public async Task StopServer()
    {
        if (_serverManager != null)
        {
            await _serverManager.StopServerAsync();
        }
    }

    [RelayCommand]
    public async Task RestartServer()
    {
        if (_serverManager != null)
        {
            await _serverManager.StopServerAsync();
            await Task.Delay(1000);
            await _serverManager.StartServerAsync();
        }
    }

    [RelayCommand]
    public void Exit()
    {
        MessageBus.Current.SendMessage(new ExitApplicationMessage());
    }

    public async void ConnectToServer()
    {
        ServerConnectFail = false;
        ServerWrongKey = false;
        ServerConnecting = true;

        var response = await GetPartner();
        if (response != null)
            SaveData();
        
        ServerConnecting = false;
    }

    private void SaveData()
    {
        AppData.SetApiKey(ApiKey);
        HomeScreen = false;
        QrScreen = true;
    }

    private Avalonia.Media.Imaging.Bitmap GenerateQRCode(string text)
    {
        using var qrGenerator = new QRCodeGenerator();

        // Create QR code data with ECC level Q
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        // Create a new QR code object
        using var qrCode = new QRCode(qrCodeData);

        // Generate the QR code as a System.Drawing.Bitmap
        using (System.Drawing.Bitmap qrCodeBitmap = qrCode.GetGraphic(20))
        {
            // Convert System.Drawing.Bitmap to Avalonia.Media.Imaging.Bitmap
            using (MemoryStream memoryStream = new MemoryStream())
            {
                qrCodeBitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return new Avalonia.Media.Imaging.Bitmap(memoryStream);
            }
        }
    }
}