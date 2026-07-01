# Aplica migraciones EF contra livriadb_experimental.
# Carga .env, arma la connection string (sin Database duplicado) y la pasa a dotnet ef.
#
#   .\migrate.ps1

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot
$EnvFile = Join-Path $Root ".env"

if (-not (Test-Path $EnvFile)) {
    Write-Error "Falta .env en $Root. Copia .env.example -> .env"
}

Get-Content $EnvFile | ForEach-Object {
    if ($_ -match '^\s*([^#=]+)=(.*)$') {
        $name = $matches[1].Trim()
        $value = $matches[2].Trim()
        Set-Item -Path "Env:$name" -Value $value
    }
}

$dbName = $env:ConnectionStrings__DbName
$baseConn = $env:ConnectionStrings__DefaultConnection

if ([string]::IsNullOrWhiteSpace($dbName)) {
    Write-Error "ConnectionStrings__DbName vacío en .env"
}

if ($dbName -eq "livriadb") {
    Write-Error @"
DbName es 'livriadb'. Este repo es experimental: usa livriadb_experimental.
  - En .env: ConnectionStrings__DbName=livriadb_experimental
  - DefaultConnection NO debe incluir Database=...
  - Limpia user-secrets: dotnet user-secrets clear --project LivriaBackend
"@
}

# Quitar Database/Initial Catalog existente (user-secrets suele traer Database=livriadb)
$parts = $baseConn -split ';' | Where-Object {
    $_ -and ($_ -notmatch '^\s*(database|initial\s*catalog)\s*=')
}
$cleanBase = ($parts -join ';').TrimEnd(';') + ';'
$finalConn = "${cleanBase}database=${dbName};"
if ($finalConn -notmatch '(?i)allow\s*user\s*variables\s*=') {
    $finalConn = "${finalConn}Allow User Variables=True;"
}

Write-Host "=== Migracion Livria experimental ===" -ForegroundColor Cyan
Write-Host "DbName: $dbName"
Write-Host "Base:   $cleanBase"
Write-Host ""

if ($dbName -ne "livriadb_experimental") {
    Write-Warning "DbName=$dbName (esperado: livriadb_experimental)"
}

Push-Location (Join-Path $Root "LivriaBackend")
try {
    dotnet ef database update `
        --project LivriaBackend.csproj `
        --connection $finalConn
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "OK: migraciones aplicadas en '$dbName'." -ForegroundColor Green
