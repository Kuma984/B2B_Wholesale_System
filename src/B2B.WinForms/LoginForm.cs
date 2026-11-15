using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using B2B.Core.Data;
using B2B.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace B2B.WinForms
{
    public class LoginForm : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Label lblError = null!;
        private int failedAttempts = 0;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Warehouse Admin Login - B2B Wholesale";
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);

            var lblTitle = new Label
            {
                Text = "Admin Portal Login",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(40, 25),
                Size = new Size(320, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblSubtitle = new Label
            {
                Text = "Authorized Warehouse Administrators Only",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(40, 60),
                Size = new Size(320, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblUser = new Label
            {
                Text = "Username or Email:",
                Location = new Point(40, 95),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
            };

            txtUsername = new TextBox
            {
                Location = new Point(40, 118),
                Size = new Size(320, 26),
                Font = new Font("Segoe UI", 10),
                Text = "admin_main" // sample default for convenience
            };

            var lblPass = new Label
            {
                Text = "Password:",
                Location = new Point(40, 155),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
            };

            txtPassword = new TextBox
            {
                Location = new Point(40, 178),
                Size = new Size(320, 26),
                Font = new Font("Segoe UI", 10),
                PasswordChar = '●',
                Text = "Admin@123" // sample default for convenience
            };

            btnLogin = new Button
            {
                Text = "Sign In as Admin",
                Location = new Point(40, 220),
                Size = new Size(320, 38),
                BackColor = Color.FromArgb(13, 110, 253),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.Click += async (s, e) => await HandleLoginAsync();

            lblError = new Label
            {
                Text = "",
                Location = new Point(40, 265),
                Size = new Size(320, 30),
                ForeColor = Color.Crimson,
                Font = new Font("Segoe UI", 8.5f),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblSubtitle);
            this.Controls.Add(lblUser);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPass);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(lblError);
            this.AcceptButton = btnLogin;
        }

        private async Task HandleLoginAsync()
        {
            lblError.Text = "Verifying credentials...";
            lblError.ForeColor = Color.Navy;
            btnLogin.Enabled = false;

            try
            {
                using var context = DbContextFactory.CreateDbContext();
                var authService = new AuthService(context);

                var (success, message, user) = await authService.AuthenticateAsync(
                    txtUsername.Text.Trim(), 
                    txtPassword.Text, 
                    "Admin");

                if (!success)
                {
                    failedAttempts++;
                    // Check if account should be locked (after 3 failed attempts)
                    if (authService.ShouldLockAccount(failedAttempts, 3))
                    {
                        var targetUser = await context.Users.FirstOrDefaultAsync(u => 
                            u.UserName == txtUsername.Text.Trim() || u.Email == txtUsername.Text.Trim());
                        if (targetUser != null && !targetUser.Lock)
                        {
                            targetUser.Lock = true;
                            await context.SaveChangesAsync();
                        }
                        lblError.ForeColor = Color.Crimson;
                        lblError.Text = "Account has been LOCKED due to 3 failed attempts!";
                        MessageBox.Show("Security Alert: Account has been locked due to 3 consecutive failed login attempts.", 
                            "Account Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        lblError.ForeColor = Color.Crimson;
                        lblError.Text = $"{message} (Attempts: {failedAttempts}/3)";
                    }
                    btnLogin.Enabled = true;
                    return;
                }

                // Login successful
                lblError.ForeColor = Color.Green;
                lblError.Text = "Login successful!";
                this.Hide();

                var mainForm = new MainForm(user!);
                mainForm.FormClosed += (s, e) => this.Close();
                mainForm.Show();
            }
            catch (Exception ex)
            {
                lblError.ForeColor = Color.Crimson;
                lblError.Text = $"Database connection error: {ex.Message}";
                btnLogin.Enabled = true;
            }
        }
    }
}
