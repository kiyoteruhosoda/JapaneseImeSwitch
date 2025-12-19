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
        try
        {
            switch (hotKeyId)
            {
                case HotKeyId.Japanese:
                    ImeController.RequestJapanese();
                    break;

                case HotKeyId.English:
                    ImeController.RequestEnglish();
                    break;

                default:
                    throw new InvalidOperationException("Unknown HotKeyId");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "IME HotKey Error");
        }
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
