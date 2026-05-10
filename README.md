# DeepSeek DeskBand

> **English** · [README.en.md](./README.en.md)
> 
> 本项目所有代码均由 Github Copilot 使用 DeepSeek-V4 模型生成。本人仅进行极少量的 UI 界面数值以及字符串调整，本行是项目中唯一人工输入的文字。

> **Windows 任务栏余额显示器** —— 在 Windows 任务栏实时显示 DeepSeek API 可用余额

## 一键编译安装

```powershell
# 在 Windows 上打开 PowerShell 7（管理员），进入项目目录：
cd D:\Project\deepseek-usage\DeskBandWidget
.\install.ps1
```

`install.ps1` 会自动完成：清理 → 生成密钥 → 编译 → COM注册 → 重启任务栏。

安装后右键任务栏 → **工具栏** → 勾选 **"DeepSeek DeskBand"**，然后右键组件 → 设置 API Key 即可。

---

## 彻底卸载

```powershell
# 管理员身份运行：
.\uninstall.ps1
```

`uninstall.ps1` 会清除 **全部 4 项**：

| 步骤 | 清除内容 |
|------|----------|
| COM 注销 | `RegAsm /unregister` |
| 注册表清理 | `HKCR\CLSID\{...}` 及 ProgID |
| **API Key 清除** | Windows 凭据管理器中的 `DeepSeekDeskBand:ApiKey` |
| 任务栏刷新 | 重启 Explorer |

> **!** 光删文件不会卸载！必须用 `uninstall.ps1` 才能清除 COM 注册和凭据。

---

## MSI 安装包（推荐）

一键安装，支持在 **设置 → 应用 → 应用和功能** 中卸载。

### 编译 MSI

```powershell
# 需要 WiX Toolset，脚本会自动安装
.\installer\build-installer.ps1
```

产物：`installer\DeepSeekDeskBand.msi`

### 安装

1. 双击 `DeepSeekDeskBand.msi` → 下一步 → 安装
2. 右键任务栏 → **工具栏** → 勾选 **"DeepSeek DeskBand"**
3. 右键组件 → **设置 API Key**

### 卸载

**设置 → 应用 → 应用和功能 → DeepSeek DeskBand → 卸载**

> **!** 卸载时会自动清除凭据管理器中的 API Key，但以防万一你也可以手动确认：
> ```batch
> cmdkey /list | findstr DeepSeek
> ```

### MSI 的优势

| 特性 | MSI 安装包 | install.ps1 |
|------|-----------|-------------|
| 安装后出现在"应用和功能" | 是 | 否 |
| 卸载时自动清理 COM 注册 | 是 | 是 |
| 卸载时自动清理凭据 | 是 | 是 |
| 支持静默/批量部署 | 是 (`msiexec /i`) | 否 |
| 支持组策略分发 | 是 | 否 |

---

## 手动编译（不用 install.ps1 / MSI）

```batch
# 1. 生成强名称密钥（仅一次）
sn -k StrongName.snk

# 2. 编译
dotnet build -c Release

# 3. 管理员身份手动注册
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe bin\Release\net481\DeepSeekDeskBand.dll /codebase

# 4. 重启 Explorer
taskkill /f /im explorer.exe && start explorer.exe
```

手动卸载：
```batch
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe bin\Release\net481\DeepSeekDeskBand.dll /unregister
cmdkey /delete:DeepSeekDeskBand:ApiKey
taskkill /f /im explorer.exe && start explorer.exe
```

---

## 前置依赖

| 依赖 | 下载 |
|------|------|
| .NET Framework 4.8.1 SDK | https://dotnet.microsoft.com/download/dotnet-framework/net481 |
| （可选）Visual Studio 2022 | https://visualstudio.microsoft.com |

---

## 设置 API Key

1. 任务栏右键组件 → **设置 API Key...**
2. 输入 DeepSeek API Key（`sk-...`）
3. 点 **测试连接** 验证
4. 点 **保存** → Key 存入 Windows 凭据管理器（加密存储）

## 功能一览

| 功能 | 说明 |
|------|------|
| 余额显示 | 任务栏实时显示 `xx.xx` |
| 自动刷新 | 每 30 秒自动查询 |
| 状态灯 | 绿=正常 / 黄=未配置 / 红=错误 |
| 右键菜单 | 刷新余额 / 设置 Key / 查看详情 / 关于 |
| 详情面板 | 点击弹出：总余额 / 充值余额 / 赠送余额 |
| 安全存储 | API Key 存 Windows Credential Manager，卸载自动清除 |

## 任务栏效果

```
┌───────────────────────────────────────────────┐
│ DeepSeek      资讯和兴趣  ...                  │
│ 82.50  ●                                      │
└───────────────────────────────────────────────┘
          ↑ 绿色=正常 黄色=未配 红色=出错
```

