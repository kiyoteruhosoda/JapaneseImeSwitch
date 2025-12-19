using Ime.Hotkey;

using var mutex = new Mutex(true, "HotKeyHost_Mutex", out bool created);
if (!created)
{
    return;
}

Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);

using var hotKeyWindow = new HotKeyWindow();
Application.Run(); // メッセージループ開始
