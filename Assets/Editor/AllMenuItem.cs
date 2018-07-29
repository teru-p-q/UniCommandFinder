using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class AllMenuItem
{
    IntPtr hMainWnd;

    public List<UnityMenuItem> EnumItems()
    {
        Win32.EnumWindows(new Win32.EnumWindowsDelegate(EnumWindowCallBack), IntPtr.Zero);

        var hMenu = Win32.GetMenu(hMainWnd);

        var items = new List<UnityMenuItem>();
        MenuItem(hMenu, "", items);

        return items;
    }

    private bool EnumWindowCallBack(IntPtr hWnd, IntPtr lparam)
    {
        int processId;

        Win32.GetWindowThreadProcessId(hWnd, out processId);
        if (processId != Process.GetCurrentProcess().Id)
        {
            return true;
        }

        //ウィンドウのタイトルの長さを取得する
        int textLen = Win32.GetWindowTextLength(hWnd);
        if (textLen == 0)
        {
            return true;
        }

        //ウィンドウのタイトルを取得する
        var tsb = new StringBuilder(textLen + 1);
        Win32.GetWindowText(hWnd, tsb, tsb.Capacity);

        hMainWnd = hWnd;

        return false;
    }

    private void MenuItem(IntPtr hMenu, string root, List<UnityMenuItem> items)
    {
        var count = Win32.GetMenuItemCount(hMenu);

        var menuName = new StringBuilder(64);

        for (var i = 0u; i < count; i++)
        {
            var mii = new Win32.MENUITEMINFO()
            {
                fMask = Win32.MIIM.FTYPE,
            };

            if (Win32.GetMenuItemInfo(hMenu, i, true, ref mii))
            {
                if (mii.fType == Win32.MFT_SEPARATOR)
                {
                    continue;
                }
            }

            Win32.GetMenuString(hMenu, i, menuName, menuName.Capacity, Win32.MF_BYPOSITION);

            var hSubMenu = Win32.GetSubMenu(hMenu, (int)i);

            var wID = Win32.GetMenuItemID(hMenu, (int)i);

            if ((int)wID > 0)
            {
                items.Add(new UnityMenuItem { Path = root, Name = menuName.ToString().Split('\t')[0], Id = (int)wID });
            }

            MenuItem(hSubMenu, root + menuName + "/", items);
        }
    }
}
