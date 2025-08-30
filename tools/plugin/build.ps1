$ErrorActionPreference = 'Stop'
$devShellVar = [Environment]::GetEnvironmentVariable("VSINSTALLDIR")
$msbuildPath = Get-Command MSBuild.exe -ErrorAction SilentlyContinue

if (-not $devShellVar -or -not $msbuildPath) {
    Write-Host "❌ 请在 Developer PowerShell for Visual Studio 2022 中运行此脚本。" -ForegroundColor Red
    exit 1
}
$scriptPath =  $MyInvocation.MyCommand.Definition
$crSimRoot = "$([System.IO.Path]::GetDirectoryName($scriptPath))\..\..\CRSim" 

function SetEnvironmentVariable {
    param (
        $Name, $Value
    )
    $out = "$Name = $Value"
    Write-Host $out -ForegroundColor DarkGray
    [Environment]::SetEnvironmentVariable($Name, $Value, 1)
}

Set-Location $crSimRoot


try {
    dotnet --version
    Write-Host "🔧 正在清理…" -ForegroundColor Cyan

    dotnet clean CRSim.csproj
    Write-Host "🔧 正在构建 CRSim，这可能需要 1-2 分钟。" -ForegroundColor Cyan
    MSBuild.exe CRSim.csproj /t:Build /p:Configuration=Debug /v:minimal
}
catch {
    Write-Host "🔥 构建失败" -ForegroundColor Red
    return
}


Write-Host "🔧 正在设置开发环境变量…" -ForegroundColor Cyan

[Environment]::SetEnvironmentVariable("CRSim_DebugBinaryFile", [System.IO.Path]::GetFullPath("${crSimRoot}\bin\Debug\net9.0-windows10.0.19041.0\CRSim.exe"), 1)
[Environment]::SetEnvironmentVariable("CRSim_DebugBinaryDirectory", [System.IO.Path]::GetFullPath("${crSimRoot}\bin\Debug\net9.0-windows10.0.19041.0\"), 1)

Write-Host "构建完成" -ForegroundColor Green
