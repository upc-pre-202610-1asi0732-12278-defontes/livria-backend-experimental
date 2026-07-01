# Diagnostica a que base de datos apuntarian migraciones y la app.
# Uso: .\scripts\verify-db-target.ps1

$ErrorActionPreference = "Stop"
$Root = Split-Path $PSScriptRoot -Parent
$EnvFile = Join-Path $Root ".env"

Write-Host "=== Verificacion BD Livria experimental ===" -ForegroundColor Cyan
Write-Host "Repo: $Root`n"

if (-not (Test-Path $EnvFile)) {
    Write-Error "Falta .env en $Root. Copia .env.example -> .env"
}

Get-Content $EnvFile | ForEach-Object {
    if ($_ -match '^\s*([^#=]+)=(.*)$') {
        Set-Item -Path "Env:$($matches[1].Trim())" -Value $matches[2].Trim()
    }
}

$dbName = $env:ConnectionStrings__DbName
$baseConn = $env:ConnectionStrings__DefaultConnection

if ([string]::IsNullOrWhiteSpace($dbName)) {
    Write-Error "ConnectionStrings__DbName vacio en .env"
}

$parts = $baseConn -split ';' | Where-Object {
    $_ -and ($_ -notmatch '^\s*(database|initial\s*catalog)\s*=')
}
$cleanBase = ($parts -join ';').TrimEnd(';') + ';'
$finalConn = "${cleanBase}database=${dbName};"

Write-Host "DbName (.env):           $dbName"
Write-Host "DefaultConnection (.env): $baseConn"
Write-Host "Cadena final:            $finalConn`n"

if ($baseConn -match '(?i)(database|initial\s*catalog)\s*=') {
    Write-Warning "DefaultConnection incluye nombre de BD. Se elimina al migrar, pero mejor quitarlo del .env/user-secrets."
}

if ($dbName -eq "livriadb") {
    Write-Error "DbName es 'livriadb'. Debe ser 'livriadb_experimental' en este repo."
}

Write-Host "--- User secrets ---" -ForegroundColor Yellow
Push-Location (Join-Path $Root "LivriaBackend")
try {
    dotnet user-secrets list 2>&1
}
finally {
    Pop-Location
}

Write-Host "`n--- dotnet ef (design-time) ---" -ForegroundColor Yellow
Push-Location (Join-Path $Root "LivriaBackend")
try {
    dotnet ef dbcontext info --project LivriaBackend.csproj 2>&1
}
finally {
    Pop-Location
}

Write-Host "`n--- Tablas __EFMigrationsHistory en MySQL ---" -ForegroundColor Yellow
if ($finalConn -match 'Port=(\d+)') { $port = $matches[1] } else { $port = "3306" }
if ($finalConn -match 'Password=([^;]+)') { $pass = $matches[1] } else { $pass = "" }

foreach ($db in @("livriadb", "livriadb_experimental")) {
    Write-Host "`nBD: $db (puerto $port)"
    $query = "SELECT COUNT(*) AS n FROM information_schema.tables WHERE table_schema='$db' AND table_name='__EFMigrationsHistory';"
    try {
        $result = mysql -h 127.0.0.1 -P $port -uroot "-p$pass" -N -e $query 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  (mysql CLI no disponible o error de conexion: $result)"
            break
        }
        if ($result -eq "1") {
            Write-Host "  -> TIENE __EFMigrationsHistory (migraciones EF aplicadas aqui)" -ForegroundColor Red
            mysql -h 127.0.0.1 -P $port -uroot "-p$pass" -e "SELECT MigrationId FROM ``$db``.__EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 3;" 2>$null
        } else {
            Write-Host "  -> sin historial de migraciones EF"
        }
    } catch {
        Write-Host "  mysql no instalado en PATH; omite chequeo directo."
        break
    }
}

Write-Host "`nSi las migraciones van a livriadb, revisa:" -ForegroundColor Green
Write-Host "  1. git pull (UserSecretsId nuevo + LivriaConnectionString)"
Write-Host "  2. .env con DbName=livriadb_experimental y DefaultConnection SIN Database="
Write-Host "  3. dotnet user-secrets clear --project LivriaBackend"
Write-Host "  4. Migrar solo con: .\migrate.ps1"
