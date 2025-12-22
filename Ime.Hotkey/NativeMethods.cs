using System.Runtime.InteropServices;

namespace Ime.Hotkey;

public static class NativeMethods
{
    public const int WM_HOTKEY = 0x0312;

    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CTRL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;

    public const string JAJP_KLID = "00000411";
    public const string ENUS_KLID = "00000409";
    public const uint KLF_NON_ACTIVATE = 0x00000000;
    public const uint KLF_ACTIVATE = 0x00000001;
    public const int WM_INPUTLANGCHANGEREQUEST = 0x0050;


    // 変換モード
    public const int IME_CMODE_NATIVE = 0x0001;   // 日本語入力
    public const int IME_CMODE_FULLSHAPE = 0x0008; // 全角

    // 文種
    public const int IME_SMODE_NONE = 0x0000;
    [DllImport("imm32.dll")]
    public static extern IntPtr ImmGetContext(IntPtr hWnd);

    [DllImport("imm32.dll")]
    public static extern bool ImmSetConversionStatus(
        IntPtr hIMC,
        int dwConversion,
        int dwSentence);

    [DllImport("imm32.dll")]
    public static extern bool ImmReleaseContext(
        IntPtr hWnd,
        IntPtr hIMC);

    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(
        IntPtr hWnd,
        int id,
        uint fsModifiers,
        uint vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(
        IntPtr hWnd,
        int id);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

    [DllImport("user32.dll")]
    public static extern bool PostMessage(
        IntPtr hWnd,
        int Msg,
        IntPtr wParam,
        IntPtr lParam);

}
