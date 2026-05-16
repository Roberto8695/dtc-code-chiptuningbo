param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$artifactsRoot = Join-Path $repoRoot "artifacts"
$publishRoot = Join-Path $artifactsRoot "publish"
$appPublishDir = Join-Path $publishRoot "app"
$setupPublishDir = Join-Path $publishRoot "setup"
$payloadZip = Join-Path $repoRoot "InstallerApp\Payload.zip"
$finalOutputDir = Join-Path $artifactsRoot "installer"

Write-Host "Limpiando salidas anteriores..."
Remove-Item $appPublishDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $setupPublishDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $finalOutputDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $payloadZip -Force -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Force -Path $appPublishDir | Out-Null
New-Item -ItemType Directory -Force -Path $setupPublishDir | Out-Null
New-Item -ItemType Directory -Force -Path $finalOutputDir | Out-Null

Write-Host "Publicando aplicacion principal..."
dotnet publish (Join-Path $repoRoot "DtcDesk.WinForms\DtcDesk.WinForms.csproj") `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -p:PublishSingleFile=false `
    -o $appPublishDir

Write-Host "Creando payload comprimido..."
Compress-Archive -Path (Join-Path $appPublishDir "*") -DestinationPath $payloadZip -CompressionLevel Optimal

Write-Host "Publicando instalador liviano..."
dotnet publish (Join-Path $repoRoot "InstallerApp\DTCDeskInstaller.csproj") `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -p:PublishSingleFile=true `
    -o $setupPublishDir

Copy-Item (Join-Path $setupPublishDir "DTCDesk_Setup_Liviano.exe") $finalOutputDir -Force

Write-Host ""
Write-Host "Instalador generado correctamente:"
Write-Host (Join-Path $finalOutputDir "DTCDesk_Setup_Liviano.exe")
