using Ime.Hotkey;
using System.Runtime.InteropServices;

namespace Ime.Core;

public static class ImeController
{
    static IntPtr _hklJapanese = NativeMethods.LoadKeyboardLayout(NativeMethods.JAJP_KLID, NativeMethods.KLF_NON_ACTIVATE);
    static IntPtr _hklEnglish = NativeMethods.LoadKeyboardLayout(NativeMethods.ENUS_KLID, NativeMethods.KLF_NON_ACTIVATE);

    public static async Task<nint> RequestJapaneseAsync()
    {
        var hWnd = Switch(_hklEnglish);
        if (hWnd == 0) return 0;

        await Task.Delay(30);

        Switch(_hklJapanese);
        await Task.Delay(30);

        SwitchJapaneseHiragana(hWnd);
        await Task.Delay(20);

        return hWnd;
    }

    public static async Task<nint> RequestEnglishAsync()
    {
        var hWnd = Switch(_hklEnglish);
        return hWnd;
    }


    public static void SwitchJapaneseHiragana(nint hWnd)
    {
        var hImc = NativeMethods.ImmGetContext(hWnd);
        if (hImc == IntPtr.Zero)
        {
            return;
        }

        try
        {
            NativeMethods.ImmSetConversionStatus(
                hImc,
                NativeMethods.IME_CMODE_NATIVE | NativeMethods.IME_CMODE_FULLSHAPE,
                NativeMethods.IME_SMODE_NONE);
        }
        finally
        {
            NativeMethods.ImmReleaseContext(hWnd, hImc);
        }
    }

    private static nint Switch(IntPtr hkl)
    {
        var hWnd = NativeMethods.GetForegroundWindow();
        if (hWnd == IntPtr.Zero)
        {
            return 0;
        }

        NativeMethods.PostMessage(
            hWnd,
            NativeMethods.WM_INPUTLANGCHANGEREQUEST,
            IntPtr.Zero,
            hkl);

        return hWnd;
    }
}
