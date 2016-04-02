namespace Order_Manager
{
    partial class Login
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.invalidLabel = new System.Windows.Forms.Label();
            this.picturebox = new System.Windows.Forms.PictureBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.signInButton = new System.Windows.Forms.Button();
            this.passwordTextbox = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.usernameTextbox = new System.Windows.Forms.TextBox();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.rememberCheckbox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picturebox)).BeginInit();
            this.SuspendLayout();
            // 
            // invalidLabel
            // 
            this.invalidLabel.AutoSize = true;
            this.invalidLabel.ForeColor = System.Drawing.Color.Red;
            this.invalidLabel.Location = new System.Drawing.Point(233, 277);
            this.invalidLabel.Name = "invalidLabel";
            this.invalidLabel.Size = new System.Drawing.Size(70, 13);
            this.invalidLabel.TabIndex = 5;
            this.invalidLabel.Text = "Invalid log on";
            this.invalidLabel.Visible = false;
            // 
            // picturebox
            // 
            this.picturebox.Image = ((System.Drawing.Image)(resources.GetObject("picturebox.Image")));
            this.picturebox.Location = new System.Drawing.Point(203, 46);
            this.picturebox.Name = "picturebox";
            this.picturebox.Size = new System.Drawing.Size(142, 141);
            this.picturebox.TabIndex = 13;
            this.picturebox.TabStop = false;
            // 
            // titleLabel
            // 
            this.titleLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.ForeColor = System.Drawing.Color.White;
            this.titleLabel.Location = new System.Drawing.Point(0, 1);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(541, 30);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Order Manger - SIGN IN";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // signInButton
            // 
            this.signInButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(168)))), ((int)(((byte)(17)))));
            this.signInButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.signInButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.signInButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.signInButton.ForeColor = System.Drawing.Color.White;
            this.signInButton.Location = new System.Drawing.Point(203, 293);
            this.signInButton.Name = "signInButton";
            this.signInButton.Size = new System.Drawing.Size(133, 37);
            this.signInButton.TabIndex = 6;
            this.signInButton.Text = "Sign In";
            this.signInButton.UseVisualStyleBackColor = false;
            this.signInButton.Click += new System.EventHandler(this.signInButton_Click);
            // 
            // passwordTextbox
            // 
            this.passwordTextbox.Location = new System.Drawing.Point(203, 246);
            this.passwordTextbox.MaxLength = 30;
            this.passwordTextbox.Name = "passwordTextbox";
            this.passwordTextbox.PasswordChar = '•';
            this.passwordTextbox.Size = new System.Drawing.Size(214, 20);
            this.passwordTextbox.TabIndex = 4;
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.passwordLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.passwordLabel.Location = new System.Drawing.Point(96, 246);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(93, 20);
            this.passwordLabel.TabIndex = 3;
            this.passwordLabel.Text = "Password :";
            // 
            // usernameTextbox
            // 
            this.usernameTextbox.Location = new System.Drawing.Point(203, 210);
            this.usernameTextbox.MaxLength = 30;
            this.usernameTextbox.Name = "usernameTextbox";
            this.usernameTextbox.Size = new System.Drawing.Size(214, 20);
            this.usernameTextbox.TabIndex = 2;
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.usernameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.usernameLabel.Location = new System.Drawing.Point(93, 210);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(96, 20);
            this.usernameLabel.TabIndex = 1;
            this.usernameLabel.Text = "Username :";
            // 
            // rememberCheckbox
            // 
            this.rememberCheckbox.AutoSize = true;
            this.rememberCheckbox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.rememberCheckbox.Location = new System.Drawing.Point(342, 305);
            this.rememberCheckbox.Name = "rememberCheckbox";
            this.rememberCheckbox.Size = new System.Drawing.Size(95, 17);
            this.rememberCheckbox.TabIndex = 14;
            this.rememberCheckbox.Text = "Remember Me";
            this.rememberCheckbox.UseVisualStyleBackColor = true;
            this.rememberCheckbox.CheckedChanged += new System.EventHandler(this.rememberCheckbox_CheckedChanged);
            // 
            // Login
            // 
            this.AcceptButton = this.signInButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(245)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(541, 340);
            this.Controls.Add(this.rememberCheckbox);
            this.Controls.Add(this.invalidLabel);
            this.Controls.Add(this.picturebox);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.signInButton);
            this.Controls.Add(this.passwordTextbox);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.usernameTextbox);
            this.Controls.Add(this.usernameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            ((System.ComponentModel.ISupportInitialize)(this.picturebox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label invalidLabel;
        private System.Windows.Forms.PictureBox picturebox;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Button signInButton;
        private System.Windows.Forms.TextBox passwordTextbox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox usernameTextbox;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.CheckBox rememberCheckbox;
    }
}