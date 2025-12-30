using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Performance.Application.Services;
using Performance.Infrastructure.Entities;
using Performance.UI;

namespace Performance
{
    public partial class LoginForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly IUserService _userService;
        private TextBox _txtUsername = null!;
        private TextBox _txtPassword = null!;
        public UserEntity? AuthenticatedUser { get; private set; }

        public LoginForm(IUserService userService)
        {
            _userService = userService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(500, 600);
            this.Name = "LoginForm";
            this.Text = "Performance - Login";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = UiColors.Background;

            // Logo/Title Panel
            var titlePanel = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = UiColors.DarkGreen
            };

            var lblAppName = new Label()
            {
                Text = "PERFORMANCE",
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Left = 0,
                Top = 40
            };
            lblAppName.Left = (titlePanel.Width - lblAppName.Width) / 2;

            var lblSubtitle = new Label()
            {
                Text = "Task & Project Management System",
                Font = new Font("Segoe UI", 11F),
                ForeColor = UiColors.LightGreen,
                AutoSize = true,
                Left = 0,
                Top = 95
            };
            lblSubtitle.Left = (titlePanel.Width - lblSubtitle.Width) / 2;

            titlePanel.Controls.Add(lblAppName);
            titlePanel.Controls.Add(lblSubtitle);

            // Login Panel
            var loginPanel = new Panel()
            {
                Width = 380,
                Height = 320,
                Left = 60,
                Top = 190,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblLogin = new Label()
            {
                Text = "Sign In",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = UiColors.PrimaryText,
                Left = 30,
                Top = 30,
                AutoSize = true
            };

            var lblUsername = new Label()
            {
                Text = "Username",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = UiColors.SecondaryText,
                Left = 30,
                Top = 85,
                AutoSize = true
            };

            _txtUsername = new TextBox()
            {
                Left = 30,
                Top = 110,
                Width = 320,
                Height = 35,
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.White,
                ForeColor = UiColors.PrimaryText
            };

            var lblPassword = new Label()
            {
                Text = "Password",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = UiColors.SecondaryText,
                Left = 30,
                Top = 160,
                AutoSize = true
            };

            _txtPassword = new TextBox()
            {
                Left = 30,
                Top = 185,
                Width = 320,
                Height = 35,
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true,
                BackColor = Color.White,
                ForeColor = UiColors.PrimaryText
            };

            var btnLogin = new Button()
            {
                Text = "LOGIN",
                Left = 30,
                Top = 245,
                Width = 320,
                Height = 45,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = UiColors.ButtonPrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;

            // Hover effects
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = UiColors.ButtonHover;
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = UiColors.ButtonPrimary;

            btnLogin.Click += async (s, e) => await PerformLogin();

            // Enter key support for both textboxes
            _txtUsername.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await PerformLogin();
                }
            };

            _txtPassword.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await PerformLogin();
                }
            };

            loginPanel.Controls.Add(lblLogin);
            loginPanel.Controls.Add(lblUsername);
            loginPanel.Controls.Add(_txtUsername);
            loginPanel.Controls.Add(lblPassword);
            loginPanel.Controls.Add(_txtPassword);
            loginPanel.Controls.Add(btnLogin);

            this.Controls.Add(titlePanel);
            this.Controls.Add(loginPanel);

            this.AcceptButton = btnLogin; // Enter key triggers login
            this.ResumeLayout(false);
        }

        private async Task PerformLogin()
        {
            var username = _txtUsername.Text.Trim();
            var password = _txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter username and password.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var user = await _userService.AuthenticateAsync(username, password);
                if (user == null)
                {
                    MessageBox.Show("Invalid credentials. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtPassword.Clear();
                    _txtPassword.Focus();
                    return;
                }

                AuthenticatedUser = user;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
