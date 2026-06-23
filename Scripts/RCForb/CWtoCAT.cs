using System;
using System.Windows.Forms;
using CWKeyer;

// Part of DXLog.net Tools – scripts, custom windows, and utilities for DXLog.net Contest Logging Software
// Contributions welcome: https://github.com/dl5rmh/dxlog-tools

namespace DXLog.net
{
    public class CWtoCAT : ScriptClass
    {
        FrmMain mainForm;

        public void Initialize(FrmMain main)
        {
            mainForm = main;
            if (mainForm._cwKeyer != null)
                mainForm._cwKeyer.CharSentEvent += new CWKey.CharSent(OnCharSent);
        }

        public void Deinitialize()
        {
            if (mainForm._cwKeyer != null)
                mainForm._cwKeyer.CharSentEvent -= OnCharSent;
        }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain) { }

        private void OnCharSent(char newChar, int radioNumber)
        {
            CATCommon radio = mainForm.COMMainProvider.RadioObject(radioNumber);
            if (radio == null) return;

            radio.SendCustomCommand(String.Format("KY {0};", newChar));
        }
    }
}