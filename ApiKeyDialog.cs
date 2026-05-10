using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeepSeekDeskBand
{
    /// <summary>
    /// API Key 设置对话框
    /// 右键 DeskBand → "设置 API Key" 时弹出
    /// </summary>
    public partial class ApiKeyDialog : Form
    {
        private TextBox _apiKeyBox;
        private Button _saveBtn;
        private Button _cancelBtn;
        private Button _testBtn;
        private Label _statusLabel;
        private Label _titleLabel;
        private Label _hintLabel;
        private CheckBox _showKeyCheck;

        // 深色主题配色
        private readonly Color _bgDark = Color.FromArgb(32, 32, 32);
        private readonly Color _bgPanel = Color.FromArgb(45, 45, 45);
        private readonly Color _textColor = Color.FromArgb(240, 240, 240);
        private readonly Color _accentColor = Color.FromArgb(0, 120, 212);
        private readonly Color _errorColor = Color.FromArgb(232, 17, 35);
        private readonly Color _successColor = Color.FromArgb(16, 137, 62);
        private readonly Color _inputBg = Color.FromArgb(60, 60, 60);

        private string? _currentApiKey;

        public ApiKeyDialog()
        {
            InitializeDialog();
            LoadExistingKey();
        }

        private void InitializeDialog()
        {
            this.Text = "DeepSeek API Key 设置";
            this.Size = new Size(480, 340);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = _bgDark;
            this.ForeColor = _textColor;
            this.ShowInTaskbar = true;

            _titleLabel = new Label
            {
                Text = "设置 API Key",
                Font = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold),
                ForeColor = _textColor,
                Location = new Point(20, 16),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            _hintLabel = new Label
            {
                Text = "在 platform.deepseek.com/api_keys 获取 Key\nKey 将安全存储在 Windows 凭据管理器中",
                Font = new Font("Microsoft YaHei UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(160, 160, 160),
                Location = new Point(20, 46),
                Size = new Size(430, 36),
                BackColor = Color.Transparent
            };

            // --- API Key 输入框 ---
            _apiKeyBox = new TextBox
            {
                Location = new Point(20, 95),
                Size = new Size(370, 32),
                Font = new Font("Consolas", 10f),
                BackColor = _inputBg,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                UseSystemPasswordChar = true,
                MaxLength = 256,
                ForeColor = Color.Gray
            };
            _apiKeyBox.Enter += (s, e) =>
            {
                if (_apiKeyBox.Text == "sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" && _apiKeyBox.ForeColor == Color.Gray)
                {
                    _apiKeyBox.Text = "";
                    _apiKeyBox.ForeColor = _textColor;
                }
            };
            _apiKeyBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_apiKeyBox.Text))
                {
                    _apiKeyBox.Text = "sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
                    _apiKeyBox.ForeColor = Color.Gray;
                }
            };

            // --- 显示/隐藏 Key ---
            _showKeyCheck = new CheckBox
            {
                Text = "显示",
                Font = new Font("Microsoft YaHei UI", 8.5f),
                ForeColor = Color.FromArgb(160, 160, 160),
                Location = new Point(395, 98),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            _showKeyCheck.CheckedChanged += (s, e) =>
            {
                _apiKeyBox.UseSystemPasswordChar = !_showKeyCheck.Checked;
            };

            // --- 测试按钮 ---
            _testBtn = new Button
            {
                Text = "测试连接",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 32),
                Location = new Point(20, 145),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = _textColor,
                Font = new Font("Microsoft YaHei UI", 8.5f),
                FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(100, 100, 100) }
            };
            _testBtn.Click += async (s, e) => await TestApiKey();

            // --- 状态标签 ---
            _statusLabel = new Label
            {
                Text = "",
                Font = new Font("Microsoft YaHei UI", 8.5f),
                ForeColor = Color.FromArgb(160, 160, 160),
                Location = new Point(130, 152),
                Size = new Size(320, 20),
                BackColor = Color.Transparent
            };

            // --- 分割线 ---
            Label separator = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(20, 195),
                Size = new Size(435, 2)
            };

            // --- 保存按钮 ---
            _saveBtn = new Button
            {
                Text = "保存",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 36),
                Location = new Point(240, 230),
                BackColor = _accentColor,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            _saveBtn.Click += SaveBtn_Click;

            // --- 取消按钮 ---
            _cancelBtn = new Button
            {
                Text = "取消",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 36),
                Location = new Point(130, 230),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = _textColor,
                Font = new Font("Microsoft YaHei UI", 10f),
                FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(100, 100, 100) }
            };
            _cancelBtn.Click += (s, e) => this.Close();

            // --- 清除按钮 ---
            Button clearBtn = new Button
            {
                Text = "清除",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 36),
                Location = new Point(20, 230),
                BackColor = _errorColor,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 9f),
                FlatAppearance = { BorderSize = 0 }
            };
            clearBtn.Click += (s, e) =>
            {
                _apiKeyBox.Clear();
                _statusLabel.Text = "Key 已清除，点击保存生效";
                _statusLabel.ForeColor = Color.FromArgb(255, 200, 0);
            };

            this.Controls.AddRange(new Control[]
            {
                _titleLabel, _hintLabel, _apiKeyBox, _showKeyCheck,
                _testBtn, _statusLabel, separator,
                _saveBtn, _cancelBtn, clearBtn
            });
        }

        /// <summary>
        /// 加载已保存的 Key（仅显示前/后几位）
        /// </summary>
        private void LoadExistingKey()
        {
            _currentApiKey = CredentialManager.LoadApiKey();
            if (!string.IsNullOrEmpty(_currentApiKey))
            {
                // 部分显示：sk-xxxx...xxxx
                if (_currentApiKey.Length > 15)
                {
                    string mask = _currentApiKey.Substring(0, 7) +
                        new string('*', 12) +
                        _currentApiKey.Substring(_currentApiKey.Length - 5);
                    _apiKeyBox.Text = mask;
                }
                else
                {
                    _apiKeyBox.Text = _currentApiKey;
                }
                _statusLabel.Text = $"已有保存的 Key（{_currentApiKey.Length} 字符）";
                _statusLabel.ForeColor = _successColor;
            }
            else
            {
                _statusLabel.Text = "尚未设置 API Key";
                _statusLabel.ForeColor = Color.FromArgb(255, 200, 0);
            }
        }

        /// <summary>
        /// 测试 API Key 是否有效
        /// </summary>
        private async System.Threading.Tasks.Task TestApiKey()
        {
            string key = GetActualKey();
            if (string.IsNullOrWhiteSpace(key))
            {
                _statusLabel.Text = "请先输入 API Key";
                _statusLabel.ForeColor = _errorColor;
                return;
            }

            _testBtn.Enabled = false;
            _testBtn.Text = "测试中...";
            _statusLabel.Text = "正在连接...";
            _statusLabel.ForeColor = Color.FromArgb(160, 160, 160);

            try
            {
                using var client = new DeepSeekApiClient();
                var result = await client.GetBalanceAsync(key);

                if (result.IsSuccess)
                {
                    _statusLabel.Text = $"连接成功！余额: {result.TotalBalance} {result.Currency}";
                    _statusLabel.ForeColor = _successColor;
                }
                else
                {
                    _statusLabel.Text = $"错误: {result.ErrorMessage}";
                    _statusLabel.ForeColor = _errorColor;
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"测试失败：{ex.Message}";
                _statusLabel.ForeColor = _errorColor;
            }
            finally
            {
                _testBtn.Enabled = true;
                _testBtn.Text = "测试连接";
            }
        }

        /// <summary>
        /// 获取实际输入（处理 mask 显示的情况）
        /// </summary>
        private string GetActualKey()
        {
            string text = _apiKeyBox.Text.Trim();

            // 如果输入框包含 • 说明是 mask 显示的旧 key 没被修改
            if (text.Contains("*") && _currentApiKey != null)
                return _currentApiKey;

            return text;
        }

        /// <summary>
        /// 保存按钮点击
        /// </summary>
        private void SaveBtn_Click(object? sender, EventArgs e)
        {
            string key = GetActualKey();

            if (string.IsNullOrWhiteSpace(key))
            {
                // 清除 Key
                CredentialManager.DeleteApiKey();
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }

            // 保存到 Credential Manager
            try
            {
                CredentialManager.SaveApiKey(key);
                _statusLabel.Text = "API Key 已保存到凭据管理器";
                _statusLabel.ForeColor = _successColor;
                this.DialogResult = DialogResult.OK;

                // 延迟关闭让用户看到成功提示
                Timer delay = new Timer { Interval = 800 };
                delay.Tick += (s, args) =>
                {
                    delay.Stop();
                    this.Close();
                };
                delay.Start();
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"保存失败：{ex.Message}";
                _statusLabel.ForeColor = _errorColor;
            }
        }
    }
}
