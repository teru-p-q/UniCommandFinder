using System;
using System.Runtime.InteropServices;
using System.Text;

public static class Win32
{
    public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

    [DllImport("user32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public extern static bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);
    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32")]
    public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [Flags]
    public enum MIIM
    {
        BITMAP = 0x00000080,
        CHECKMARKS = 0x00000008,
        DATA = 0x00000020,
        FTYPE = 0x00000100,
        ID = 0x00000002,
        STATE = 0x00000001,
        STRING = 0x00000040,
        SUBMENU = 0x00000004,
        TYPE = 0x00000010
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MENUITEMINFO
    {
        public Int32 cbSize = Marshal.SizeOf(typeof(MENUITEMINFO));
        public MIIM fMask;
        public UInt32 fType;
        public UInt32 fState;
        public UInt32 wID;
        public IntPtr hSubMenu;
        public IntPtr hbmpChecked;
        public IntPtr hbmpUnchecked;
        public IntPtr dwItemData;
        public string dwTypeData = null;
        public UInt32 cch; // length of dwTypeData
        public IntPtr hbmpItem;

        public MENUITEMINFO() { }
    }

    internal const UInt32 MF_BYCOMMAND = 0x00000000;
    internal const UInt32 MF_BYPOSITION = 0x00000400;

    internal const UInt32 MFT_SEPARATOR = 0x00000800;
    internal const UInt32 MFT_STRING = 0x00000000;

    [DllImport("user32.dll")]
    public static extern IntPtr GetMenu(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);
    [DllImport("user32.dll")]
    public static extern int GetMenuString(IntPtr hMenu, uint uIDItem, StringBuilder lpString, int nMaxCount, uint uFlag);
    [DllImport("user32.dll")]
    public static extern int GetMenuItemCount(IntPtr hMenu);
    [DllImport("user32.dll")]
    public static extern uint GetMenuItemID(IntPtr hMenu, int nPos);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMenuItemInfo(IntPtr hMenu, UInt32 uItem, bool fByPosition, ref MENUITEMINFO lpmii);
}
