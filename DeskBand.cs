using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using DeepSeekDeskBand.Interop;

namespace DeepSeekDeskBand
{
    /// <summary>
    /// DeskBand 核心实现 —— 嵌入 Windows 任务栏的 AI 助手组件
    /// 实现 IDeskBand2 接口，作为任务栏工具栏显示在"资讯和兴趣"左侧
    /// </summary>
    [ComVisible(true)]
    [Guid(DeskBandGuids.ClsidDeskBand)]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId(DeskBandGuids.ProgId)]
    public partial class DeskBand :
        IDeskBand2,      // DeskBand 核心接口
        IObjectWithSite,  // 站点管理（获取任务栏引用）
        IInputObject,     // 输入焦点管理
        IPersistStream,   // 状态持久化
        IContextMenu      // 右键菜单
    {
        #region 私有字段

        private DeskBandControl? _deskBandControl;
        private IntPtr _sitePtr;
        private object? _site;
        private bool _isCompositionEnabled;
        private FlyoutWindow? _flyout;

        // 组件显示尺寸
        private const int BandMinWidth = 140;
        private const int BandMinHeight = 40;
        private const int BandMaxWidth = 300;
        private const int BandActualWidth = 180;
        private const int BandActualHeight = 40;

        #endregion

        #region 构造函数

        public DeskBand()
        {
            _deskBandControl = new DeskBandControl(this);
        }

        /// <summary>
        /// DeskBand 被加载到任务栏时调用，开始数据刷新
        /// </summary>
        private void OnBandLoaded()
        {
            _deskBandControl?.Start();
        }

        #endregion

        #region IOleWindow 实现

        public int GetWindow(out IntPtr phwnd)
        {
            if (_deskBandControl != null)
            {
                phwnd = _deskBandControl.Handle;
                return 0; // S_OK
            }
            phwnd = IntPtr.Zero;
            return unchecked((int)0x80004005); // E_FAIL
        }

        public int ContextSensitiveHelp(bool fEnterMode)
        {
            return 0; // S_OK
        }

        #endregion

        #region IDockingWindow 实现

        public int ShowDW(bool fShow)
        {
            if (_deskBandControl != null)
            {
                _deskBandControl.Visible = fShow;
                if (fShow) OnBandLoaded();
            }
            return 0; // S_OK
        }

        public int CloseDW(uint dwReserved)
        {
            CloseFlyout();
            if (_deskBandControl != null)
            {
                _deskBandControl.Dispose();
                _deskBandControl = null;
            }
            return 0; // S_OK
        }

        public int ResizeBorderDW(ref RECT prcBorder, object punkToolbarSite, bool fReserved)
        {
            return 0; // S_OK, 不需要调整边框
        }

        #endregion

        #region IDeskBand 实现

        public int GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO pdbi)
        {
            if ((pdbi.dwMask & (uint)DBIM.MINSIZE) != 0)
            {
                pdbi.ptMinSize.X = BandMinWidth;
                pdbi.ptMinSize.Y = BandMinHeight;
            }

            if ((pdbi.dwMask & (uint)DBIM.MAXSIZE) != 0)
            {
                pdbi.ptMaxSize.X = BandMaxWidth;
                pdbi.ptMaxSize.Y = BandActualHeight;
            }

            if ((pdbi.dwMask & (uint)DBIM.INTEGRAL) != 0)
            {
                pdbi.ptIntegral.X = 1;
                pdbi.ptIntegral.Y = 1;
            }

            if ((pdbi.dwMask & (uint)DBIM.ACTUAL) != 0)
            {
                pdbi.ptActual.X = BandActualWidth;
                pdbi.ptActual.Y = BandActualHeight;
            }

            if ((pdbi.dwMask & (uint)DBIM.TITLE) != 0)
            {
                pdbi.wszTitle = "";
            }

            if ((pdbi.dwMask & (uint)DBIM.MODEFLAGS) != 0)
            {
                pdbi.dwModeFlags = (DBIM)(DBIMF.VARIABLEHEIGHT | DBIMF.NORMAL | DBIMF.BKCOLOR);
            }

            if ((pdbi.dwMask & (uint)DBIM.BKCOLOR) != 0)
            {
                pdbi.crBkgnd = ColorTranslator.ToWin32(Color.FromArgb(32, 32, 32));
            }

            return 0; // S_OK
        }

        #endregion

        #region IDeskBand2 实现

        public int CanRenderComposited(out bool pfCanRenderComposited)
        {
            pfCanRenderComposited = true;
            return 0; // S_OK
        }

        public int SetCompositionState(bool fCompositionEnabled)
        {
            _isCompositionEnabled = fCompositionEnabled;
            return 0; // S_OK
        }

        public int GetCompositionState(out bool pfCompositionEnabled)
        {
            pfCompositionEnabled = _isCompositionEnabled;
            return 0; // S_OK
        }

        #endregion

        #region IObjectWithSite 实现

        public int SetSite(object pUnkSite)
        {
            _site = pUnkSite;
            if (pUnkSite != null)
            {
                _sitePtr = Marshal.GetIUnknownForObject(pUnkSite);
            }
            else
            {
                _sitePtr = IntPtr.Zero;
            }
            return 0; // S_OK
        }

        public int GetSite(ref Guid riid, out object ppvSite)
        {
            if (_site != null)
            {
                IntPtr pUnk = Marshal.GetIUnknownForObject(_site);
                try
                {
                    IntPtr pInterface;
                    int hr = Marshal.QueryInterface(pUnk, ref riid, out pInterface);
                    if (hr == 0)
                    {
                        ppvSite = Marshal.GetObjectForIUnknown(pInterface);
                        Marshal.Release(pInterface);
                        return 0;
                    }
                }
                finally
                {
                    Marshal.Release(pUnk);
                }
            }
            ppvSite = null!;
            return unchecked((int)0x80004002); // E_NOINTERFACE
        }

        #endregion

        #region IInputObject 实现

        public int UIActivateIO(bool fActivate, ref MSG pMsg)
        {
            if (fActivate && _deskBandControl != null)
            {
                _deskBandControl.Focus();
            }
            return 0; // S_OK
        }

        public int HasFocusIO()
        {
            if (_deskBandControl != null && _deskBandControl.ContainsFocus)
                return 1; // S_OK (has focus)
            return 0; // S_FALSE (no focus)
        }

        public int TranslateAcceleratorIO(ref MSG pMsg)
        {
            return 1; // S_FALSE - 不处理快捷键
        }

        #endregion

        #region IPersist 实现

        public int GetClassID(out Guid pClassID)
        {
            pClassID = new Guid(DeskBandGuids.ClsidDeskBand);
            return 0; // S_OK
        }

        #endregion

        #region IPersistStream 实现

        public int IsDirty()
        {
            return 1; // S_FALSE - 不需要保存状态
        }

        public int Load(IStream pStm)
        {
            return 0; // S_OK
        }

        public int Save(IStream pStm, bool fClearDirty)
        {
            return 0; // S_OK
        }

        public int GetSizeMax(out long pcbSize)
        {
            pcbSize = 0;
            return 0; // S_OK
        }

        #endregion

        #region IContextMenu 实现

        public int QueryContextMenu(IntPtr hmenu, uint iMenu, uint idCmdFirst, uint idCmdLast, uint uFlags)
        {
            const uint MF_STRING = 0x00000000;
            const uint MF_SEPARATOR = 0x00000800;

            int index = (int)iMenu;

            NativeMethods.InsertMenu(hmenu, index++, (uint)MF_STRING, idCmdFirst + 0, "刷新余额");
            NativeMethods.InsertMenu(hmenu, index++, (uint)MF_STRING, idCmdFirst + 1, "设置 API Key...");
            NativeMethods.InsertMenu(hmenu, index++, (uint)MF_SEPARATOR, 0, "");
            NativeMethods.InsertMenu(hmenu, index++, (uint)MF_STRING, idCmdFirst + 2, "查看详情");
            NativeMethods.InsertMenu(hmenu, index++, (uint)MF_SEPARATOR, 0, "");
            NativeMethods.InsertMenu(hmenu, index++, (uint)MF_STRING, idCmdFirst + 3, "隐藏");
            NativeMethods.InsertMenu(hmenu, index++, (uint)MF_STRING, idCmdFirst + 4, "关于");

            return index - (int)iMenu;
        }

        public int InvokeCommand(ref CMINVOKECOMMANDINFO pici)
        {
            int cmdId = (int)pici.lpVerb.ToInt64();
            switch (cmdId)
            {
                case 0: // 刷新余额
                    _ = _deskBandControl?.RefreshBalance();
                    break;
                case 1: // 设置 API Key
                    ShowApiKeyDialog();
                    break;
                case 2: // 查看余额详情
                    ShowFlyout(_deskBandControl?.CurrentBalance ?? BalanceResult.NoApiKey());
                    break;
                case 3: // 隐藏
                    HideDeskBand();
                    break;
                case 4: // 关于
                    ShowAbout();
                    break;
            }
            return 0; // S_OK
        }

        public int GetCommandString(IntPtr idCmd, uint uType, IntPtr pReserved, System.Text.StringBuilder pszName, uint cchMax)
        {
            return 0; // S_OK
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 显示余额详情 Flyout
        /// </summary>
        public void ShowFlyout(BalanceResult balance)
        {
            CloseFlyout();
            _flyout = new FlyoutWindow(this, balance);

            if (_deskBandControl != null)
            {
                Rectangle bandRect = _deskBandControl.RectangleToScreen(
                    _deskBandControl.ClientRectangle);
                _flyout.Location = new System.Drawing.Point(
                    bandRect.Left,
                    bandRect.Top - _flyout.Height);
            }

            _flyout.Show();
        }

        public void CloseFlyout()
        {
            if (_flyout != null)
            {
                _flyout.Close();
                _flyout.Dispose();
                _flyout = null;
            }
        }

        /// <summary>
        /// 获取 DeskBand 控件（供 FlyoutWindow 调用）
        /// </summary>
        public DeskBandControl? GetControl()
        {
            return _deskBandControl;
        }

        /// <summary>
        /// 弹出 API Key 设置对话框
        /// </summary>
        private void ShowApiKeyDialog()
        {
            using var dialog = new ApiKeyDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Key 已更新，重新刷新
                _ = _deskBandControl?.ReloadAndRefresh();
            }
        }

        private void ShowAbout()
        {
            System.Windows.MessageBox.Show(
                "DeepSeek DeskBand v1.0.0.0\n\n" +
                "功能：显示 DeepSeek API 可用余额\n" +
                "刷新：每 30 秒自动更新\n" +
                "存储：API Key 保存在 Windows 凭据管理器\n\n" +
                "技术栈：C# / .NET Framework 4.8 / COM DeskBand",
                "关于 DeepSeek DeskBand",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }

        private void HideDeskBand()
        {
            System.Windows.MessageBox.Show(
                "要隐藏 DeepSeek 助手：\n右键任务栏 → 工具栏 → 取消勾选 \"DeepSeek AI 助手\"",
                "提示",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }

        #endregion

        #region COM 注册

        /// <summary>
        /// 注册 COM 组件（安装时调用）
        /// </summary>
        [ComRegisterFunction]
        public static void Register(Type t)
        {
            string clsid = t.GUID.ToString("B").ToUpper();
            string progId = DeskBandGuids.ProgId;

            // 注册 CATID_DeskBand 组件类别
            // 这是关键步骤：让 Explorer 知道这是一个 DeskBand
            string key = $@"CLSID\{clsid}\Implemented Categories\{{00021492-0000-0000-C000-000000000046}}";
            Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(key);

            // 设置组件名称
            key = $@"CLSID\{clsid}";
            using (var regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(key, true))
            {
                regKey?.SetValue(string.Empty, DeskBandGuids.Title);
            }

            // 注册 ProgID
            key = $@"CLSID\{clsid}\ProgId";
            Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(key)?.SetValue(string.Empty, progId);
        }

        /// <summary>
        /// 注销 COM 组件（卸载时调用）
        /// </summary>
        [ComUnregisterFunction]
        public static void Unregister(Type t)
        {
            string clsid = t.GUID.ToString("B").ToUpper();
            string key = $@"CLSID\{clsid}\Implemented Categories\{{00021492-0000-0000-C000-000000000046}}";
            Microsoft.Win32.Registry.ClassesRoot.DeleteSubKey(key, false);
        }

        #endregion
    }

    /// <summary>
    /// 原生方法 P/Invoke
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool InsertMenu(
            IntPtr hMenu,
            int uPosition,
            uint uFlags,
            uint uIDNewItem,
            [MarshalAs(UnmanagedType.LPTStr)] string lpNewItem);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy,
            uint uFlags);

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;
    }
}
