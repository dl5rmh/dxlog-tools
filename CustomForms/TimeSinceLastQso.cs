using System;
using System.Drawing;
using System.Windows.Forms;
using DXLog.net;

// Part of DXLog.net Tools – scripts, custom windows, and utilities for DXLog.net Contest Logging Software
// Contributions welcome: https://github.com/dl5rmh/dxlog-tools

namespace DXLog.net
{
    public partial class TimeSinceLastQso : KForm
    {
        public static string CusWinName
        {
            get { return "Time since last QSO"; }
        }

        public static int CusFormID
        {
            get { return 2001; }
        }

        private const int CapSeconds = 180;
        private const double GreenUntil = 30;
        private const double WarningAt = 60;
        private const double WarningUntil = 90;

        private ContestData _cdata = null;
        private FrmMain mainForm = null;
        private Font _windowFont = new Font("Courier New", 10, FontStyle.Regular);

        private DateTime _lastQsoTimeUtc;
        private bool _hasQso = false;
        private System.Windows.Forms.Timer _tickTimer;
        private Label lbInfo;

        private Color _bgColor = Color.Black;
        private Color _colorOk = Color.Lime;
        private Color _colorWarning = Color.Yellow;
        private Color _colorAlert = Color.Red;

        public TimeSinceLastQso()
        {
            InitializeComponent();
        }

        public TimeSinceLastQso(ContestData cdata)
        {
            InitializeComponent();

            ColorSetTypes = new string[] { "Background", "Color", "Warning", "Alert" };
            DefaultColors = new Color[] { Color.Black, Color.Lime, Color.Yellow, Color.Red };

            _cdata = cdata;
            FormLayoutChangeEvent += new FormLayoutChange(Handle_FormLayoutChangeEvent);

            _tickTimer = new System.Windows.Forms.Timer();
            _tickTimer.Interval = 1000;
            _tickTimer.Tick += new EventHandler(TickTimer_Tick);
        }

        private void Handle_FormLayoutChangeEvent()
        {
            InitializeLayout();
        }

        public override void InitializeLayout()
        {
            base.InitializeLayout(_windowFont);

            _windowFont = new Font(base.FormLayout.FontName, base.FormLayout.FontSize, FontStyle.Bold);
            lbInfo.Font = _windowFont;

            if (ColorValues != null && ColorValues.Length >= 4)
            {
                _bgColor = ColorValues[0];
                _colorOk = ColorValues[1];
                _colorWarning = ColorValues[2];
                _colorAlert = ColorValues[3];
            }

            this.BackColor = _bgColor;
            lbInfo.BackColor = _bgColor;
            lbInfo.ForeColor = ComputeColor(_hasQso ? (DateTime.UtcNow - _lastQsoTimeUtc).TotalSeconds : 0);

            if (mainForm == null)
            {
                mainForm = (FrmMain)(ParentForm == null ? Owner : ParentForm);
                if (mainForm != null)
                    mainForm.NewQSOSaved += new FrmMain.NewQSOSavedEvent(MainForm_NewQSOSaved);
            }

            base.Text = "Time since last QSO";
        }

        private void MainForm_NewQSOSaved(DXQSO newQso)
        {
            if (InvokeRequired)
            {
                Invoke(new FrmMain.NewQSOSavedEvent(MainForm_NewQSOSaved), new object[] { newQso });
                return;
            }

            _hasQso = true;
            _lastQsoTimeUtc = newQso.QSOTime;
            UpdateDisplay(0);

            if (!_tickTimer.Enabled)
                _tickTimer.Start();
        }

        private void TickTimer_Tick(object sender, EventArgs e)
        {
            double elapsed = (DateTime.UtcNow - _lastQsoTimeUtc).TotalSeconds;
            UpdateDisplay(elapsed);
        }

        private void UpdateDisplay(double elapsedSeconds)
        {
            bool capped = elapsedSeconds > CapSeconds;
            lbInfo.Text = capped ? ">" + CapSeconds : ((int)elapsedSeconds).ToString();
            lbInfo.ForeColor = ComputeColor(elapsedSeconds);
        }

        private Color ComputeColor(double elapsedSeconds)
        {
            if (elapsedSeconds < GreenUntil)
                return _colorOk;
            if (elapsedSeconds < WarningAt)
                return Lerp(_colorOk, _colorWarning, (elapsedSeconds - GreenUntil) / (WarningAt - GreenUntil));
            if (elapsedSeconds < WarningUntil)
                return _colorWarning;
            if (elapsedSeconds < CapSeconds)
                return Lerp(_colorWarning, _colorAlert, (elapsedSeconds - WarningUntil) / (CapSeconds - WarningUntil));
            return _colorAlert;
        }

        private static Color Lerp(Color from, Color to, double t)
        {
            t = Math.Max(0.0, Math.Min(1.0, t));
            int r = (int)(from.R + (to.R - from.R) * t);
            int g = (int)(from.G + (to.G - from.G) * t);
            int b = (int)(from.B + (to.B - from.B) * t);
            return Color.FromArgb(r, g, b);
        }

        private void InitializeComponent()
        {
            this.lbInfo = new Label();
            this.SuspendLayout();

            this.lbInfo.Font = new Font("Consolas", 24F, FontStyle.Bold);
            this.lbInfo.ForeColor = _colorOk;
            this.lbInfo.BackColor = _bgColor;
            this.lbInfo.TextAlign = ContentAlignment.MiddleCenter;
            this.lbInfo.Dock = DockStyle.Fill;
            this.lbInfo.Text = "--";

            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = _bgColor;
            this.ClientSize = new Size(140, 80);
            this.Controls.Add(this.lbInfo);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.FormID = 2001;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TimeSinceLastQso";
            this.Text = "Time since last QSO";

            this.ResumeLayout(false);
        }
    }
}
