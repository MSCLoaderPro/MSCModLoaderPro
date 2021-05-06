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
    public partial class Info : Form
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

        public Info()
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
            labVer.Text = version.Major + "." + version.Minor;
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

            btnQuit.SetToCenter(this);
            btnExit.Click += btnQuit_Click;
            btnExit.ForeColor = Color.Red;

            Font smallFont = new Font(myFont.FontFamily, 12, myFont.Style);
            btnWebsite.Font = smallFont;
        }

        private void UpdateView_Shown(object sender, EventArgs e)
        {
        }

        string GetSeconds(Stopwatch s)
        {
            return Math.Round(s.Elapsed.TotalSeconds, 2).ToString() + "s";
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }


        private void UpdateView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnQuit.Enabled)
            {
                e.Cancel = true;
            }
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

        int x = 0;
        private void labVer_Click(object sender, EventArgs e)
        {
            x++;
            if (x >= 10)
            {
                labInfo.Text = "You are a curious person, aren't you?";
                labInfo.SetToCenter(this);
            }

        }
    }
}
