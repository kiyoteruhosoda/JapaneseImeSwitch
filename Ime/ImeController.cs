using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Ime.Core;

public static class ImeController
{
    /*
     * =========================
     * A. Logi Actions 用（本番）
     * =========================
     */

    public static void ForceHiragana()
    {
        _ = TryForceHiraganaOnce();
    }

    /*
     * =========================
     * B. デバッグ用（詳細取得）
     * =========================
     */
    public static ImeTryResult TryForceHiraganaOnce()
    {
        // ★ IME 対象ウィンドウ（Edit / RichEdit 等）
        var imeHwnd = GetImeTargetWindow();
        if (imeHwnd == IntPtr.Zero)
        {
            return new ImeTryResult
            {
                Success = false,
                Status = ImeTryStatus.NoForeground
            };
        }

        // ★ 表示用：トップレベルウィンドウ
        var topHwnd = NativeIme.GetAncestor(imeHwnd, NativeIme.GA_ROOT);

        // Window title
        var sb = new StringBuilder(256);
        NativeIme.GetWindowText(topHwnd, sb, sb.Capacity);

        // Process ID / Name
        NativeIme.GetWindowThreadProcessId(topHwnd, out var pid);

        string processName;
        try
        {
            processName = Process.GetProcessById((int)pid).ProcessName;
        }
        catch
        {
            processName = "(unknown)";
        }

        /*
         * =========================
         * 入力言語切替（TSF向け）
         * =========================
         */
        var hklJapanese = NativeIme.LoadKeyboardLayout(
            "00000411", // Japanese
            NativeIme.KLF_ACTIVATE);

        if (hklJapanese != IntPtr.Zero)
        {
            NativeIme.PostMessage(
                topHwnd,
                NativeIme.WM_INPUTLANGCHANGEREQUEST,
                IntPtr.Zero,
                hklJapanese);

            return new ImeTryResult
            {
                Success = true,
                Status = ImeTryStatus.LanguageSwitchRequested,
                Hwnd = imeHwnd,
                WindowTitle = sb.ToString(),
                ProcessId = pid,
                ProcessName = processName
            };
        }

        return new ImeTryResult
        {
            Success = false,
            Status = ImeTryStatus.FailedToSet,
            Hwnd = imeHwnd,
            WindowTitle = sb.ToString(),
            ProcessId = pid,
            ProcessName = processName
        };
    }

    /*
     * =========================
     * IME 対象ウィンドウ取得
     * =========================
     */
    private static IntPtr GetImeTargetWindow()
    {
        var foreground = NativeIme.GetForegroundWindow();
        if (foreground == IntPtr.Zero)
            return IntPtr.Zero;

        var threadId = NativeIme.GetWindowThreadProcessId(foreground, out _);
        if (threadId == 0)
            return IntPtr.Zero;

        var gui = new NativeIme.GUITHREADINFO
        {
            cbSize = (uint)Marshal.SizeOf<NativeIme.GUITHREADINFO>()
        };

        if (!NativeIme.GetGUIThreadInfo(threadId, ref gui))
            return IntPtr.Zero;

        return gui.hwndFocus != IntPtr.Zero
            ? gui.hwndFocus
            : gui.hwndActive;
    }
}
