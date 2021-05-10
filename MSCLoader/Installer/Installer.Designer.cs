
namespace Installer
{
    partial class Installer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Installer));
            this.title = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnWebsite = new System.Windows.Forms.Button();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.labelBadMessage = new System.Windows.Forms.Label();
            this.btnDownload = new System.Windows.Forms.Button();
            this.panelPath = new System.Windows.Forms.Panel();
            this.txtboxPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.labWelcome = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.labVersionInfo = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnPlayNoSteam = new System.Windows.Forms.Button();
            this.btnDevmenu = new System.Windows.Forms.Button();
            this.labWarning = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtModsFolderName = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btnInstallDev = new System.Windows.Forms.Button();
            this.chkDebugger = new System.Windows.Forms.CheckBox();
            this.chkUnityTemplate = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkVSTemplate = new System.Windows.Forms.CheckBox();
            this.labVer = new System.Windows.Forms.Label();
            this.btnLicenses = new System.Windows.Forms.Button();
            this.btnBrowseMods = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabs.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panelPath.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.title.ForeColor = System.Drawing.Color.White;
            this.title.Location = new System.Drawing.Point(252, 3);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(356, 25);
            this.title.TabIndex = 0;
            this.title.Text = "MSC MOD LOADER PRO INSTALLER";
            // 
            // panel1
            // 
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel1.Controls.Add(this.btnWebsite);
            this.panel1.Controls.Add(this.btnMinimize);
            this.panel1.Controls.Add(this.btnExit);
            this.panel1.Controls.Add(this.title);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(776, 35);
            this.panel1.TabIndex = 1;
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
            this.btnWebsite.TabIndex = 3;
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
            this.btnMinimize.Location = new System.Drawing.Point(659, 5);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(53, 25);
            this.btnMinimize.TabIndex = 2;
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
            this.btnExit.Location = new System.Drawing.Point(718, 5);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(53, 25);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "X";
            this.btnExit.UseCompatibleTextRendering = true;
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // panel2
            // 
            this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel2.Controls.Add(this.tabs);
            this.panel2.Location = new System.Drawing.Point(12, 53);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(776, 385);
            this.panel2.TabIndex = 2;
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tabPage1);
            this.tabs.Controls.Add(this.tabPage2);
            this.tabs.Controls.Add(this.tabPage3);
            this.tabs.Controls.Add(this.tabPage4);
            this.tabs.Location = new System.Drawing.Point(3, 3);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(777, 385);
            this.tabs.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Black;
            this.tabPage1.Controls.Add(this.labelBadMessage);
            this.tabPage1.Controls.Add(this.btnDownload);
            this.tabPage1.Controls.Add(this.panelPath);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.labWelcome);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(769, 359);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            // 
            // labelBadMessage
            // 
            this.labelBadMessage.AutoSize = true;
            this.labelBadMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labelBadMessage.ForeColor = System.Drawing.Color.White;
            this.labelBadMessage.Location = new System.Drawing.Point(353, 230);
            this.labelBadMessage.Name = "labelBadMessage";
            this.labelBadMessage.Size = new System.Drawing.Size(49, 25);
            this.labelBadMessage.TabIndex = 7;
            this.labelBadMessage.Text = "blah";
            this.labelBadMessage.Visible = false;
            // 
            // btnDownload
            // 
            this.btnDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnDownload.Enabled = false;
            this.btnDownload.FlatAppearance.BorderSize = 0;
            this.btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnDownload.ForeColor = System.Drawing.Color.White;
            this.btnDownload.Location = new System.Drawing.Point(250, 276);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(230, 35);
            this.btnDownload.TabIndex = 3;
            this.btnDownload.Text = "INSTALL";
            this.btnDownload.UseCompatibleTextRendering = true;
            this.btnDownload.UseVisualStyleBackColor = false;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // panelPath
            // 
            this.panelPath.Controls.Add(this.txtboxPath);
            this.panelPath.Controls.Add(this.btnBrowse);
            this.panelPath.Location = new System.Drawing.Point(21, 177);
            this.panelPath.Name = "panelPath";
            this.panelPath.Size = new System.Drawing.Size(707, 40);
            this.panelPath.TabIndex = 6;
            // 
            // txtboxPath
            // 
            this.txtboxPath.Location = new System.Drawing.Point(3, 3);
            this.txtboxPath.Name = "txtboxPath";
            this.txtboxPath.ReadOnly = true;
            this.txtboxPath.Size = new System.Drawing.Size(642, 20);
            this.txtboxPath.TabIndex = 5;
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnBrowse.FlatAppearance.BorderSize = 0;
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnBrowse.ForeColor = System.Drawing.Color.White;
            this.btnBrowse.Location = new System.Drawing.Point(651, 3);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(53, 25);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseCompatibleTextRendering = true;
            this.btnBrowse.UseVisualStyleBackColor = false;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(16, 82);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(471, 50);
            this.label1.TabIndex = 4;
            this.label1.Text = "In order to start, \r\nplease select your My Summer Car installation folder.";
            // 
            // labWelcome
            // 
            this.labWelcome.AutoSize = true;
            this.labWelcome.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labWelcome.ForeColor = System.Drawing.Color.White;
            this.labWelcome.Location = new System.Drawing.Point(16, 16);
            this.labWelcome.Name = "labWelcome";
            this.labWelcome.Size = new System.Drawing.Size(415, 25);
            this.labWelcome.TabIndex = 3;
            this.labWelcome.Text = "Welcome to My Summer Car Mod Loader Pro!";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.Black;
            this.tabPage2.Controls.Add(this.labVersionInfo);
            this.tabPage2.Controls.Add(this.progressBar);
            this.tabPage2.Controls.Add(this.labelStatus);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(769, 359);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            // 
            // labVersionInfo
            // 
            this.labVersionInfo.AutoSize = true;
            this.labVersionInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labVersionInfo.ForeColor = System.Drawing.Color.White;
            this.labVersionInfo.Location = new System.Drawing.Point(245, 114);
            this.labVersionInfo.Name = "labVersionInfo";
            this.labVersionInfo.Size = new System.Drawing.Size(272, 25);
            this.labVersionInfo.TabIndex = 10;
            this.labVersionInfo.Text = "Now downloading version: 1.0";
            this.labVersionInfo.Visible = false;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(141, 66);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(475, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 9;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labelStatus.ForeColor = System.Drawing.Color.White;
            this.labelStatus.Location = new System.Drawing.Point(322, 14);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(116, 25);
            this.labelStatus.TabIndex = 4;
            this.labelStatus.Text = "StatusLabel";
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.Color.Black;
            this.tabPage3.Controls.Add(this.btnBrowseMods);
            this.tabPage3.Controls.Add(this.btnPlayNoSteam);
            this.tabPage3.Controls.Add(this.btnDevmenu);
            this.tabPage3.Controls.Add(this.labWarning);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.txtModsFolderName);
            this.tabPage3.Controls.Add(this.btnClose);
            this.tabPage3.Controls.Add(this.btnPlay);
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(769, 359);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "tabPage3";
            // 
            // btnPlayNoSteam
            // 
            this.btnPlayNoSteam.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnPlayNoSteam.FlatAppearance.BorderSize = 0;
            this.btnPlayNoSteam.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlayNoSteam.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnPlayNoSteam.ForeColor = System.Drawing.Color.White;
            this.btnPlayNoSteam.Location = new System.Drawing.Point(270, 245);
            this.btnPlayNoSteam.Name = "btnPlayNoSteam";
            this.btnPlayNoSteam.Size = new System.Drawing.Size(230, 35);
            this.btnPlayNoSteam.TabIndex = 10;
            this.btnPlayNoSteam.Text = "START GAME (NO STEAM)";
            this.btnPlayNoSteam.UseCompatibleTextRendering = true;
            this.btnPlayNoSteam.UseVisualStyleBackColor = false;
            this.btnPlayNoSteam.Click += new System.EventHandler(this.btnPlayNoSteam_Click);
            // 
            // btnDevmenu
            // 
            this.btnDevmenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnDevmenu.FlatAppearance.BorderSize = 0;
            this.btnDevmenu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDevmenu.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnDevmenu.ForeColor = System.Drawing.Color.White;
            this.btnDevmenu.Location = new System.Drawing.Point(270, 337);
            this.btnDevmenu.Name = "btnDevmenu";
            this.btnDevmenu.Size = new System.Drawing.Size(230, 35);
            this.btnDevmenu.TabIndex = 9;
            this.btnDevmenu.Text = "INSTALL DEV TOOLS";
            this.btnDevmenu.UseCompatibleTextRendering = true;
            this.btnDevmenu.UseVisualStyleBackColor = false;
            this.btnDevmenu.Click += new System.EventHandler(this.btnDevmenu_Click);
            // 
            // labWarning
            // 
            this.labWarning.AutoSize = true;
            this.labWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labWarning.ForeColor = System.Drawing.Color.White;
            this.labWarning.Location = new System.Drawing.Point(218, 176);
            this.labWarning.Name = "labWarning";
            this.labWarning.Size = new System.Drawing.Size(336, 25);
            this.labWarning.TabIndex = 8;
            this.labWarning.Text = "(We recommend to leave it at default)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(293, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(184, 25);
            this.label3.TabIndex = 7;
            this.label3.Text = "Mods Folder Name:";
            // 
            // txtModsFolderName
            // 
            this.txtModsFolderName.Location = new System.Drawing.Point(201, 140);
            this.txtModsFolderName.Name = "txtModsFolderName";
            this.txtModsFolderName.Size = new System.Drawing.Size(374, 20);
            this.txtModsFolderName.TabIndex = 6;
            this.txtModsFolderName.Text = "Mods";
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(270, 286);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(230, 35);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "CLOSE";
            this.btnClose.UseCompatibleTextRendering = true;
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // btnPlay
            // 
            this.btnPlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnPlay.FlatAppearance.BorderSize = 0;
            this.btnPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlay.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnPlay.ForeColor = System.Drawing.Color.White;
            this.btnPlay.Location = new System.Drawing.Point(270, 204);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(230, 35);
            this.btnPlay.TabIndex = 4;
            this.btnPlay.Text = "START GAME";
            this.btnPlay.UseCompatibleTextRendering = true;
            this.btnPlay.UseVisualStyleBackColor = false;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(148, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(482, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "MSC Mod Loader Pro has been successfully installed!";
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.Color.Black;
            this.tabPage4.Controls.Add(this.btnInstallDev);
            this.tabPage4.Controls.Add(this.chkDebugger);
            this.tabPage4.Controls.Add(this.chkUnityTemplate);
            this.tabPage4.Controls.Add(this.label4);
            this.tabPage4.Controls.Add(this.chkVSTemplate);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(769, 359);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "tabPage4";
            // 
            // btnInstallDev
            // 
            this.btnInstallDev.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnInstallDev.FlatAppearance.BorderSize = 0;
            this.btnInstallDev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstallDev.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnInstallDev.ForeColor = System.Drawing.Color.White;
            this.btnInstallDev.Location = new System.Drawing.Point(266, 261);
            this.btnInstallDev.Name = "btnInstallDev";
            this.btnInstallDev.Size = new System.Drawing.Size(230, 35);
            this.btnInstallDev.TabIndex = 7;
            this.btnInstallDev.Text = "INSTALL";
            this.btnInstallDev.UseCompatibleTextRendering = true;
            this.btnInstallDev.UseVisualStyleBackColor = false;
            this.btnInstallDev.Click += new System.EventHandler(this.btnInstallDev_Click);
            // 
            // chkDebugger
            // 
            this.chkDebugger.AutoSize = true;
            this.chkDebugger.BackColor = System.Drawing.Color.Transparent;
            this.chkDebugger.ForeColor = System.Drawing.Color.White;
            this.chkDebugger.Location = new System.Drawing.Point(269, 164);
            this.chkDebugger.Name = "chkDebugger";
            this.chkDebugger.Size = new System.Drawing.Size(73, 17);
            this.chkDebugger.TabIndex = 6;
            this.chkDebugger.Text = "Debugger";
            this.chkDebugger.UseVisualStyleBackColor = false;
            // 
            // chkUnityTemplate
            // 
            this.chkUnityTemplate.AutoSize = true;
            this.chkUnityTemplate.BackColor = System.Drawing.Color.Transparent;
            this.chkUnityTemplate.ForeColor = System.Drawing.Color.White;
            this.chkUnityTemplate.Location = new System.Drawing.Point(269, 118);
            this.chkUnityTemplate.Name = "chkUnityTemplate";
            this.chkUnityTemplate.Size = new System.Drawing.Size(97, 17);
            this.chkUnityTemplate.TabIndex = 5;
            this.chkUnityTemplate.Text = "Unity Template";
            this.chkUnityTemplate.UseVisualStyleBackColor = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(169, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(252, 25);
            this.label4.TabIndex = 4;
            this.label4.Text = "Developer Tools Installation";
            // 
            // chkVSTemplate
            // 
            this.chkVSTemplate.AutoSize = true;
            this.chkVSTemplate.BackColor = System.Drawing.Color.Transparent;
            this.chkVSTemplate.ForeColor = System.Drawing.Color.White;
            this.chkVSTemplate.Location = new System.Drawing.Point(269, 72);
            this.chkVSTemplate.Name = "chkVSTemplate";
            this.chkVSTemplate.Size = new System.Drawing.Size(207, 17);
            this.chkVSTemplate.TabIndex = 0;
            this.chkVSTemplate.Text = "Microsoft Visual Studio 2019 Template";
            this.chkVSTemplate.UseVisualStyleBackColor = false;
            // 
            // labVer
            // 
            this.labVer.AutoSize = true;
            this.labVer.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labVer.ForeColor = System.Drawing.Color.White;
            this.labVer.Location = new System.Drawing.Point(14, 441);
            this.labVer.Name = "labVer";
            this.labVer.Size = new System.Drawing.Size(39, 25);
            this.labVer.TabIndex = 4;
            this.labVer.Text = "1.0";
            this.labVer.Click += new System.EventHandler(this.labVer_Click);
            // 
            // btnLicenses
            // 
            this.btnLicenses.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnLicenses.FlatAppearance.BorderSize = 0;
            this.btnLicenses.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLicenses.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnLicenses.ForeColor = System.Drawing.Color.White;
            this.btnLicenses.Location = new System.Drawing.Point(556, 441);
            this.btnLicenses.Name = "btnLicenses";
            this.btnLicenses.Size = new System.Drawing.Size(232, 29);
            this.btnLicenses.TabIndex = 8;
            this.btnLicenses.Text = "THIRD-PARTY LICENSES";
            this.btnLicenses.UseCompatibleTextRendering = true;
            this.btnLicenses.UseVisualStyleBackColor = false;
            this.btnLicenses.Click += new System.EventHandler(this.btnLicenses_Click);
            // 
            // btnBrowseMods
            // 
            this.btnBrowseMods.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(152)))), ((int)(((byte)(129)))));
            this.btnBrowseMods.FlatAppearance.BorderSize = 0;
            this.btnBrowseMods.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseMods.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.btnBrowseMods.ForeColor = System.Drawing.Color.White;
            this.btnBrowseMods.Location = new System.Drawing.Point(581, 140);
            this.btnBrowseMods.Name = "btnBrowseMods";
            this.btnBrowseMods.Size = new System.Drawing.Size(53, 25);
            this.btnBrowseMods.TabIndex = 11;
            this.btnBrowseMods.Text = "...";
            this.btnBrowseMods.UseCompatibleTextRendering = true;
            this.btnBrowseMods.UseVisualStyleBackColor = false;
            this.btnBrowseMods.Click += new System.EventHandler(this.btnBrowseMods_Click);
            // 
            // Installer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(101)))), ((int)(((byte)(34)))), ((int)(((byte)(18)))));
            this.ClientSize = new System.Drawing.Size(800, 474);
            this.Controls.Add(this.btnLicenses);
            this.Controls.Add(this.labVer);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Installer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MSC Mod Loader Pro Installer";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tabs.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.panelPath.ResumeLayout(false);
            this.panelPath.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label labWelcome;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtboxPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Panel panelPath;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Label labelBadMessage;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnWebsite;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtModsFolderName;
        private System.Windows.Forms.Label labWarning;
        private System.Windows.Forms.Label labVer;
        private System.Windows.Forms.Label labVersionInfo;
        private System.Windows.Forms.Button btnDevmenu;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.CheckBox chkVSTemplate;
        private System.Windows.Forms.CheckBox chkDebugger;
        private System.Windows.Forms.CheckBox chkUnityTemplate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnInstallDev;
        private System.Windows.Forms.Button btnLicenses;
        private System.Windows.Forms.Button btnPlayNoSteam;
        private System.Windows.Forms.Button btnBrowseMods;
    }
}

