using System;
using System.Windows.Forms;
using CWKeyer;

// Part of DXLog.net Tools – scripts, custom windows, and utilities for DXLog.net Contest Logging Software
// Contributions welcome: https://github.com/dl5rmh/dxlog-tools

namespace DXLog.net
{
    public class CWSpeedSet : ScriptClass
    {
        FrmMain mainForm;

        public void Initialize(FrmMain main)
        {
            mainForm = main;
        }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            if (main._cwKeyer == null) return;

            int radioNumber = main._cwKeyer.ActiveRadio;
            int currentSpeed = main._cwKeyer.CWSpeed(radioNumber);

            using (Form dlg = new Form())
            {
                dlg.Text = "CW Speed (WPM)";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.Width = 220;
                dlg.Height = 120;
                dlg.TopMost = true;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                Label lbl = new Label() { Text = "WPM:", Left = 10, Top = 18, Width = 40 };
                TextBox txt = new TextBox() { Text = currentSpeed.ToString(), Left = 60, Top = 15, Width = 80 };
                Button ok = new Button() { Text = "OK", Left = 30, Top = 50, Width = 60, DialogResult = DialogResult.OK };
                Button cancel = new Button() { Text = "Cancel", Left = 110, Top = 50, Width = 60, DialogResult = DialogResult.Cancel };

                dlg.Controls.Add(lbl);
                dlg.Controls.Add(txt);
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                txt.SelectAll();
                txt.Focus();

                if (dlg.ShowDialog() != DialogResult.OK) return;

                int newSpeed;
                if (!int.TryParse(txt.Text.Trim(), out newSpeed)) return;

                if (newSpeed < main._cwKeyer.CWMinimumSpeed) newSpeed = main._cwKeyer.CWMinimumSpeed;
                if (newSpeed > main._cwKeyer.CWMaximumSpeed) newSpeed = main._cwKeyer.CWMaximumSpeed;

                main._cwKeyer.SetCWSpeed(radioNumber, newSpeed);

                CATCommon radio = main.COMMainProvider.RadioObject(radioNumber);
                if (radio == null) return;

                radio.SendCustomCommand(String.Format("KS{0:D3};", newSpeed));
            }
        }
    }
}