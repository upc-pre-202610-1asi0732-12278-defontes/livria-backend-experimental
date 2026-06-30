# Ejecuta seed-mock-data.sql contra MySQL experimental en Docker.
# Uso: .\scripts\seed-mock-data.ps1

$ErrorActionPreference = "Stop"

$ContainerName = "livria-mysql-experimental"
$DbName        = "livriadb_experimental"
$DbUser        = "root"
$DbPassword    = "livria_exp_root"

$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$SqlFile    = Join-Path $ScriptDir "seed-mock-data.sql"

if (-not (Test-Path $SqlFile)) {
    Write-Error "No se encontro $SqlFile"
}

$running = docker ps --filter "name=$ContainerName" --format "{{.Names}}" 2>$null
if (-not $running) {
    Write-Error "El contenedor '$ContainerName' no esta en ejecucion. Inicia MySQL con Docker primero."
}

Write-Host "Insertando datos mock en $DbName..." -ForegroundColor Cyan

Get-Content -Path $SqlFile -Raw -Encoding UTF8 | docker exec -i $ContainerName mysql "-u$DbUser" "-p$DbPassword" $DbName

if ($LASTEXITCODE -ne 0) {
    Write-Error "El seed fallo (exit code $LASTEXITCODE)."
}

Write-Host ""
Write-Host "Listo. Usuarios de prueba (password: 0000):" -ForegroundColor Green
Write-Host "  mock_alice  - communityplan, duena comunidad 201"
Write-Host "  mock_bob    - freeplan"
Write-Host "  mock_carol  - communityplan"
Write-Host ""
Write-Host "Datos: 4 libros, 1 comunidad, 3 posts, 5 comentarios, 2 resenas, carrito y notificaciones."
