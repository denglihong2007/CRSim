$ErrorActionPreference = 'Stop'
$devShellVar = [Environment]::GetEnvironmentVariable("VSINSTALLDIR")
$msbuildPath = Get-Command MSBuild.exe -ErrorAction SilentlyContinue

if (-not $devShellVar -or -not $msbuildPath) {
    Write-Host "❌ 请在 Developer PowerShell for Visual Studio 中运行此脚本。" -ForegroundColor Red
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
    MSBuild.exe CRSim.csproj /t:Build /p:Configuration=Debug /p:Platform=x64 /v:q /clp:ErrorsOnly
}
catch {
    Write-Host "🔥 构建失败" -ForegroundColor Red
    return
}


Write-Host "🔧 正在设置开发环境变量…" -ForegroundColor Cyan

$baseOutDir = Join-Path $crSimRoot "bin\x64\Debug"
$exeFile = Get-ChildItem -Path $baseOutDir -Filter "CRSim.exe" -recurse | 
           Sort-Object LastWriteTime -Descending | 
           Select-Object -First 1

if ($null -eq $exeFile) {
    Write-Host "❌ 找不到构建生成的 CRSim.exe，请检查项目配置。" -ForegroundColor Red
    return
}

$fullExePath = $exeFile.FullName
$binDirectory = $exeFile.DirectoryName

[Environment]::SetEnvironmentVariable("CRSim_DebugBinaryFile", $fullExePath, 1)
[Environment]::SetEnvironmentVariable("CRSim_DebugBinaryDirectory", $binDirectory, 1)

Write-Host "✅ 已自动定位路径: $binDirectory" -ForegroundColor Gray
Write-Host "✨ 构建完成" -ForegroundColor Green
