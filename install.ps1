<#
.SYNOPSIS
    DeepSeek DeskBand - Build & Install (一键编译安装)
.DESCRIPTION
    自动完成：清理 → 生成强名称密钥 → 编译 → COM注册 → 重启任务栏
    编译部分无需管理员，COM 注册自动提权。
    Usage:  .\install.ps1
#>
param()

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# ====== 路径配置 ======
$REGASM   = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"
$SN_EXE   = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8.1 Tools\sn.exe"
$DLL_PATH = "$ScriptDir\bin\Release\net481\DeepSeekDeskBand.dll"
$CSPROJ   = "$ScriptDir\DeskBandWidget.csproj"

# ====== 查找 dotnet ======
$DOTNET = $null
$dotnetPaths = @(
    (Get-Command dotnet -ErrorAction SilentlyContinue).Source,
    "$env:ProgramFiles\dotnet\dotnet.exe",
    "${env:ProgramFiles(x86)}\dotnet\dotnet.exe",
    "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
)
try { $whereResult = & where.exe dotnet 2>$null; if ($whereResult) { $dotnetPaths += $whereResult } } catch {}
foreach ($p in $dotnetPaths) {
    if ($p -and (Test-Path $p)) { $DOTNET = $p; break }
}
if (-not $DOTNET) {
    Write-Host "[ERROR] dotnet not found." -ForegroundColor Red
    Write-Host "        Install .NET SDK or add dotnet to PATH." -ForegroundColor Yellow
    Pause; exit 1
}

Write-Host "============================================"
Write-Host "  DeepSeek DeskBand - Build & Install"
Write-Host "============================================"
Write-Host ""

# ========== Step 1: Clean ==========
Write-Host "[1/5] Cleaning ..." -NoNewline
if (Test-Path $DLL_PATH) {
    & $REGASM $DLL_PATH /unregister /silent 2>$null | Out-Null
    Start-Sleep -Milliseconds 500
}
$env:DOTNET_CLI_UI_LANGUAGE = 'en'
& $DOTNET clean $CSPROJ -c Release 2>&1 | Out-Null
Write-Host " OK" -ForegroundColor Green

# ========== Step 2: Check sn.exe ==========
Write-Host "[2/5] Checking sn.exe ..." -NoNewline
if (-not (Test-Path $SN_EXE)) {
    Write-Host " NOT FOUND" -ForegroundColor Red
    Write-Host "        Edit SN_EXE path in install.ps1" -ForegroundColor Yellow
    Pause; exit 1
}
Write-Host " OK" -ForegroundColor Green

# ========== Step 3: Generate strong name key ==========
if (-not (Test-Path "$ScriptDir\StrongName.snk")) {
    Write-Host "[3/5] Generating StrongName.snk ..."
    & $SN_EXE -k "$ScriptDir\StrongName.snk"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Key generation failed!" -ForegroundColor Red
        Pause; exit 1
    }
} else {
    Write-Host "[3/5] StrongName.snk exists, skip"
}

# ========== Step 4: Build ==========
Write-Host "[4/5] Building ..." -NoNewline
$buildResult = & $DOTNET build $CSPROJ -c Release 2>&1
Remove-Item Env:\DOTNET_CLI_UI_LANGUAGE -ErrorAction SilentlyContinue
if ($LASTEXITCODE -ne 0) {
    Write-Host " FAILED" -ForegroundColor Red
    Write-Host $buildResult
    Pause; exit 1
}
Write-Host " OK" -ForegroundColor Green

# ========== Step 5: Elevate & Register COM ==========
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "[5/5] Requesting Admin elevation for COM registration..."
    Start-Process pwsh -ArgumentList "-NoProfile -File `"$PSCommandPath`"" -Verb RunAs
    exit 0
}

# --- Admin section ---
Write-Host "[5/5] Registering COM component ..."
if (-not (Test-Path $DLL_PATH)) {
    Write-Host "[ERROR] DLL not found: $DLL_PATH" -ForegroundColor Red
    Pause; exit 1
}

& $REGASM $DLL_PATH /unregister /silent 2>$null
& $REGASM $DLL_PATH /codebase
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] COM registration failed!" -ForegroundColor Red
    Pause; exit 1
}

Write-Host ""
Write-Host "============================================"
Write-Host "  Install Complete!"
Write-Host "  Right-click taskbar > Toolbars > `"DeepSeek DeskBand`""
Write-Host "  Then right-click the widget > Set API Key"
Write-Host "============================================"

Write-Host "Restarting Explorer..."
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
