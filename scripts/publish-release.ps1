[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$releasesDir = Join-Path $repoRoot "releases"
$coreProject = Join-Path $repoRoot "src\AutoRefreshHDR\AutoRefreshHDR.csproj"
$editorProject = Join-Path $repoRoot "src\ConfigEditor\ConfigEditor.csproj"

function Read-YesNo {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Prompt
    )

    while ($true) {
        $answer = (Read-Host "$Prompt (S/N)").Trim().ToUpperInvariant()
        if ($answer -in @("S", "SIM", "Y", "YES")) { return $true }
        if ($answer -in @("N", "NAO", "NÃO", "NO")) { return $false }
        Write-Host "Resposta invalida. Digite S para Sim ou N para Nao." -ForegroundColor Yellow
    }
}

function Read-ValidVersion {
    while ($true) {
        $inputVersion = (Read-Host "Informe o numero da release (formato x.x.x, ex: 1.0.0)").Trim()
        if ($inputVersion -match '^\d+\.\d+\.\d+$') {
            return $inputVersion
        }
        Write-Host "Formato invalido. Utilize o padrao semantico x.x.x (apenas numeros)." -ForegroundColor Yellow
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AutoRefreshHDR - Gerador de Releases  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $releasesDir)) {
    Write-Host "Pasta 'releases' nao encontrada. Criando em: $releasesDir" -ForegroundColor DarkGray
    New-Item -ItemType Directory -Path $releasesDir | Out-Null
}

$versionFolders = Get-ChildItem -Path $releasesDir -Directory |
    Where-Object { $_.Name -match '^\d+\.\d+\.\d+$' } |
    Sort-Object { [version]$_.Name }

if ($versionFolders.Count -eq 0) {
    $suggestedVersion = "1.0.0"
    Write-Host "Nenhuma versao encontrada em 'releases'." -ForegroundColor White
    Write-Host "A versao a ser criada sera: $suggestedVersion" -ForegroundColor Green
}
else {
    $latestVersion = [version]($versionFolders[-1].Name)
    $suggestedVersion = "$($latestVersion.Major).$($latestVersion.Minor).$($latestVersion.Build + 1)"
    Write-Host "Versao atual mais recente: $latestVersion" -ForegroundColor White
    Write-Host "Proxima versao sugerida (patch +1): $suggestedVersion" -ForegroundColor Green
}

Write-Host ""
if (Read-YesNo -Prompt "Deseja prosseguir com a publicacao da versao $suggestedVersion?") {
    $targetVersion = $suggestedVersion
}
else {
    $targetVersion = Read-ValidVersion
}

$targetDir = Join-Path $releasesDir $targetVersion

if (Test-Path $targetDir) {
    Write-Host ""
    Write-Host "ATENCAO: A pasta da versao '$targetVersion' ja existe em:" -ForegroundColor Yellow
    Write-Host "  $targetDir" -ForegroundColor Yellow

    if (-not (Read-YesNo -Prompt "Deseja deletar todo o conteudo dessa pasta e republicar a versao $targetVersion?")) {
        Write-Host "Publicacao cancelada pelo usuario." -ForegroundColor Yellow
        exit 0
    }

    Write-Host "Limpando conteudo existente em $targetDir..." -ForegroundColor DarkGray
    Get-ChildItem -Path $targetDir -Force | Remove-Item -Recurse -Force
}
else {
    New-Item -ItemType Directory -Path $targetDir | Out-Null
}

Write-Host ""
Write-Host "Publicando AutoRefreshHDR (win-x64, framework-dependent, single-file, ReadyToRun)..." -ForegroundColor Cyan
& dotnet publish $coreProject -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:DebugType=None -o $targetDir
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao publicar o projeto AutoRefreshHDR." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Publicando ConfigEditor (win-x64, framework-dependent, single-file, ReadyToRun)..." -ForegroundColor Cyan
& dotnet publish $editorProject -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:DebugType=None -o $targetDir
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao publicar o projeto ConfigEditor." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Release $targetVersion publicada com sucesso!" -ForegroundColor Green
Write-Host "  Destino: $targetDir" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
