# Vérifier les privilèges admin
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "Ce script doit être exécuté en tant qu'administrateur!"
    pause
    exit
}

$serviceName = "EvoConnectServer"

$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($service) {
    Write-Host "Arrêt du service..."
    Stop-Service -Name $serviceName -Force
    Start-Sleep -Seconds 2
    
    Write-Host "Suppression du service..."
    sc.exe delete $serviceName
    Write-Host "Service désinstallé avec succès!" -ForegroundColor Green
} else {
    Write-Host "Le service n'existe pas." -ForegroundColor Yellow
}

pause