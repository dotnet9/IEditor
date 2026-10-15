# 统一发布入口：scripts/publish.ps1
# 用法：pwsh scripts/publish.ps1 [-RuntimeIdentifier win-x64] [-Version 0.0.0]
# 输出：artifacts/publish/<rid>/<AppName>/
[CmdletBinding()]
param(
    [string] $RuntimeIdentifier = "win-x64",
    [string] $Version = ""
)

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path

if ([string]::IsNullOrWhiteSpace($Version)) {
    [xml] $project = Get-Content -LiteralPath (Join-Path $repositoryRoot 'src/IEditor.App/IEditor.App.csproj')
    $Version = ($project.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1)
}
if ($Version -notmatch '^\d+(\.\d+){1,3}$') { throw "Invalid version: $Version" }
Write-Host "发布 $RuntimeIdentifier (Version=$Version)"

# 全平台 NativeAOT：完整反射元数据保全，单线程 ILC 更稳
dotnet publish (Join-Path $repositoryRoot "src/IEditor.App/IEditor.App.csproj") -c Release -r $RuntimeIdentifier --self-contained true -p:Version=$Version -p:DebugType=portable -p:DebugSymbols=true -p:PublishAot=true -p:PublishTrimmed=true -p:PublishSingleFile=false -p:IlcGenerateCompleteTypeMetadata=true -p:IlcTrimMetadata=false -p:IlcSingleThreaded=true -o (Join-Path $repositoryRoot "artifacts/publish/$RuntimeIdentifier/IEditor.App")
if ($LASTEXITCODE -ne 0) { throw "publish failed for $RuntimeIdentifier" }
