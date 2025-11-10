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

// Fichier de log pour déboguer les problèmes de démarrage
string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup.log");

void LogToFile(string message)
{
    try
    {
        File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
    }
    catch { }
}

try
{
    LogToFile("=== Démarrage de l'application ===");
    LogToFile($"Arguments: {string.Join(", ", args)}");
    LogToFile($"Working Directory: {Directory.GetCurrentDirectory()}");
    LogToFile($"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");

    // Vérifier les arguments de ligne de commande pour l'installation du service
    if (args.Length > 0)
    {
        var command = args[0].ToLower();
        LogToFile($"Commande détectée: {command}");

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

    LogToFile("Création du WebApplicationBuilder...");
    var builder = WebApplication.CreateBuilder(args);

    // Configurer comme service Windows
    builder.Host.UseWindowsService();
    LogToFile("Configuration Windows Service OK");

    // Configurer le logging
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    /*     // Ajouter le logging vers un fichier
        builder.Logging.(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "evoconnect-{Date}.log")
        );
     */
    builder.Logging.AddEventLog(settings =>
    {
        settings.SourceName = "EvoConnect";
    });

    LogToFile("Configuration du logging OK");

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.WebHost.UseUrls("http://0.0.0.0:6236");
    LogToFile("Configuration WebHost OK");

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

    LogToFile("Configuration des contrôleurs OK");

    LogToFile("Récupération de la chaîne de connexion...");
    var conn = AppData.EfConnectionString();
    LogToFile($"Chaîne de connexion: {conn?.Substring(0, Math.Min(50, conn?.Length ?? 0))}...");

    services.AddDbContext<ClinicDbContext>(
        options =>
        {
            options.UseFirebird(conn);
        },
        ServiceLifetime.Scoped
    );
    LogToFile("Configuration DbContext OK");

    LogToFile("Initialisation de la base de données KPI...");
    var initializer = new KpiDatabaseInitializer(conn);

    try
    {
        await initializer.InitializeAsync();
        LogToFile("Base de données KPI initialisée avec succès");
        Console.WriteLine("KPI database ready");
    }
    catch (Exception ex)
    {
        LogToFile($"ERREUR lors de l'initialisation KPI: {ex}");
        Console.WriteLine($"Failed to initialize KPI database: {ex.Message}");

        // Ne pas lancer l'exception si on est en mode service
        // Le service doit pouvoir démarrer même si KPI échoue
        if (!builder.Environment.IsProduction())
        {
            throw;
        }
    }

    LogToFile("Configuration JWT...");
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];

    if (string.IsNullOrEmpty(secretKey))
    {
        LogToFile("ATTENTION: SecretKey JWT non trouvée dans la configuration");
    }

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

    LogToFile("Configuration JWT OK");

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
        LogToFile("Mode serveur détecté - Configuration VipStats");
        builder.Services.AddScoped<VipStatsRefreshService>();
        builder.Services.AddHostedService<VipStatsBackgroundService>();
    }

    LogToFile("Construction de l'application...");
    var app = builder.Build();

    app.UseAuthentication();
    app.UseAuthorization();

    LogToFile("Configuration des fichiers statiques...");
    var imageDir = AppData.ImageDir();
    LogToFile($"Répertoire d'images: {imageDir}");

    if (!Directory.Exists(imageDir))
    {
        LogToFile($"Création du répertoire d'images: {imageDir}");
        Directory.CreateDirectory(imageDir);
    }

    app.UseStaticFiles(
        new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(imageDir),
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

    LogToFile("Application prête à démarrer");
    Console.WriteLine("EvoConnect Server starting...");

    app.Run();

    LogToFile("Application arrêtée normalement");
}
catch (Exception ex)
{
    LogToFile($"ERREUR FATALE: {ex}");
    Console.WriteLine($"FATAL ERROR: {ex.Message}");
    Console.WriteLine($"See log file: {logPath}");

    // Attendre un peu pour que les logs soient écrits
    Thread.Sleep(2000);

    // Re-lancer l'exception pour que le service s'arrête proprement
    throw;
}

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
                $"create \"{serviceName}\" binPath=\"\\\"{exePath}\\\"\" DisplayName=\"{displayName}\" start=auto",
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
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                Console.WriteLine($"Output: {output}");
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error: {error}");
                }

                if (process.ExitCode != 0)
                {
                    throw new Exception(
                        $"Échec de la création du service (code: {process.ExitCode}): {error}"
                    );
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
        Console.WriteLine("\nPour vérifier les logs en cas d'erreur:");
        Console.WriteLine(
            $"  - Fichier: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup.log")}"
        );
        Console.WriteLine("  - Event Viewer: Windows Logs → Application → Source: EvoConnect");
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
        Console.WriteLine($"\nDétails: {ex}");
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
        try
        {
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
        }
        catch
        {
            Console.WriteLine("Le service ne répond pas. Forçage de l'arrêt...");
        }
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
            Console.WriteLine("Vérifiez le fichier startup.log pour les détails du démarrage.");

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
        Console.WriteLine($"ERREUR lors du démarrage: {ex.Message}");
        Console.WriteLine("\nVérifiez les logs pour plus de détails:");
        Console.WriteLine(
            $"  - {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup.log")}"
        );
        Console.WriteLine("  - Event Viewer → Windows Logs → Application");
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
