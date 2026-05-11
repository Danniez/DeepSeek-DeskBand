<#
.SYNOPSIS
    编译 DeepSeek DeskBand 的 MSI 安装包
.DESCRIPTION
    需要 WiX Toolset v7 (https://wixtoolset.org)
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
# 检查 .NET SDK 是否安装
$dotnetTool = Get-Command "dotnet" -ErrorAction SilentlyContinue
if (-not $dotnetTool) {
    Write-Host "[ERROR] 未找到 .NET SDK，请先安装:" -ForegroundColor Red
    Write-Host "  https://dotnet.microsoft.com/download"
    exit 1
}

# ========== 安装/查找 WiX Toolset v7 ==========
function Get-WixExePath {
    $dotnetToolsPath = Join-Path $env:USERPROFILE ".dotnet\tools"
    $wixExe = Join-Path $dotnetToolsPath "wix.exe"
    if (Test-Path $wixExe) { return $wixExe }
    return $null
}

function Test-WixInstalled {
    $ver = wix --version 2>$null
    if ($LASTEXITCODE -eq 0 -and $ver) {
        Write-Host "  WiX Toolset: $ver" -ForegroundColor Green
        return $true
    }
    return $false
}

$wixInstalled = Test-WixInstalled

if (-not $wixInstalled) {
    Write-Host "[INFO] 正在安装 WiX Toolset v7..."
    dotnet tool install --global wix
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] WiX 安装失败，请手动安装:" -ForegroundColor Red
        Write-Host "  dotnet tool install --global wix"
        exit 1
    }
    $wixInstalled = Test-WixInstalled
    if (-not $wixInstalled) {
        Write-Host "[ERROR] WiX 安装验证失败，请手动安装:" -ForegroundColor Red
        Write-Host "  dotnet tool install --global wix"
        exit 1
    }
}

# ========== 接受 OSMF EULA（WiX v7 必需）==========
Write-Host "[INFO] 接受 WiX v7 OSMF EULA..."
wix eula accept wix7 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✅ EULA 已接受"
} else {
    Write-Host "  [WARN] EULA 接受命令未成功，编译时将通过 -acceptEula 参数处理" -ForegroundColor Yellow
}

# ========== 确保 WiX 扩展已缓存（强制匹配当前版本）==========
Write-Host "[INFO] 配置 WiX 扩展..."
# 获取 WiX 版本号
$wixVer = wix --version 2>$null
$wixVer = $wixVer -replace '\+.*$', ''  # 去掉 git hash，如 "7.0.0+b8977d6" → "7.0.0"
Write-Host "  WiX 版本: $wixVer"

# 移除旧的扩展缓存，避免版本冲突
wix extension remove WixToolset.UI.wixext -g 2>$null

# 添加与当前 WiX 版本匹配的 UI 扩展
wix extension add -g "WixToolset.UI.wixext/$wixVer" -acceptEula wix7
if ($LASTEXITCODE -ne 0) {
    # 降级：尝试不带版本号（自动选择兼容版本）
    Write-Host "  [WARN] 带版本号添加失败，尝试不带版本号..." -ForegroundColor Yellow
    wix extension add -g WixToolset.UI.wixext -acceptEula wix7
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] WixToolset.UI.wixext 扩展添加失败！" -ForegroundColor Red
        exit 1
    }
}

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

# WiX v7 编译（需要加载 UI 扩展以支持 WixUI_Minimal）
wix build "Product.wxs" `
    -o $msiOutput `
    -arch x64 `
    -ext WixToolset.UI.wixext `
    -acceptEula wix7

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
