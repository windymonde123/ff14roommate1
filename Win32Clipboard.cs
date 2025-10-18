// Win32Clipboard.cs
using System;
using System.Runtime.InteropServices;

namespace Housemate
{
    /// <summary>
    /// 直接操作 Windows 剪贴板，复制任意长度 UTF-16 文本。
    /// </summary>
    public static class Win32Clipboard
    {
        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE  = 0x0002;

        [DllImport("user32.dll", SetLastError = true)] private static extern bool   OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool   CloseClipboard();
        [DllImport("user32.dll", SetLastError = true)] private static extern bool   EmptyClipboard();
        [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool   GlobalUnlock(IntPtr hMem);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GlobalFree(IntPtr hMem);

        public static void CopyTextToClipboard(string text)
        {
            if (text == null) text = string.Empty;
            // 末尾要有双字节的 '\0'
            var bytes = (text.Length + 1) * 2; // UTF-16 每字符2字节 + 终止0
            IntPtr hMem = IntPtr.Zero;
            try
            {
                hMem = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes);
                if (hMem == IntPtr.Zero) throw new Exception("GlobalAlloc failed");

                var pMem = GlobalLock(hMem);
                if (pMem == IntPtr.Zero) throw new Exception("GlobalLock failed");

                // 把托管字符串复制到非托管缓冲
                Marshal.Copy(text.ToCharArray(), 0, pMem, text.Length);
                // 末尾补 \0\0
                Marshal.WriteInt16(pMem, text.Length * 2, 0);

                GlobalUnlock(hMem);

                if (!OpenClipboard(IntPtr.Zero)) throw new Exception("OpenClipboard failed");
                try
                {
                    if (!EmptyClipboard()) throw new Exception("EmptyClipboard failed");
                    if (SetClipboardData(CF_UNICODETEXT, hMem) == IntPtr.Zero)
                        throw new Exception("SetClipboardData failed");
                    // 成功后，剪贴板“接管”hMem 的释放权，不能再 Free
                    hMem = IntPtr.Zero;
                }
                finally
                {
                    CloseClipboard();
                }
            }
            finally
            {
                if (hMem != IntPtr.Zero) GlobalFree(hMem);
            }
        }
    }
}
