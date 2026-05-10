using System;
using System.Runtime.InteropServices;

namespace DeepSeekDeskBand.Interop
{
    /// <summary>
    /// COM 接口定义 - Windows Shell DeskBand 所需的所有接口
    /// </summary>

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000114-0000-0000-C000-000000000046")]
    public interface IOleWindow
    {
        [PreserveSig]
        int GetWindow(out IntPtr phwnd);

        [PreserveSig]
        int ContextSensitiveHelp([MarshalAs(UnmanagedType.Bool)] bool fEnterMode);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("012DD920-7B26-11D0-8CA9-00A0C92DBFE8")]
    public interface IDockingWindow : IOleWindow
    {
        [PreserveSig]
        new int GetWindow(out IntPtr phwnd);

        [PreserveSig]
        new int ContextSensitiveHelp([MarshalAs(UnmanagedType.Bool)] bool fEnterMode);

        [PreserveSig]
        int ShowDW([MarshalAs(UnmanagedType.Bool)] bool fShow);

        [PreserveSig]
        int CloseDW([MarshalAs(UnmanagedType.U4)] uint dwReserved);

        [PreserveSig]
        int ResizeBorderDW(
            ref RECT prcBorder,
            [MarshalAs(UnmanagedType.IUnknown)] object punkToolbarSite,
            [MarshalAs(UnmanagedType.Bool)] bool fReserved);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EB0FE172-1A3A-11D0-89B3-00A0C90A90AC")]
    public interface IDeskBand : IDockingWindow
    {
        [PreserveSig]
        new int GetWindow(out IntPtr phwnd);

        [PreserveSig]
        new int ContextSensitiveHelp([MarshalAs(UnmanagedType.Bool)] bool fEnterMode);

        [PreserveSig]
        new int ShowDW([MarshalAs(UnmanagedType.Bool)] bool fShow);

        [PreserveSig]
        new int CloseDW([MarshalAs(UnmanagedType.U4)] uint dwReserved);

        [PreserveSig]
        new int ResizeBorderDW(
            ref RECT prcBorder,
            [MarshalAs(UnmanagedType.IUnknown)] object punkToolbarSite,
            [MarshalAs(UnmanagedType.Bool)] bool fReserved);

        [PreserveSig]
        int GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO pdbi);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79D16DE4-ABEE-4021-8D9D-9169B261D657")]
    public interface IDeskBand2 : IDeskBand
    {
        [PreserveSig]
        new int GetWindow(out IntPtr phwnd);

        [PreserveSig]
        new int ContextSensitiveHelp([MarshalAs(UnmanagedType.Bool)] bool fEnterMode);

        [PreserveSig]
        new int ShowDW([MarshalAs(UnmanagedType.Bool)] bool fShow);

        [PreserveSig]
        new int CloseDW([MarshalAs(UnmanagedType.U4)] uint dwReserved);

        [PreserveSig]
        new int ResizeBorderDW(
            ref RECT prcBorder,
            [MarshalAs(UnmanagedType.IUnknown)] object punkToolbarSite,
            [MarshalAs(UnmanagedType.Bool)] bool fReserved);

        [PreserveSig]
        new int GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO pdbi);

        [PreserveSig]
        int CanRenderComposited(out bool pfCanRenderComposited);

        [PreserveSig]
        int SetCompositionState([MarshalAs(UnmanagedType.Bool)] bool fCompositionEnabled);

        [PreserveSig]
        int GetCompositionState(out bool pfCompositionEnabled);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("FC4801A3-2BA9-11CF-A229-00AA003D7352")]
    public interface IObjectWithSite
    {
        [PreserveSig]
        int SetSite([MarshalAs(UnmanagedType.IUnknown)] object pUnkSite);

        [PreserveSig]
        int GetSite(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvSite);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("68284faa-6a48-11d0-8c78-00c04fd918b4")]
    public interface IInputObject
    {
        [PreserveSig]
        int UIActivateIO([MarshalAs(UnmanagedType.Bool)] bool fActivate, ref MSG pMsg);

