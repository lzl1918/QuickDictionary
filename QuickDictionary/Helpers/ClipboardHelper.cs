using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickDictionary.Helpers
{
    public static class ClipboardHelper
    {
        public const int WM_DRAWCLIPBOARD = 0x308;
        public const int WM_CHANGECBCHAIN = 0x30D;
        public const int WM_CLIPBOARDUPDATE = 0x031D;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr HWnd, IntPtr HWndNext);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        
    }
}
