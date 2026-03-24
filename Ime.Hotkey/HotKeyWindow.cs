using System.Runtime.InteropServices;

namespace Ime.Hotkey;

public sealed class HotKeyWindow : NativeWindow, IDisposable
{
    private bool _disposed;
    private bool _hotKeysRegistered;

    public HotKeyWindow()
    {
        // 非表示ウィンドウを作成
        CreateHandle(new CreateParams());
        RegisterHotKeys();
    }

    private void RegisterHotKeys()
    {
        if (_hotKeysRegistered)
        {
            UnregisterHotKeys();
        }

        bool success = NativeMethods.RegisterHotKey(
            Handle,
            (int)HotKeyId.Japanese,
            NativeMethods.MOD_CTRL | NativeMethods.MOD_WIN,
            (uint)Keys.Z);

        if (!success)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"RegisterHotKey (Japanese) failed. Win32Error={error}");
        }

        success = NativeMethods.RegisterHotKey(
            Handle,
            (int)HotKeyId.English,
            NativeMethods.MOD_CTRL | NativeMethods.MOD_WIN,
            (uint)Keys.A);

        if (!success)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"RegisterHotKey (English) failed. Win32Error={error}");
        }

        _hotKeysRegistered = true;
    }

    private void UnregisterHotKeys()
    {
        if (!_hotKeysRegistered) return;

        NativeMethods.UnregisterHotKey(Handle, (int)HotKeyId.Japanese);
        NativeMethods.UnregisterHotKey(Handle, (int)HotKeyId.English);
        _hotKeysRegistered = false;
    }

    /// <summary>
    /// Windows メッセージを受信
    /// </summary>
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_HOTKEY)
        {
            HotKeyId hotKeyId = (HotKeyId)m.WParam.ToInt32();
            OnHotKeyPressed(hotKeyId);
        }

        base.WndProc(ref m);
    }

    /// <summary>
    /// ホットキー押下時の処理
    /// </summary>
    private void OnHotKeyPressed(HotKeyId hotKeyId)
    {
        _ = hotKeyId switch
        {
            HotKeyId.Japanese => HandleJapaneseAsync(),
            HotKeyId.English => HandleEnglishAsync(),
            _ => Task.CompletedTask
        };
    }

    private async Task HandleJapaneseAsync()
    {
        await ImeController.RequestJapaneseAsync();
        await Task.Delay(30);
        ShowOverlay("あ");
    }

    private async Task HandleEnglishAsync()
    {
        await ImeController.RequestEnglishAsync();
        ShowOverlay("A");
    }

    private void ShowOverlay(string text)
    {
        var form = new ImeOverlayForm(text);
        form.Show();
    }

    /// <summary>
    /// ホットキー解除
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        UnregisterHotKeys();

        DestroyHandle();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