        [PreserveSig]
        int HasFocusIO();

        [PreserveSig]
        int TranslateAcceleratorIO(ref MSG pMsg);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010c-0000-0000-C000-000000000046")]
    public interface IPersist
    {
        [PreserveSig]
        int GetClassID(out Guid pClassID);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000109-0000-0000-C000-000000000046")]
    public interface IPersistStream : IPersist
    {
        [PreserveSig]
        new int GetClassID(out Guid pClassID);

        [PreserveSig]
        int IsDirty();

        [PreserveSig]
        int Load([In] System.Runtime.InteropServices.ComTypes.IStream pStm);

        [PreserveSig]
        int Save([In] System.Runtime.InteropServices.ComTypes.IStream pStm, [MarshalAs(UnmanagedType.Bool)] bool fClearDirty);

        [PreserveSig]
        int GetSizeMax(out long pcbSize);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("4CB43E46-865A-4AD0-838B-14039285B627")]
    public interface IContextMenu
    {
        [PreserveSig]
        int QueryContextMenu(
            IntPtr hmenu,
            uint iMenu,
            uint idCmdFirst,
            uint idCmdLast,
            uint uFlags);

        [PreserveSig]
        int InvokeCommand(ref CMINVOKECOMMANDINFO pici);

        [PreserveSig]
        int GetCommandString(
            IntPtr idCmd,
            uint uType,
            IntPtr pReserved,
            System.Text.StringBuilder pszName,
            uint cchMax);
    }

    // ==================== 结构体 ====================

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr HWnd;
        public uint Message;
        public IntPtr WParam;
        public IntPtr LParam;
        public uint Time;
        public POINT Pt;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DESKBANDINFO
    {
        public uint dwMask;
        public POINT ptMinSize;
        public POINT ptMaxSize;
        public POINT ptIntegral;
        public POINT ptActual;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string wszTitle;
        public DBIM dwModeFlags;
        public int crBkgnd;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CMINVOKECOMMANDINFO
    {
        public uint cbSize;
        public uint fMask;
        public IntPtr hwnd;
        public IntPtr lpVerb;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpParameters;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpDirectory;
        public int nShow;
        public uint dwHotKey;
        public IntPtr hIcon;
    }

    // ==================== 枚举/常量 ====================

    [Flags]
    public enum DBIM : uint
    {
        MINSIZE = 0x0001,
        MAXSIZE = 0x0002,
        INTEGRAL = 0x0004,
        ACTUAL = 0x0008,
        TITLE = 0x0010,
        MODEFLAGS = 0x0020,
        BKCOLOR = 0x0040
    }

    [Flags]
    public enum DBIMF : uint
    {
        NORMAL = 0x0000,
        FIXED = 0x0001,
        FIXEDBMP = 0x0004,
        VARIABLEHEIGHT = 0x0008,
        UNDELETEABLE = 0x0010,
        DEBOSSED = 0x0020,
        BKCOLOR = 0x0040,
        USECHEVRON = 0x0080,
        BREAK = 0x0100,
        ADDTOFRONT = 0x0200,
        TOPALIGN = 0x0400,
        NOGRIPPER = 0x0800,
        ALWAYSGRIPPER = 0x1000,
        NOMARGINS = 0x2000
    }

    /// <summary>
    /// DeskBand 注册用的 CLSID
    /// </summary>
    public static class DeskBandGuids
    {
        public const string ClsidDeskBand = "A1B2C3D4-E5F6-7890-ABCD-EF1234567890";
        public const string ClsidDeskBandClass = "DeepSeek.DeskBand";
        public const string ProgId = "DeepSeek.DeskBand.1";
        public const string Title = "DeepSeek DeskBand";
        public const uint BandID = 0xDE00;
    }
}
