using Ime.Core;
using System.Runtime.InteropServices;

namespace Ime.Hotkey;

public sealed class HotKeyWindow : NativeWindow, IDisposable
{
    private bool _disposed;

    public HotKeyWindow()
    {
        // 非表示ウィンドウを作成
        CreateHandle(new CreateParams());

        bool success = NativeMethods.RegisterHotKey(
            Handle,
            (int)HotKeyId.Japanese,
            NativeMethods.MOD_CTRL | NativeMethods.MOD_WIN,
            (uint)Keys.Z);

        if (!success)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"RegisterHotKey failed. Win32Error={error}");
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
                $"RegisterHotKey failed. Win32Error={error}");
        }
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

        NativeMethods.UnregisterHotKey(Handle, (int)HotKeyId.Japanese);
        NativeMethods.UnregisterHotKey(Handle, (int)HotKeyId.English);

        DestroyHandle();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
