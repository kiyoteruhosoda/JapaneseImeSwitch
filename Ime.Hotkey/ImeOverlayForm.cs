using System.Drawing;
using System.Windows.Forms;

namespace Ime.Hotkey;

public sealed class ImeOverlayForm : Form
{
    private readonly System.Windows.Forms.Timer _timer;
    private int _tickCount;

    public ImeOverlayForm(string text)
    {
        // ===== ウィンドウ基本設定 =====
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;

        BackColor = Color.White;
        Opacity = 0.85;
        Size = new Size(180, 120);

        // ===== 表示内容（アイコン＋文字） =====
        var label = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.Black,
            Font = new Font("Yu Gothic", 28)
        };
        Controls.Add(label);

        // ===== 表示位置（アクティブディスプレイ中央） =====
        var screen = GetActiveScreen();
        Location = new Point(
            screen.WorkingArea.Left + (screen.WorkingArea.Width - Width) / 2,
            screen.WorkingArea.Top + (screen.WorkingArea.Height - Height) / 2
        );

        // ===== フェードアウト Timer =====
        _timer = new System.Windows.Forms.Timer
        {
            Interval = 50 // 50ms × 20回 = 約1秒
        };

        _timer.Tick += (_, _) =>
        {
            _tickCount++;

            if (_tickCount > 6) // 少し待ってからフェード開始
            {
                Opacity -= 0.05;
            }

            if (Opacity <= 0)
            {
                _timer.Stop();
                Close();
            }
        };

        _timer.Start();
    }

    // ★ フォーカスを奪わない（論理的抑止）
    protected override bool ShowWithoutActivation => true;

    // ★ OS レベルでアクティブ化を禁止（物理的抑止）
    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
            return cp;
        }
    }

    // ★ 現在アクティブな画面を取得
    private static Screen GetActiveScreen()
    {
        return Screen.FromPoint(Cursor.Position);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        // 
        // ImeOverlayForm
        // 
        ClientSize = new Size(284, 261);
        Font = new Font("Yu Gothic", 27.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
        Name = "ImeOverlayForm";
        ResumeLayout(false);

    }
}
