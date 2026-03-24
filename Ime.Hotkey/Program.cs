using Ime.Hotkey;

using var mutex = new Mutex(true, "HotKeyHost_Mutex", out bool created);
if (!created)
{
    MessageBox.Show(
        "IME Hotkeyは既に起動しています。",
        "IME Hotkey",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information);
    return;
}

Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);

using var context = new TrayApplicationContext();
Application.Run(context);
