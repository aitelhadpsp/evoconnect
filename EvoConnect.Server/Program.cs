using System.Diagnostics;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Text.Json.Serialization;
using EvoConnect.Common;
using EvoConnect.Common.Models;
using EvoConnect.Server;
using EvoConnect.Server.Background;
using EvoConnect.Server.Data;
using EvoConnect.Server.Initializers;
using EvoConnect.Server.Repository;
using EvoConnect.Server.Repository.Interfaces;
using EvoConnect.Server.Services;
using EvoConnect.Server.Sync;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

// Vérifier les arguments de ligne de commande pour l'installation du service
if (args.Length > 0)
{
    var command = args[0].ToLower();

    switch (command)
    {
        case "--install-service":
        case "/install-service":
            InstallService();
            return;

        case "--uninstall-service":
        case "/uninstall-service":
            UninstallService();
            return;

        case "--start-service":
        case "/start-service":
            StartServiceCommand();
            return;

        case "--stop-service":
        case "/stop-service":
            StopServiceCommand();
            return;

        case "--help":
        case "/?":
            ShowHelp();
            return;
    }
}

var builder = WebApplication.CreateBuilder(args);

// Configurer comme service Windows
builder.Host.UseWindowsService();

// Configurer le logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventLog(settings =>
{
    settings.SourceName = "EvoConnect";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.UseUrls("http://0.0.0.0:6236");

var services = builder.Services;
services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System
            .Text
            .Json
            .JsonNamingPolicy
            .CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var conn = AppData.EfConnectionString();

services.AddDbContext<ClinicDbContext>(
    options =>
    {
        options.UseFirebird(conn);
    },
    ServiceLifetime.Scoped
);

var initializer = new KpiDatabaseInitializer(conn);

try
{
    await initializer.InitializeAsync();
    Console.WriteLine("KPI database ready");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to initialize KPI database: {ex.Message}");
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Failed to initialize KPI database");
    throw;
}

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        };
    });

services.AddEndpointsApiExplorer();

services.AddHttpContextAccessor();
services.AddScoped<IAppointmentsDA, AppointmentsDA>();
services.AddScoped<IFinancialDA, FinancialDA>();
services.AddScoped<IImagesDA, ImagesDA>();
services.AddScoped<IPartnerDA, PartnerDA>();
services.AddScoped<IPatientsDA, PatientsDA>();
services.AddScoped<IPaymentDA, PaymentDA>();
services.AddScoped<IActesRepository, ActesRepository>();
services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
services.AddScoped<IAuthRepository, AuthRepository>();
services.AddSingleton<IExecuterDA, ExecuterDA>();
services.AddSingleton<Synchronise>();
services.AddSingleton<CancelationConf>();
services.AddScoped<IKpiConfigRepository, KpiConfigRepository>();

services.AddHostedService<DataCollector>();
services.AddHostedService<UdpServicePublisher>();

if (AppData.IsServer())
{
    builder.Services.AddScoped<VipStatsRefreshService>();
    builder.Services.AddHostedService<VipStatsBackgroundService>();
}

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(AppData.ImageDir()),
        RequestPath = "/image",
        ServeUnknownFileTypes = false,
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
            ctx.Context.Response.Headers.Append("Accept-Ranges", "bytes");
        },
    }
);
app.MapControllers();
app.UseRouting();

app.UseHttpsRedirection();

app.Run();

// ============= Méthodes de gestion du service =============

static bool IsAdministrator()
{
    var identity = WindowsIdentity.GetCurrent();
    var principal = new WindowsPrincipal(identity);
    return principal.IsInRole(WindowsBuiltInRole.Administrator);
}

static void EnsureAdministrator()
{
    if (!IsAdministrator())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ERREUR: Cette opération nécessite des privilèges administrateur.");
        Console.WriteLine("Veuillez relancer l'application en tant qu'administrateur.");
        Console.ResetColor();
        Console.WriteLine("\nAppuyez sur une touche pour quitter...");
        Console.ReadKey();
        Environment.Exit(1);
    }
}

