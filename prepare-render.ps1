# Script para verificar que todo está listo para el despliegue en Render
Write-Host "Verificando preparación para despliegue en Render.com..." -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# Verificar archivos requeridos
Write-Host "Verificando archivos de configuración..." -ForegroundColor Yellow

$requiredFiles = @(
    "render.yaml",
    ".dockerignore",
    "src/Services/Auth.Service/Dockerfile",
    "src/Services/Messages.Service/Dockerfile",
    "src/Services/Groups.Service/Dockerfile",
    "src/UI/Dockerfile"
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "  OK $file" -ForegroundColor Green
    } else {
        Write-Host "  ERROR $file - FALTANTE" -ForegroundColor Red
        $allGood = $false
    }
}

Write-Host ""

# Verificar estado de Git
Write-Host "Verificando estado de Git..." -ForegroundColor Yellow

$gitStatus = git status --porcelain 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ADVERTENCIA: No se pudo verificar Git" -ForegroundColor Yellow
} elseif ($gitStatus) {
    Write-Host "  ADVERTENCIA: Hay cambios sin commitear" -ForegroundColor Yellow
    Write-Host "  Ejecuta: git add . ; git commit -m 'Preparar para despliegue' ; git push" -ForegroundColor Cyan
} else {
    Write-Host "  OK Todo commiteado" -ForegroundColor Green
}

Write-Host ""

# Resumen final
Write-Host "========================================" -ForegroundColor Cyan

if ($allGood) {
    Write-Host "LISTO PARA DESPLEGAR EN RENDER" -ForegroundColor Green
    Write-Host ""
    Write-Host "Proximos pasos:" -ForegroundColor Cyan
    Write-Host "  1. Push a GitHub: git push origin master" -ForegroundColor White
    Write-Host "  2. Ve a https://dashboard.render.com" -ForegroundColor White
    Write-Host "  3. Click en New + y luego Blueprint" -ForegroundColor White
    Write-Host "  4. Selecciona tu repositorio TPI-.NET" -ForegroundColor White
    Write-Host "  5. Click en Apply" -ForegroundColor White
    Write-Host ""
    Write-Host "Lee RENDER_DEPLOYMENT.md para instrucciones detalladas" -ForegroundColor Yellow
} else {
    Write-Host "PROBLEMAS DETECTADOS" -ForegroundColor Red
    Write-Host "Por favor, corrige los errores antes de desplegar" -ForegroundColor Yellow
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
