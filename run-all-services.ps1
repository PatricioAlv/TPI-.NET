# Script para ejecutar todos los servicios del TPI
# Uso: .\run-all-services.ps1

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Sistema de Mensajería - TPI .NET" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Función para verificar si un puerto está en uso
function Test-Port {
    param($Port)
    $connection = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue
    return $connection.TcpTestSucceeded
}

# Verificar puertos
Write-Host "Verificando puertos disponibles..." -ForegroundColor Yellow
$ports = @(5001, 5002, 5003)
$portsInUse = @()

foreach ($port in $ports) {
    if (Test-Port -Port $port) {
        $portsInUse += $port
    }
}

if ($portsInUse.Count -gt 0) {
    Write-Host "ADVERTENCIA: Los siguientes puertos ya están en uso: $($portsInUse -join ', ')" -ForegroundColor Red
    $continue = Read-Host "¿Desea continuar de todos modos? (s/n)"
    if ($continue -ne 's') {
        exit
    }
}

Write-Host ""
Write-Host "Iniciando servicios..." -ForegroundColor Green
Write-Host ""

# Obtener la ruta del proyecto
$projectRoot = $PSScriptRoot

# Iniciar Auth Service
Write-Host "[1/3] Iniciando Auth Service (puerto 5001)..." -ForegroundColor Cyan
$authPath = Join-Path $projectRoot "src\Services\Auth.Service"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$authPath'; dotnet run" -WindowStyle Normal

Start-Sleep -Seconds 2

# Iniciar Messages Service
Write-Host "[2/3] Iniciando Messages Service (puerto 5002)..." -ForegroundColor Cyan
$messagesPath = Join-Path $projectRoot "src\Services\Messages.Service"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$messagesPath'; dotnet run" -WindowStyle Normal

Start-Sleep -Seconds 2

# Iniciar Groups Service
Write-Host "[3/3] Iniciando Groups Service (puerto 5003)..." -ForegroundColor Cyan
$groupsPath = Join-Path $projectRoot "src\Services\Groups.Service"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$groupsPath'; dotnet run" -WindowStyle Normal

Start-Sleep -Seconds 3

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Servicios iniciados correctamente!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "URLs de los servicios:" -ForegroundColor Yellow
Write-Host "  - Auth Service:     http://localhost:5001/swagger" -ForegroundColor White
Write-Host "  - Messages Service: http://localhost:5002/swagger" -ForegroundColor White
Write-Host "  - Groups Service:   http://localhost:5003/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Para abrir la UI, ejecuta:" -ForegroundColor Yellow
Write-Host "  start src\UI\wwwroot\index.html" -ForegroundColor White
Write-Host ""
Write-Host "Presiona Ctrl+C en cada ventana para detener los servicios" -ForegroundColor Yellow
Write-Host ""

# Preguntar si desea abrir la UI
$openUI = Read-Host "¿Desea abrir la interfaz de usuario ahora? (s/n)"
if ($openUI -eq 's') {
    $uiPath = Join-Path $projectRoot "src\UI\wwwroot\index.html"
    Start-Process $uiPath
}

Write-Host ""
Write-Host "Script completado. Presiona cualquier tecla para salir..." -ForegroundColor Green
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
