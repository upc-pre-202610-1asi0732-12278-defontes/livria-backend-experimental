# Aplica migraciones EF contra livriadb_experimental (puerto 3307).
# Requiere: .env en la raíz, MySQL experimental levantado.
#
#   docker compose -f docker-compose.experimental.yml up -d
#   .\migrate.ps1

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot

if (-not (Test-Path (Join-Path $Root ".env"))) {
    Write-Error "Falta .env en $Root. Copia .env.example → .env"
}

Get-Content (Join-Path $Root ".env") | ForEach-Object {
    if ($_ -match '^\s*([^#=]+)=(.*)$') {
        $name = $matches[1].Trim()
        $value = $matches[2].Trim()
        Set-Item -Path "Env:$name" -Value $value
    }
}

$dbName = $env:ConnectionStrings__DbName
if ($dbName -ne "livriadb_experimental") {
    Write-Warning "ConnectionStrings__DbName=$dbName (esperado: livriadb_experimental)"
}

Write-Host "Migrando hacia: $dbName en $($env:ConnectionStrings__DefaultConnection)" -ForegroundColor Cyan

Push-Location (Join-Path $Root "LivriaBackend")
try {
    dotnet ef database update --project LivriaBackend.csproj
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
    Pop-Location
}

Write-Host "Migraciones OK en livriadb_experimental." -ForegroundColor Green
