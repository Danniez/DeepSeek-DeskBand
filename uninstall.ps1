<#
.SYNOPSIS
    DeepSeek DeskBand - Full Uninstall (PowerShell 7)
#>
param()

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "[INFO] Restarting as Administrator..."
    Start-Process pwsh -ArgumentList "-NoProfile -File `"$PSCommandPath`"" -Verb RunAs
    exit 0
}

$REGASM   = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"
$DLL_PATH = "$ScriptDir\bin\Release\net481\DeepSeekDeskBand.dll"
$CLSID    = "{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}"

Write-Host "============================================"
Write-Host "  DeepSeek DeskBand - Full Uninstall"
Write-Host "============================================"
Write-Host ""

# 1. Unregister COM
Write-Host "[1/4] Unregistering COM ..."
if (Test-Path $DLL_PATH) {
    & $REGASM $DLL_PATH /unregister /silent 2>$null
    Write-Host "        Unregistered: $DLL_PATH"
} else {
    Write-Host "        [skip] DLL not found, cleaning registry directly..."
}

# 2. Clean registry
Write-Host "[2/4] Cleaning registry ..."
$null = Remove-Item -Path "HKCR:\CLSID\$CLSID" -Recurse -Force -ErrorAction SilentlyContinue
$null = Remove-Item -Path "HKCR:\DeepSeek.DeskBand.1" -Recurse -Force -ErrorAction SilentlyContinue
$null = Remove-Item -Path "HKCR:\DeepSeek.DeskBand" -Recurse -Force -ErrorAction SilentlyContinue
$null = Remove-Item -Path "HKLM:\SOFTWARE\Classes\CLSID\$CLSID" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "        Registry cleaned"

# 3. Remove API Key from Credential Manager
Write-Host "[3/4] Removing saved API Key ..."
$result = cmdkey /delete:DeepSeekDeskBand:ApiKey 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "        API Key removed from Credential Manager"
} else {
    Write-Host "        [skip] No saved credential found"
}
$null = cmdkey /delete:LegacyGeneric:target=DeepSeekDeskBand:ApiKey 2>$null

Write-Host "  Removed:"
Write-Host "  - COM registration"
Write-Host "  - Registry entries"
Write-Host "  - API Key from Credential Manager"
Write-Host "============================================"

Write-Host "Restarting Explorer..."
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
