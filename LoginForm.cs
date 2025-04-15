using ProiectPAOO;
using System;
using System.Windows.Forms;

namespace ProiectPAOO
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblSelectLoginType = new System.Windows.Forms.Label();
            this.btnClientLogin = new System.Windows.Forms.Button();
            this.btnAdminLogin = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblSelectLoginType.AutoSize = true;
            this.lblSelectLoginType.Location = new System.Drawing.Point(100, 30);
            this.lblSelectLoginType.Name = "lblSelectLoginType";
            this.lblSelectLoginType.Size = new System.Drawing.Size(100, 13);
            this.lblSelectLoginType.TabIndex = 0;
            this.lblSelectLoginType.Text = "Select Login Type:";
            this.lblSelectLoginType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnClientLogin.Location = new System.Drawing.Point(75, 60);
            this.btnClientLogin.Name = "btnClientLogin";
            this.btnClientLogin.Size = new System.Drawing.Size(150, 30);
            this.btnClientLogin.TabIndex = 1;
            this.btnClientLogin.Text = "Login as Client";
            this.btnClientLogin.UseVisualStyleBackColor = true;
            this.btnClientLogin.Click += new System.EventHandler(this.btnClientLogin_Click);
            this.btnAdminLogin.Location = new System.Drawing.Point(75, 100);
            this.btnAdminLogin.Name = "btnAdminLogin";
            this.btnAdminLogin.Size = new System.Drawing.Size(150, 30);
            this.btnAdminLogin.TabIndex = 2;
            this.btnAdminLogin.Text = "Login as Admin";
            this.btnAdminLogin.UseVisualStyleBackColor = true;
            this.btnAdminLogin.Click += new System.EventHandler(this.btnAdminLogin_Click);
            this.ClientSize = new System.Drawing.Size(300, 180);
            this.Controls.Add(this.lblSelectLoginType);
            this.Controls.Add(this.btnClientLogin);
            this.Controls.Add(this.btnAdminLogin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnClientLogin_Click(object sender, EventArgs e)
        {
            try
            {
                PopulationManager.LogUsage("client", "Login", "N/A");
                OpenMainWindow("client");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdminLogin_Click(object sender, EventArgs e)
        {
            try
            {
                ShowAdminLoginDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAdminLoginDialog()
        {
            using (Form loginDialog = new Form())
            {
                loginDialog.Text = "Admin Login";
                loginDialog.Size = new System.Drawing.Size(300, 150);
                loginDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                loginDialog.StartPosition = FormStartPosition.CenterParent;
                loginDialog.MaximizeBox = false;
                loginDialog.MinimizeBox = false;

                Label lblUsername = new Label();
                lblUsername.Text = "Username:";
                lblUsername.Location = new System.Drawing.Point(10, 20);
                lblUsername.Size = new System.Drawing.Size(80, 20);

                TextBox txtUsername = new TextBox();
                txtUsername.Location = new System.Drawing.Point(100, 20);
                txtUsername.Size = new System.Drawing.Size(170, 20);

                Label lblPassword = new Label();
                lblPassword.Text = "Password:";
                lblPassword.Location = new System.Drawing.Point(10, 50);
                lblPassword.Size = new System.Drawing.Size(80, 20);

                TextBox txtPassword = new TextBox();
                txtPassword.Location = new System.Drawing.Point(100, 50);
                txtPassword.Size = new System.Drawing.Size(170, 20);
                txtPassword.PasswordChar = '*';

                Button btnOk = new Button();
                btnOk.Text = "OK";
                btnOk.DialogResult = DialogResult.OK;
                btnOk.Location = new System.Drawing.Point(100, 80);
                btnOk.Size = new System.Drawing.Size(80, 25);

                Button btnCancel = new Button();
                btnCancel.Text = "Cancel";
                btnCancel.DialogResult = DialogResult.Cancel;
                btnCancel.Location = new System.Drawing.Point(190, 80);
                btnCancel.Size = new System.Drawing.Size(80, 25);

                loginDialog.Controls.Add(lblUsername);
                loginDialog.Controls.Add(txtUsername);
                loginDialog.Controls.Add(lblPassword);
                loginDialog.Controls.Add(txtPassword);
                loginDialog.Controls.Add(btnOk);
                loginDialog.Controls.Add(btnCancel);

                loginDialog.AcceptButton = btnOk;
                loginDialog.CancelButton = btnCancel;

                if (loginDialog.ShowDialog(this) == DialogResult.OK)
                {
                    string username = txtUsername.Text;
                    string password = txtPassword.Text;

                    if (AuthenticateAdmin(username, password))
                    {
                        PopulationManager.LogUsage("admin", "Login", "N/A");
                        OpenMainWindow(username);
                    }
                    else
                    {
                        MessageBox.Show("Invalid credentials", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool AuthenticateAdmin(string username, string password)
        {
            return "admin".Equals(username) && "pass".Equals(password);
        }

        private void OpenMainWindow(string currentUser)
        {
            MainForm mainForm = new MainForm(currentUser);
            mainForm.Show();
            this.Hide();
        }

        private System.Windows.Forms.Label lblSelectLoginType;
        private System.Windows.Forms.Button btnClientLogin;
        private System.Windows.Forms.Button btnAdminLogin;
    }
}