# Iniciar todos los servicios backend en ventanas separadas

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Iniciando Backend TPI .NET" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = $PSScriptRoot

Write-Host "Iniciando Auth Service (Puerto 5001)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$projectRoot\src\Services\Auth.Service'; Write-Host 'AUTH SERVICE - Puerto 5001' -ForegroundColor Yellow; dotnet run"

Start-Sleep -Seconds 3

Write-Host "Iniciando Messages Service (Puerto 5002)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$projectRoot\src\Services\Messages.Service'; Write-Host 'MESSAGES SERVICE - Puerto 5002' -ForegroundColor Yellow; dotnet run"

Start-Sleep -Seconds 3

Write-Host "Iniciando Groups Service (Puerto 5003)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$projectRoot\src\Services\Groups.Service'; Write-Host 'GROUPS SERVICE - Puerto 5003' -ForegroundColor Yellow; dotnet run"

Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Servicios iniciados correctamente!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Servicios corriendo en:" -ForegroundColor Yellow
Write-Host "  - Auth Service:     http://localhost:5001" -ForegroundColor White
Write-Host "  - Messages Service: http://localhost:5002" -ForegroundColor White
Write-Host "  - Groups Service:   http://localhost:5003" -ForegroundColor White
Write-Host ""
Write-Host "Para exponer al internet, ejecuta en otra terminal:" -ForegroundColor Cyan
Write-Host "  ngrok start --all --config ngrok.yml" -ForegroundColor White
Write-Host ""
Write-Host "Para detener los servicios:" -ForegroundColor Yellow
Write-Host "  - Cierra cada ventana de PowerShell" -ForegroundColor White
Write-Host "  - O presiona Ctrl+C en cada una" -ForegroundColor White
Write-Host ""
