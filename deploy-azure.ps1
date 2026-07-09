# Deploy Livria backend a Azure App Service (Linux + .NET 8)
# Uso: .\deploy-azure.ps1

$ErrorActionPreference = "Stop"

$ResourceGroup = "livriaexpermientos"
$AppName = "livriaexperimental"
$Root = $PSScriptRoot
$Backend = Join-Path $Root "LivriaBackend"
$Publish = Join-Path $Backend "publish"
$Zip = Join-Path $Root "livria-deploy.zip"

Write-Host "=== Publish linux-x64 ===" -ForegroundColor Cyan
Push-Location $Backend
try {
    if (Test-Path $Publish) { Remove-Item $Publish -Recurse -Force }
    dotnet publish -c Release -r linux-x64 --self-contained false -o $Publish
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
    Pop-Location
}

Write-Host "=== ZIP (tar, rutas compatibles con Linux) ===" -ForegroundColor Cyan
if (Test-Path $Zip) { Remove-Item $Zip -Force }
Push-Location $Publish
try {
    tar -a -c -f $Zip *
}
finally {
    Pop-Location
}

Write-Host "=== Startup command ===" -ForegroundColor Cyan
az webapp config set `
    --resource-group $ResourceGroup `
    --name $AppName `
    --startup-file "dotnet LivriaBackend.dll"

Write-Host "=== Deploy ===" -ForegroundColor Cyan
az webapp deploy `
    --resource-group $ResourceGroup `
    --name $AppName `
    --src-path $Zip `
    --type zip `
    --clean true `
    --restart true

if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "OK: https://${AppName}-hccmatatddgvh9fg.chilecentral-01.azurewebsites.net/swagger" -ForegroundColor Green
