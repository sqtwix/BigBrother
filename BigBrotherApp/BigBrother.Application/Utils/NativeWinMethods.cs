using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace BigBrother.Application.Utils;

public static class NativeWinMethods
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    public static (string ProcessName, string WindowTitle) GetActiveWindowInfo()
    {
        try
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return (null, null);

            const int nChars = 256;
            StringBuilder sb = new StringBuilder(nChars);
            if (GetWindowText(hWnd, sb, nChars) > 0)
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                try
                {
                    using (Process proc = Process.GetProcessById((int)processId))
                    {
                        return (proc.ProcessName, sb.ToString());
                    }
                }
                catch { return (null, null); }
            }
            return (null, null);
        }
        catch { return (null, null); }
    }

    public static TimeSpan GetIdleTime()
    {
        try
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            if (!GetLastInputInfo(ref lastInputInfo))
                return TimeSpan.Zero;

            uint tickCount = (uint)Environment.TickCount64;
            uint idleTicks = tickCount - lastInputInfo.dwTime;
            return TimeSpan.FromMilliseconds(idleTicks);
        }
        catch { return TimeSpan.Zero; }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct LASTINPUTINFO
{
    public uint cbSize;
    public uint dwTime;
}