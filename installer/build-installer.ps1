<#
.SYNOPSIS
    编译 DeepSeek DeskBand 的 MSI 安装包
.DESCRIPTION
    需要 WiX Toolset v5 (https://wixtoolset.org)
    需要 .NET Framework 4.8.1 SDK
.USAGE
    .\build-installer.ps1
#>
param()

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "============================================"
Write-Host "  DeepSeek DeskBand - Build MSI Installer"
Write-Host "============================================"
Write-Host ""

# ========== 前置检查 ==========
# 检查 WiX 是否安装
$wixToolset = Get-Command "dotnet" -ErrorAction SilentlyContinue
if (-not $wixToolset) {
    Write-Host "[ERROR] 未找到 .NET SDK，请先安装:" -ForegroundColor Red
    Write-Host "  https://dotnet.microsoft.com/download"
    exit 1
}

$wixInstalled = dotnet wix --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[INFO] 正在安装 WiX Toolset..."
    dotnet tool install --global wix --version 5.*
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] WiX 安装失败，请手动安装:" -ForegroundColor Red
        Write-Host "  dotnet tool install --global wix"
        exit 1
    }
    Write-Host "  WiX 安装成功"
}

$wixVersion = dotnet wix --version
Write-Host "  WiX Toolset: $wixVersion"

# ========== 编译 DeskBand DLL ==========
Write-Host "[1/3] 编译 DeskBand DLL..."
Set-Location $ProjectRoot
dotnet build -c Release --nologo
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] 编译失败！" -ForegroundColor Red
    exit 1
}
Write-Host "  编译完成"

# ========== 编译 MSI ==========
Write-Host "[2/3] 编译 MSI 安装包..."
Set-Location $ScriptDir

# 清理旧产物
$msiOutput = "DeepSeekDeskBand.msi"
if (Test-Path $msiOutput) { Remove-Item $msiOutput -Force }

# WiX 编译
dotnet wix build "Product.wxs" `
    -ext WixToolset.Netfx `
    -o $msiOutput `
    -arch x64

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] MSI 编译失败！" -ForegroundColor Red
    exit 1
}

Write-Host "  MSI 生成完成"

# ========== 验证 ==========
Write-Host "[3/3] 验证 MSI..."
$msiPath = Join-Path $ScriptDir $msiOutput
if (Test-Path $msiPath) {
    $size = (Get-Item $msiPath).Length
    Write-Host "  ✅ MSI 安装包已生成:"
    Write-Host "     路径: $msiPath"
    Write-Host "     大小: $('{0:N0}' -f $size) 字节"
} else {
    Write-Host "[ERROR] MSI 未找到！" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "============================================"
Write-Host "  构建完成！"
Write-Host "============================================"
Write-Host ""
Write-Host "安装方法："
Write-Host "  1. 右键 DeepSeekDeskBand.msi → 安装"
Write-Host "  2. 右键任务栏 → 工具栏 → 勾选 DeepSeek DeskBand"
Write-Host ""
Write-Host "卸载方法："
Write-Host "  设置 → 应用 → 应用和功能 → DeepSeek DeskBand → 卸载"
Write-Host "  (API Key 需手动清除：cmdkey /delete:DeepSeekDeskBand:ApiKey)"
Write-Host ""
