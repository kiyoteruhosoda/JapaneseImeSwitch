using System.Diagnostics;

namespace Ime.Core;

public sealed class ImeTryResult
{
    public bool Success { get; init; }
    public ImeTryStatus Status { get; init; }

    public IntPtr Hwnd { get; init; }
    public string WindowTitle { get; init; } = "";
    public uint ProcessId { get; init; }
    public string ProcessName { get; init; } = "";

    public override string ToString()
    {
        return
            $"Success={Success}, " +
            $"Status={Status}, " +
            $"HWND=0x{Hwnd.ToInt64():X}, " +
            $"PID={ProcessId}, " +
            $"Process={ProcessName}, " +
            $"Title=\"{WindowTitle}\"";
    }
}
