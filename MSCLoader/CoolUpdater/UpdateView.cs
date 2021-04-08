using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Ionic.Zip;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoolUpdater
{
    public partial class UpdateView : Form
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        string modsPath;

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
           IntPtr pdv, [In] ref uint pcFonts);

        private PrivateFontCollection fonts = new PrivateFontCollection();

        Font myFont;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        readonly Color colorBtn = Color.FromArgb(255, 199, 152, 129);

        public UpdateView(string modsPath)
        {
            InitializeComponent();
            this.modsPath = modsPath;
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            byte[] fontData = Properties.Resources.FugazOne_Regular;
            IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
            Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            uint dummy = 0;
            fonts.AddMemoryFont(fontPtr, Properties.Resources.FugazOne_Regular.Length);
            AddFontMemResourceEx(fontPtr, (uint)Properties.Resources.FugazOne_Regular.Length, IntPtr.Zero, ref dummy);
            Marshal.FreeCoTaskMem(fontPtr);

            myFont = new Font(fonts.Families[0], 16.0F);

            title.Font = myFont;
            title.SetToCenter(this);
            title.BackColor = Color.Transparent;
            title.MouseMove += DragWindowByThis;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            labVer.Text = "RC-" + version.Major + "." + version.Minor;
            if (version.Build != 0)
            {
                labVer.Text += "." + version.Build;
            }

            panel1.Paint += WhiteBorder;
            panel1.MouseMove += DragWindowByThis;

            panel2.Paint += WhiteBorder;

            foreach (Control control in GetAllControls(this, typeof(Button)))
            {
                control.Font = myFont;
                (control as Button).TextAlign = ContentAlignment.MiddleCenter;
                (control as Button).BackColor = colorBtn;
                (control as Button).ForeColor = Color.Yellow;
                (control as Button).UseCompatibleTextRendering = true;
                (control as Button).FlatStyle = FlatStyle.Flat;
                (control as Button).FlatAppearance.BorderSize = 0;
            }

            foreach (Control control in GetAllControls(this, typeof(Label)))
            {
                control.Font = myFont;
                control.BackColor = Color.Transparent;
                (control as Label).ForeColor = Color.White;
                if (control.Name == "labVer") continue;
                control.SetToCenter(this);
                (control as Label).TextAlign = ContentAlignment.MiddleCenter;
            }

            foreach (Control control in GetAllControls(this, typeof(TextBox)))
            {
                (control as TextBox).BackColor = colorBtn;
                (control as TextBox).ForeColor = Color.White;
                (control as TextBox).BorderStyle = BorderStyle.None;
            }

            foreach (Control control in GetAllControls(this, typeof(ProgressBar)))
            {
                (control as ProgressBar).ForeColor = Color.Yellow;
                control.BackColor = colorBtn;
                //control.SetToCenter(this);
                (control as ProgressBar).Style = ProgressBarStyle.Continuous;
            }

            foreach (Control control in GetAllControls(this, typeof(CheckedListBox)))
            {
                (control as CheckedListBox).ForeColor = Color.White;
                control.BackColor = colorBtn;
                //control.SetToCenter(this);
            }

            btnQuit.SetToCenter(modsList);
            btnStartGame.SetToCenter(logBox);
            btnExit.Click += btnQuit_Click;
            btnExit.ForeColor = Color.Red;
        }

        private void UpdateView_Shown(object sender, EventArgs e)
        {
            UpdateAll();
        }

        string GetSeconds(Stopwatch s)
        {
            return Math.Round(s.Elapsed.TotalSeconds, 2).ToString() + "s";
        }

        private async void UpdateAll()
        {
            ToggleButtons(false);
            try
            {
                var stopwatch = Stopwatch.StartNew();
                Log($"({GetSeconds(stopwatch)}) Mod Loader Pro Auto-Update Tool Initialized!\n");

                await Task.Run(() => Thread.Sleep(500));

                Process[] mscProcess = Process.GetProcessesByName("mysummercar");
                if (mscProcess.Length > 0)
                {
                    await Task.Run(() =>
                    {
                        foreach (Process process in mscProcess)
                        {
                            Log($"({GetSeconds(stopwatch)}) Killing process: {process.ProcessName}...");
                            process.Kill();
                        }
                        Log($"({GetSeconds(stopwatch)}) MSC process(es) have been killed!\n");
                    });
                }

                if (!Directory.Exists(Program.Downloads))
                {
                    throw new DirectoryNotFoundException("Downloads folder doesn't exist!");
                }

                // A small workaround for "not accessible files".
                Log($"({GetSeconds(stopwatch)}) Loading update procedure...\n");
                await Task.Run(() => Thread.Sleep(1000));

                DirectoryInfo di = new DirectoryInfo(Program.Downloads);
                FileInfo[] files = di.GetFiles("*.zip");

                foreach (var f in files)
                {
                    modsList.Items.Add(f.Name.Split('.')[0]);
                }

                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i];
                    try
                    {
                        await Task.Run(() =>
                        {
                            Log($"({GetSeconds(stopwatch)}) ({i + 1}/{files.Length}) Unpacking {file.Name}...");
                            using (ZipFile zip = ZipFile.Read(file.FullName))
                            {
                                zip.ExtractAll(modsPath, ExtractExistingFileAction.OverwriteSilently);
                            }
                            Log($"({GetSeconds(stopwatch)}) {file.Name} unpacking completed!\n");
                            modsList.SetItemChecked(i, true);
                        });
                    }
                    catch (Exception ex)
                    {
                        Log($"\n===============================================\nAn error has occured while extracting {file.Name} :(\n\n" + ex.ToString() + "\n\n");
                    }

                    updateProgress.Value = (int)(((double)i + 1) / files.Length * 100);
                }

                // Cleanup after install.
                Directory.Delete(Program.Downloads, true);

                Log($"({GetSeconds(stopwatch)}) All mods have been updated, have a nice day :)");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
            ToggleButtons(true);
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            // Restart via .exe
            string args = "/C start \"\" \"steam://rungameid/516750\"";
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                args = "/C start \"\" \"..\\mysummercar.exe\"";
            }

            Log($"Restarting game now using Steam");
            Process cmd = new Process();
            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            cmd.Start();

            Environment.Exit(0);
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        void ToggleButtons(bool enabled)
        {
            btnQuit.Enabled = enabled;
            btnStartGame.Enabled = enabled;
            btnExit.Enabled = enabled;  
        }

        private void UpdateView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnQuit.Enabled)
            {
                e.Cancel = true;
            }
        }

        void Log(string s)
        {
            logBox.Text += s + Environment.NewLine;
        }

        public IEnumerable<Control> GetAllControls(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAllControls(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        private void DragWindowByThis(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void WhiteBorder(object sender, PaintEventArgs e)
        {
            int thickness = 3;
            int halfThickness = thickness / 2;
            using (Pen p = new Pen(Color.White, thickness))
            {
                e.Graphics.DrawRectangle(p, new Rectangle(halfThickness,
                                                          halfThickness,
                                                          (sender as Control).ClientSize.Width - thickness,
                                                          (sender as Control).ClientSize.Height - thickness));
            }
        }
        
        private const int CS_DROPSHADOW = 0x20000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnWebsite_Click(object sender, EventArgs e)
        {
            Process.Start("https://mscloaderpro.github.io/docs/");
        }
    }

    public static class CustomExtensions
    {
        public static void SetToCenter(this Control control, Form form)
        {
            int x = form.Width / 2 - control.Width / 2 - 12;
            control.Location = new Point(x, control.Location.Y);
        }

        public static void SetToCenter(this Control control, Control other)
        {
            int x = (other.Left + other.Width / 2) - control.Width / 2;
            control.Location = new Point(x, control.Location.Y);
        }
    }
}
