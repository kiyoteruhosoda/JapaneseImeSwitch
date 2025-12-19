using Ime.Core;

Console.WriteLine("Switching the active window to Japanese Input (JP) in 5 seconds...");
Console.WriteLine("Activate Notepad, a browser, etc. now.");

// 1. 5秒待機
Thread.Sleep(5000);

// 2. 現在アクティブなウィンドウ（最前面のアプリ）のハンドルを取得
IntPtr hWnd = NativeIme.GetForegroundWindow();

if (hWnd == IntPtr.Zero)
{
    Console.WriteLine("No active window found.");
    return;
}

// 3. 日本語のロケールID (0411) のハンドルを取得
// これが "Google日本語入力" や "MS-IME" などの日本語環境を指します
IntPtr hklJapanese = NativeIme.LoadKeyboardLayout("0411", NativeIme.KLF_ACTIVATE);
//0409

// 4. 対象ウィンドウに「入力言語を変更してくれ」というメッセージを送信
// これが Shift+Alt 相当の動作になります
NativeIme.PostMessage(hWnd, NativeIme.WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, hklJapanese);

Console.WriteLine("Switching message sent (0x{0:X})", hklJapanese.ToInt64());

// 結果確認用に少し待つ（コンソールがすぐ閉じないように）
Thread.Sleep(2000);
