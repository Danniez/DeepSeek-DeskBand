using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSeekDeskBand
{
    /// <summary>
    /// 简洁余额显示 —— 无图标、无标题，只有余额数字
    /// </summary>
    public partial class DeskBandControl : UserControl
    {
        private readonly DeskBand _deskBand;
        private readonly DeepSeekApiClient _apiClient;

        private Label _balanceLabel;
        private Label _statusDot;
        private Color _dotColor = Color.Yellow;
        private Timer _refreshTimer;

        private BalanceResult? _currentBalance;
        private bool _isLoading;
        private string? _apiKey;

        private readonly Color _bgColor = Color.FromArgb(32, 32, 32);
        private readonly Color _hoverColor = Color.FromArgb(50, 50, 50);
        private readonly Color _white = Color.FromArgb(240, 240, 240);
        private readonly Color _green = Color.FromArgb(16, 185, 129);
        private readonly Color _yellow = Color.FromArgb(250, 204, 21);
        private readonly Color _red = Color.FromArgb(248, 113, 113);

        public DeskBandControl(DeskBand deskBand)
        {
            _deskBand = deskBand;
            _apiClient = new DeepSeekApiClient();
            InitializeControl();
        }

        private void InitializeControl()
        {
            this.BackColor = _bgColor;
            this.MinimumSize = new Size(120, 40);
            this.Size = new Size(180, 40);
            this.Cursor = Cursors.Hand;
            this.Margin = new Padding(0);

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            _balanceLabel = new Label
            {
                AutoSize = false,
                Location = new Point(8, 4),
                Size = new Size(this.Width - 28, 32),
                Text = "...",
                ForeColor = _white,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _statusDot = new Label
            {
                AutoSize = false,
                Size = new Size(8, 8),
                Location = new Point(this.Width - 16, 16),
                BackColor = Color.Transparent,
                Text = ""
            };
            _statusDot.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using Brush b = new SolidBrush(_dotColor);
                e.Graphics.FillEllipse(b, 0, 0, 8, 8);
            };

            this.Controls.Add(_balanceLabel);
            this.Controls.Add(_statusDot);

            this.Click += (s, e) =>
                _deskBand.ShowFlyout(_currentBalance ?? BalanceResult.NoApiKey());
            _balanceLabel.Click += (s, e) =>
                _deskBand.ShowFlyout(_currentBalance ?? BalanceResult.NoApiKey());

            this.MouseEnter += (s, e) => this.BackColor = _hoverColor;
            this.MouseLeave += (s, e) => this.BackColor = _bgColor;

            _refreshTimer = new Timer { Interval = 30000 };
            _refreshTimer.Tick += async (s, e) => await RefreshBalance();
        }

        public void Start()
        {
            _apiKey = CredentialManager.LoadApiKey();
            if (string.IsNullOrEmpty(_apiKey))
                SetState("设置 API Key", _yellow);
            else
                _ = RefreshBalance();
            _refreshTimer.Start();
        }

        public async Task RefreshBalance()
        {
            if (_isLoading) return;
            _apiKey = CredentialManager.LoadApiKey();
            if (string.IsNullOrEmpty(_apiKey)) { SetState("设置 API Key", _yellow); return; }

            _isLoading = true;
            SetState("...", _yellow);

            try
            {
                _currentBalance = await _apiClient.GetBalanceAsync(_apiKey);
                if (_currentBalance.IsSuccess)
                    SetState($"余额 {_currentBalance.TotalBalance ?? "—"} {_currentBalance.Currency ?? ""}", _green);
                else
                    SetState(ShortError(_currentBalance), _red);
            }
            catch (Exception ex)
            {
                _currentBalance = BalanceResult.Error(ex.Message);
                SetState("Error", _red);
            }
            finally { _isLoading = false; }
        }

        public async Task ReloadAndRefresh()
        {
            _apiKey = CredentialManager.LoadApiKey();
            await RefreshBalance();
        }

        public BalanceResult? CurrentBalance => _currentBalance;

        private void SetState(string text, Color dotColor)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetState(text, dotColor))); return; }
            _balanceLabel.Text = text;
            _balanceLabel.ForeColor = _white;
            _dotColor = dotColor;
            _statusDot.Invalidate();
        }

        private static string ShortError(BalanceResult r)
        {
            if (r.Status == BalanceResult.BalanceStatus.NoApiKey) return "无 Key";
            if (r.Status == BalanceResult.BalanceStatus.NetworkError) return "网络错误";
            return "错误";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_balanceLabel != null) _balanceLabel.Size = new Size(this.Width - 28, 32);
            if (_statusDot != null) _statusDot.Location = new Point(this.Width - 16, 16);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _refreshTimer?.Dispose(); _apiClient?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}

