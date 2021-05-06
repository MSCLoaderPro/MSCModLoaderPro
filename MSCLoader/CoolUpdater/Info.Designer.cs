
namespace CoolUpdater
{
    partial class Info
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Info));
            this.title = new System.Windows.Forms.Label();
            this.btnQuit = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnWebsite = new System.Windows.Forms.Button();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labVer = new System.Windows.Forms.Label();
            this.labInfo = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.title.ForeColor = System.Drawing.Color.White;
            this.title.Location = new System.Drawing.Point(150, 3);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(169, 25);
            this.title.TabIndex = 0;
            this.title.Text = "COOL UPDATER";
            // 
            // btnQuit
            // 
            this.btnQuit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnQuit.Location = new System.Drawing.Point(155, 291);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(301, 35);
            this.btnQuit.TabIndex = 3;
            this.btnQuit.Text = "QUIT";
            this.btnQuit.UseVisualStyleBackColor = true;
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnWebsite);
            this.panel1.Controls.Add(this.btnMinimize);
            this.panel1.Controls.Add(this.btnExit);
            this.panel1.Controls.Add(this.title);
            this.panel1.Location = new System.Drawing.Point(12, 9);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(635, 35);
            this.panel1.TabIndex = 6;
            // 
            // btnWebsite
            // 
            this.btnWebsite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnWebsite.FlatAppearance.BorderSize = 0;
            this.btnWebsite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWebsite.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnWebsite.ForeColor = System.Drawing.Color.White;
            this.btnWebsite.Location = new System.Drawing.Point(5, 5);
            this.btnWebsite.Name = "btnWebsite";
            this.btnWebsite.Size = new System.Drawing.Size(53, 25);
            this.btnWebsite.TabIndex = 5;
            this.btnWebsite.Text = "?";
            this.btnWebsite.UseCompatibleTextRendering = true;
            this.btnWebsite.UseVisualStyleBackColor = false;
            this.btnWebsite.Click += new System.EventHandler(this.btnWebsite_Click);
            // 
            // btnMinimize
            // 
            this.btnMinimize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnMinimize.ForeColor = System.Drawing.Color.White;
            this.btnMinimize.Location = new System.Drawing.Point(518, 5);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(53, 25);
            this.btnMinimize.TabIndex = 4;
            this.btnMinimize.Text = "-";
            this.btnMinimize.UseCompatibleTextRendering = true;
            this.btnMinimize.UseVisualStyleBackColor = false;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Location = new System.Drawing.Point(577, 5);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(53, 25);
            this.btnExit.TabIndex = 3;
            this.btnExit.Text = "X";
            this.btnExit.UseCompatibleTextRendering = true;
            this.btnExit.UseVisualStyleBackColor = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.labInfo);
            this.panel2.Controls.Add(this.btnQuit);
            this.panel2.Location = new System.Drawing.Point(12, 50);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(635, 355);
            this.panel2.TabIndex = 7;
            // 
            // labVer
            // 
            this.labVer.AutoSize = true;
            this.labVer.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labVer.ForeColor = System.Drawing.Color.White;
            this.labVer.Location = new System.Drawing.Point(12, 408);
            this.labVer.Name = "labVer";
            this.labVer.Size = new System.Drawing.Size(39, 25);
            this.labVer.TabIndex = 8;
            this.labVer.Text = "1.0";
            this.labVer.Click += new System.EventHandler(this.labVer_Click);
            // 
            // labInfo
            // 
            this.labInfo.AutoSize = true;
            this.labInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labInfo.ForeColor = System.Drawing.Color.White;
            this.labInfo.Location = new System.Drawing.Point(103, 24);
            this.labInfo.Name = "labInfo";
            this.labInfo.Size = new System.Drawing.Size(422, 175);
            this.labInfo.TabIndex = 6;
            this.labInfo.Text = resources.GetString("labInfo.Text");
            // 
            // Info
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(101)))), ((int)(((byte)(34)))), ((int)(((byte)(18)))));
            this.ClientSize = new System.Drawing.Size(659, 442);
            this.Controls.Add(this.labVer);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Info";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mod Loader Pro - Mod Updater";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateView_FormClosing);
            this.Shown += new System.EventHandler(this.UpdateView_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Button btnQuit;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnWebsite;
        private System.Windows.Forms.Label labVer;
        private System.Windows.Forms.Label labInfo;
    }
}