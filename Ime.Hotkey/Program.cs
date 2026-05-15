using Ime.Hotkey;

using var mutex = new Mutex(true, "HotKeyHost_Mutex", out bool created);
if (!created)
{
    // 既に起動中の場合はサイレントに終了（タスクスケジューラ経由の二重起動対策）
    return;
}

Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
using var context = new TrayApplicationContext();
Application.Run(context);
