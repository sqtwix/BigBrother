using System.Runtime.InteropServices;
using System.Text;

namespace BigBrother.Application.Utils;

internal class NativeWinMethods
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
}

[StructLayout(LayoutKind.Sequential)]
public struct LASTINPUTINFO
{
    public uint cbSize;
    public uint dwTime;
}