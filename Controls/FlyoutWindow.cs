using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeepSeekDeskBand
{
    public partial class FlyoutWindow : Form
    {
        private readonly DeskBand _deskBand;
        private readonly BalanceResult _balance;

        private const int W = 360;
        private const int H = 280;

        private readonly Color _bg     = Color.FromArgb(32, 32, 32);
        private readonly Color _card   = Color.FromArgb(45, 45, 45);
        private readonly Color _text   = Color.FromArgb(240, 240, 240);
        private readonly Color _accent = Color.FromArgb(14, 165, 233);
        private readonly Color _green  = Color.FromArgb(16, 185, 129);
        private readonly Color _red    = Color.FromArgb(248, 113, 113);
        private readonly Color _yellow = Color.FromArgb(250, 204, 21);
        private readonly Color _gray   = Color.FromArgb(140, 140, 140);

        public FlyoutWindow(DeskBand deskBand, BalanceResult balance)
        {
            _deskBand = deskBand;
            _balance = balance;

            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(W, _balance.IsSuccess ? H : 190);
            this.BackColor = _bg;

            this.Deactivate += (s, e) => this.Close();
            this.Paint += (s, e) => { using var p = new Pen(Color.FromArgb(60,60,60)); e.Graphics.DrawRectangle(p, 0, 0, W-1, this.Height-1); };

            BuildUI();
        }

        private void BuildUI()
        {
            // Title bar
            var bar = new Panel { Location = new Point(0,0), Size = new Size(W, 32), BackColor = Color.FromArgb(28,28,28) };
            var title = new Label { Text = "DeepSeek DeskBand", ForeColor = _text, Font = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold), Location = new Point(12, 7), AutoSize = true, BackColor = Color.Transparent };
            var close = new Button { FlatStyle = FlatStyle.Flat, Text = "X", ForeColor = _gray, BackColor = Color.Transparent, Font = new Font("Segoe UI", 9f), Size = new Size(28, 28), Location = new Point(W - 34, 2), FlatAppearance = { BorderSize = 0 } };
            close.Click += (s, e) => this.Close();
            close.MouseEnter += (s, e) => close.ForeColor = Color.White;
            close.MouseLeave += (s, e) => close.ForeColor = _gray;
            bar.Controls.Add(title); bar.Controls.Add(close);
            this.Controls.Add(bar);

            if (_balance.IsSuccess) BuildBalanceView(); else BuildErrorView();
        }

        private void BuildBalanceView()
        {
            // Total balance card
            var card = new Panel { Location = new Point(10, 48), Size = new Size(W - 20, 72), BackColor = _card };
            var val  = new Label { Text = $"余额 {_balance.TotalBalance ?? "—"} {_balance.Currency ?? "CNY"}", Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = _green, Location = new Point(14, 8), Size = new Size(W - 48, 30), BackColor = Color.Transparent };
            var lbl  = new Label { Text = _balance.IsAvailable ? "● 可用" : "● 不可用", Font = new Font("Microsoft YaHei UI", 8f), ForeColor = _balance.IsAvailable ? _green : _red, Location = new Point(16, 46), AutoSize = true, BackColor = Color.Transparent };
            card.Controls.Add(val); card.Controls.Add(lbl);
            this.Controls.Add(card);

            // Sub cards
            int cw = (W - 32) / 2, cy = 138;
            var c1 = new Panel { Location = new Point(10, cy), Size = new Size(cw, 52), BackColor = _card };
            var v1 = new Label { Text = $"{_balance.ToppedUpBalance ?? "—"} {_balance.Currency ?? "CNY"}", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = _accent, Location = new Point(12, 6), AutoSize = true, BackColor = Color.Transparent };
            var l1 = new Label { Text = "充值余额", Font = new Font("Microsoft YaHei UI", 7.5f), ForeColor = _gray, Location = new Point(12, 31), AutoSize = true, BackColor = Color.Transparent };
            c1.Controls.Add(v1); c1.Controls.Add(l1); this.Controls.Add(c1);

            var c2 = new Panel { Location = new Point(10 + cw + 12, cy), Size = new Size(cw, 52), BackColor = _card };
            var v2 = new Label { Text = $"{_balance.GrantedBalance ?? "—"} {_balance.Currency ?? "CNY"}", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(168, 85, 247), Location = new Point(12, 6), AutoSize = true, BackColor = Color.Transparent };
            var l2 = new Label { Text = "赠金余额", Font = new Font("Microsoft YaHei UI", 7.5f), ForeColor = _gray, Location = new Point(12, 31), AutoSize = true, BackColor = Color.Transparent };
            c2.Controls.Add(v2); c2.Controls.Add(l2); this.Controls.Add(c2);

            // Buttons
            var b1 = new Button { Text = "设置 API Key", FlatStyle = FlatStyle.Flat, Size = new Size(100, 32), Location = new Point(10, 220), BackColor = _accent, ForeColor = Color.White, Font = new Font("Microsoft YaHei UI", 9f), FlatAppearance = { BorderSize = 0 } };
            b1.Click += (s, e) => { this.Close(); using var d = new ApiKeyDialog(); if (d.ShowDialog() == DialogResult.OK) _ = _deskBand.GetControl()?.ReloadAndRefresh(); };
            var b2 = new Button { Text = "刷新", FlatStyle = FlatStyle.Flat, Size = new Size(100, 32), Location = new Point(W - 114, 220), BackColor = Color.FromArgb(60,60,60), ForeColor = _text, Font = new Font("Microsoft YaHei UI", 9f), FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(80,80,80) } };
            b2.Click += (s, e) => { this.Close(); _ = _deskBand.GetControl()?.RefreshBalance(); };
            this.Controls.Add(b1); this.Controls.Add(b2);
        }

        private void BuildErrorView()
        {
            var card = new Panel { Location = new Point(10, 42), Size = new Size(W - 20, 100), BackColor = _card };
            string errTitle = _balance.Status == BalanceResult.BalanceStatus.NoApiKey ? "未设置 API Key" : "获取失败";
            string errMsg = _balance.ErrorMessage ?? "未知错误";

            var tl = new Label { Text = errTitle, Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = _text, Location = new Point(14, 12), AutoSize = true, BackColor = Color.Transparent };
            var tm = new Label { Text = errMsg, Font = new Font("Segoe UI", 10f), ForeColor = _gray, Location = new Point(14, 46), Size = new Size(W - 48, 36), BackColor = Color.Transparent };
            card.Controls.Add(tl); card.Controls.Add(tm);
            this.Controls.Add(card);

            var b1 = new Button { Text = "设置 API Key", FlatStyle = FlatStyle.Flat, Size = new Size(100, 32), Location = new Point(10, 150), BackColor = _accent, ForeColor = Color.White, Font = new Font("Microsoft YaHei UI", 9f), FlatAppearance = { BorderSize = 0 } };
            b1.Click += (s, e) => { this.Close(); using var d = new ApiKeyDialog(); if (d.ShowDialog() == DialogResult.OK) _ = _deskBand.GetControl()?.ReloadAndRefresh(); };
            var b2 = new Button { Text = "刷新", FlatStyle = FlatStyle.Flat, Size = new Size(100, 32), Location = new Point(W - 114, 150), BackColor = Color.FromArgb(60,60,60), ForeColor = _text, Font = new Font("Microsoft YaHei UI", 9f), FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(80,80,80) } };
            b2.Click += (s, e) => { this.Close(); _ = _deskBand.GetControl()?.RefreshBalance(); };
            this.Controls.Add(b1); this.Controls.Add(b2);
        }
    }
}
