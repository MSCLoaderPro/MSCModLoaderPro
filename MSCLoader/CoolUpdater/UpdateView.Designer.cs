
namespace CoolUpdater
{
    partial class UpdateView
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
            this.label1 = new System.Windows.Forms.Label();
            this.updateProgress = new System.Windows.Forms.ProgressBar();
            this.modsList = new System.Windows.Forms.CheckedListBox();
            this.btnQuit = new System.Windows.Forms.Button();
            this.btnStartGame = new System.Windows.Forms.Button();
            this.logBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(265, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mod Loader Pro";
            // 
            // updateProgress
            // 
            this.updateProgress.Location = new System.Drawing.Point(12, 208);
            this.updateProgress.Name = "updateProgress";
            this.updateProgress.Size = new System.Drawing.Size(588, 23);
            this.updateProgress.TabIndex = 1;
            // 
            // modsList
            // 
            this.modsList.FormattingEnabled = true;
            this.modsList.Location = new System.Drawing.Point(12, 33);
            this.modsList.Name = "modsList";
            this.modsList.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.modsList.Size = new System.Drawing.Size(288, 154);
            this.modsList.TabIndex = 2;
            // 
            // btnQuit
            // 
            this.btnQuit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnQuit.Location = new System.Drawing.Point(12, 237);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(288, 36);
            this.btnQuit.TabIndex = 3;
            this.btnQuit.Text = "Quit";
            this.btnQuit.UseVisualStyleBackColor = true;
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // btnStartGame
            // 
            this.btnStartGame.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnStartGame.Location = new System.Drawing.Point(312, 237);
            this.btnStartGame.Name = "btnStartGame";
            this.btnStartGame.Size = new System.Drawing.Size(288, 36);
            this.btnStartGame.TabIndex = 4;
            this.btnStartGame.Text = "Quit and Start the Game";
            this.btnStartGame.UseVisualStyleBackColor = true;
            this.btnStartGame.Click += new System.EventHandler(this.btnStartGame_Click);
            // 
            // logBox
            // 
            this.logBox.Location = new System.Drawing.Point(312, 33);
            this.logBox.Multiline = true;
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            this.logBox.Size = new System.Drawing.Size(288, 154);
            this.logBox.TabIndex = 5;
            // 
            // UpdateView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(612, 282);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.btnStartGame);
            this.Controls.Add(this.btnQuit);
            this.Controls.Add(this.modsList);
            this.Controls.Add(this.updateProgress);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "UpdateView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mod Loader Pro - Mod Updater";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateView_FormClosing);
            this.Shown += new System.EventHandler(this.UpdateView_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar updateProgress;
        private System.Windows.Forms.CheckedListBox modsList;
        private System.Windows.Forms.Button btnQuit;
        private System.Windows.Forms.Button btnStartGame;
        private System.Windows.Forms.TextBox logBox;
    }
}