# Script para ejecutar migraciones de Entity Framework en todos los servicios
# Uso: .\setup-databases.ps1

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Configuracion de Bases de Datos" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Obtener la ruta del proyecto
$projectRoot = $PSScriptRoot

Write-Host "Verificando instalacion de dotnet-ef..." -ForegroundColor Yellow
$efVersion = dotnet ef --version 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Host "dotnet-ef no esta instalado. Instalando..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
} else {
    Write-Host "dotnet-ef ya esta instalado: $efVersion" -ForegroundColor Green
}

Write-Host ""

# Funcion para ejecutar migraciones
function Setup-Database {
    param(
        [string]$ServiceName,
        [string]$ServicePath,
        [int]$StepNumber
    )
    
    Write-Host "[$StepNumber/3] Configurando $ServiceName..." -ForegroundColor Cyan
    
    Push-Location $ServicePath
    
    # Eliminar migraciones existentes (si las hay)
    Write-Host "  - Limpiando migraciones antiguas..." -ForegroundColor Gray
    $migrations = Get-ChildItem -Path "Migrations" -ErrorAction SilentlyContinue
    if ($migrations) {
        dotnet ef database drop --force 2>$null
        dotnet ef migrations remove --force 2>$null
    }
    
    # Crear nueva migracion
    Write-Host "  - Creando migracion..." -ForegroundColor Gray
    $output = dotnet ef migrations add InitialCreate 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ERROR: No se pudo crear la migracion" -ForegroundColor Red
        Write-Host "  Detalle: $output" -ForegroundColor DarkGray
        Pop-Location
        return $false
    }
    
    # Aplicar migracion
    Write-Host "  - Aplicando migracion a la base de datos..." -ForegroundColor Gray
    $output = dotnet ef database update 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ERROR: No se pudo actualizar la base de datos" -ForegroundColor Red
        Write-Host "  Detalle: $output" -ForegroundColor DarkGray
        Pop-Location
        return $false
    }
    
    Write-Host "  OK $ServiceName configurado correctamente" -ForegroundColor Green
    Write-Host ""
    
    Pop-Location
    return $true
}

# Verificar conexión a PostgreSQL
Write-Host "Verificando PostgreSQL..." -ForegroundColor Yellow

# Intentar encontrar psql en rutas comunes
$psqlPaths = @(
    "C:\Program Files\PostgreSQL\16\bin\psql.exe",
    "C:\Program Files\PostgreSQL\15\bin\psql.exe",
    "C:\Program Files\PostgreSQL\14\bin\psql.exe",
    "C:\Program Files (x86)\PostgreSQL\16\bin\psql.exe",
    "C:\Program Files (x86)\PostgreSQL\15\bin\psql.exe"
)

$psqlPath = $null
foreach ($path in $psqlPaths) {
    if (Test-Path $path) {
        $psqlPath = $path
        break
    }
}

if ($psqlPath) {
    Write-Host "PostgreSQL encontrado en: $psqlPath" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "Creando bases de datos (si no existen)..." -ForegroundColor Yellow
    
    # Crear bases de datos
    $databases = @("auth_db", "messages_db", "groups_db")
    foreach ($db in $databases) {
        Write-Host "  - Creando base de datos: $db" -ForegroundColor Gray
        & $psqlPath -U postgres -c "CREATE DATABASE $db;" 2>$null | Out-Null
    }
} else {
    Write-Host "PostgreSQL (psql) no encontrado en rutas comunes" -ForegroundColor Yellow
    Write-Host "Las bases de datos se crearan automaticamente al ejecutar las migraciones" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "NOTA: Asegurate de que PostgreSQL este instalado y corriendo" -ForegroundColor Yellow
    Write-Host "      Las migraciones crearan las bases de datos automaticamente" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Ejecutando migraciones..." -ForegroundColor Yellow
Write-Host ""

# Ejecutar migraciones para cada servicio
$success = $true

$success = $success -and (Setup-Database -ServiceName "Auth Service" `
    -ServicePath (Join-Path $projectRoot "src\Services\Auth.Service") `
    -StepNumber 1)

$success = $success -and (Setup-Database -ServiceName "Messages Service" `
    -ServicePath (Join-Path $projectRoot "src\Services\Messages.Service") `
    -StepNumber 2)

$success = $success -and (Setup-Database -ServiceName "Groups Service" `
    -ServicePath (Join-Path $projectRoot "src\Services\Groups.Service") `
    -StepNumber 3)

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan

if ($success) {
    Write-Host "Configuracion completada exitosamente!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Ahora puedes ejecutar los servicios con:" -ForegroundColor Yellow
    Write-Host "  .\run-all-services.ps1" -ForegroundColor White
} else {
    Write-Host "Configuracion completada con errores" -ForegroundColor Red
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Revisa los mensajes de error anteriores" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Presiona cualquier tecla para salir..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