static void InstallService()
{
    EnsureAdministrator();

    const string serviceName = "EvoConnectServer";
    const string displayName = "EvoConnect Server";
    const string description = "Service de gestion EvoConnect pour cliniques dentaires";

    try
    {
        // Vérifier si le service existe déjà
        var existingService = ServiceController
            .GetServices()
            .FirstOrDefault(s => s.ServiceName == serviceName);

        if (existingService != null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Le service existe déjà. Désinstallation en cours...");
            Console.ResetColor();
            UninstallServiceInternal(serviceName);
            Thread.Sleep(2000);
        }

        // Obtenir le chemin de l'exécutable actuel
        var exePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(exePath))
        {
            throw new Exception("Impossible de déterminer le chemin de l'exécutable");
        }

        Console.WriteLine($"Installation du service: {displayName}");
        Console.WriteLine($"Chemin: {exePath}");

        // Créer le service en utilisant sc.exe
        var startInfo = new ProcessStartInfo
        {
            FileName = "sc.exe",
            Arguments =
                $"create \"{serviceName}\" binPath=\"{exePath}\" DisplayName=\"{displayName}\" start=auto",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        using (var process = Process.Start(startInfo))
        {
            if (process != null)
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    var error = process.StandardError.ReadToEnd();
                    throw new Exception($"Échec de la création du service: {error}");
                }
            }
        }

        // Configurer la description
        startInfo.Arguments = $"description \"{serviceName}\" \"{description}\"";
        using (var process = Process.Start(startInfo))
        {
            process?.WaitForExit();
        }

        // Configurer la récupération en cas d'échec
        startInfo.Arguments =
            $"failure \"{serviceName}\" reset=86400 actions=restart/60000/restart/60000/restart/60000";
        using (var process = Process.Start(startInfo))
        {
            process?.WaitForExit();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Service installé avec succès!");
        Console.WriteLine(
            $"✓ Le service '{displayName}' démarrera automatiquement au démarrage du système."
        );
        Console.ResetColor();

        Console.WriteLine("\nVoulez-vous démarrer le service maintenant? (O/N)");
        var key = Console.ReadKey();
        if (key.Key == ConsoleKey.O || key.Key == ConsoleKey.Y)
        {
            Console.WriteLine();
            StartServiceCommand();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nERREUR lors de l'installation: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nAppuyez sur une touche pour quitter...");
    Console.ReadKey();
}

static void UninstallService()
{
    EnsureAdministrator();
    const string serviceName = "EvoConnectServer";

    try
    {
        Console.WriteLine("Désinstallation du service...");
        UninstallServiceInternal(serviceName);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Service désinstallé avec succès!");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nERREUR lors de la désinstallation: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nAppuyez sur une touche pour quitter...");
    Console.ReadKey();
}

static void UninstallServiceInternal(string serviceName)
{
    var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == serviceName);

    if (service == null)
    {
        Console.WriteLine("Le service n'existe pas.");
        return;
    }

    // Arrêter le service s'il est en cours d'exécution
    if (service.Status != ServiceControllerStatus.Stopped)
    {
        Console.WriteLine("Arrêt du service...");
        service.Stop();
        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
        Thread.Sleep(2000);
    }

    // Supprimer le service
    var startInfo = new ProcessStartInfo
    {
        FileName = "sc.exe",
        Arguments = $"delete \"{serviceName}\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
    };

    using var process = Process.Start(startInfo);
    if (process != null)
    {
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new Exception($"Échec de la suppression: {error}");
        }
    }
}

static void StartServiceCommand()
{
    EnsureAdministrator();
    const string serviceName = "EvoConnectServer";

    try
    {
        var service = new ServiceController(serviceName);

        if (service.Status == ServiceControllerStatus.Running)
        {
            Console.WriteLine("Le service est déjà en cours d'exécution.");
        }
        else
        {
            Console.WriteLine("Démarrage du service...");
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Service démarré avec succès!");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERREUR: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nAppuyez sur une touche pour quitter...");
    Console.ReadKey();
}

static void StopServiceCommand()
{
    EnsureAdministrator();
    const string serviceName = "EvoConnectServer";

    try
    {
        var service = new ServiceController(serviceName);

        if (service.Status == ServiceControllerStatus.Stopped)
        {
            Console.WriteLine("Le service est déjà arrêté.");
        }
        else
        {
            Console.WriteLine("Arrêt du service...");
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Service arrêté avec succès!");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERREUR: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine("\nAppuyez sur une touche pour quitter...");
    Console.ReadKey();
}

static void ShowHelp()
{
    Console.WriteLine("EvoConnect Server - Gestion du service Windows");
    Console.WriteLine("==============================================\n");
    Console.WriteLine("Commandes disponibles:");
    Console.WriteLine("  --install-service    Installer le service Windows (nécessite admin)");
    Console.WriteLine("  --uninstall-service  Désinstaller le service Windows (nécessite admin)");
    Console.WriteLine("  --start-service      Démarrer le service");
    Console.WriteLine("  --stop-service       Arrêter le service");
    Console.WriteLine("  --help               Afficher cette aide");
    Console.WriteLine("\nExemples:");
    Console.WriteLine("  EvoConnect.Server.exe --install-service");
    Console.WriteLine("  EvoConnect.Server.exe --uninstall-service");
    Console.WriteLine("\nNote: Toutes les commandes nécessitent des privilèges administrateur.");
    Console.WriteLine("\nAppuyez sur une touche pour quitter...");
    Console.ReadKey();
}
