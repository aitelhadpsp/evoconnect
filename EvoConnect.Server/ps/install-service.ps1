# Vérifier les privilèges admin
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "Ce script doit être exécuté en tant qu'administrateur!"
    pause
    exit
}

$serviceName = "EvoConnectServer"
$displayName = "EvoConnect Server"
$description = "Service de gestion EvoConnect pour cliniques dentaires"
$exePath = Join-Path $PSScriptRoot "EvoConnect.Server.exe"

# Vérifier si le fichier exe existe
if (-not (Test-Path $exePath)) {
    Write-Error "L'exécutable n'existe pas: $exePath"
    pause
    exit
}

# Arrêter et supprimer le service s'il existe déjà
$existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Arrêt du service existant..."
    Stop-Service -Name $serviceName -Force
    Start-Sleep -Seconds 2
    
    Write-Host "Suppression du service existant..."
    sc.exe delete $serviceName
    Start-Sleep -Seconds 2
}

# Créer le nouveau service
Write-Host "Installation du service $displayName..."
New-Service -Name $serviceName `
    -BinaryPathName $exePath `
    -DisplayName $displayName `
    -Description $description `
    -StartupType Automatic

# Configurer la récupération en cas d'échec
Write-Host "Configuration de la récupération automatique..."
sc.exe failure $serviceName reset= 86400 actions= restart/60000/restart/60000/restart/60000

# Démarrer le service
Write-Host "Démarrage du service..."
Start-Service -Name $serviceName

# Vérifier l'état
Start-Sleep -Seconds 3
$service = Get-Service -Name $serviceName
Write-Host "`nÉtat du service: $($service.Status)" -ForegroundColor Green
Write-Host "Le service démarrera automatiquement au démarrage du système." -ForegroundColor Green

pause