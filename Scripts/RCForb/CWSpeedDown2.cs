using System;
using System.Windows.Forms;
using CWKeyer;

// Part of DXLog.net Tools – scripts, custom windows, and utilities for DXLog.net Contest Logging Software
// Contributions welcome: https://github.com/dl5rmh/dxlog-tools

namespace DXLog.net
{
    public class CWSpeedDown2 : ScriptClass
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
            int newSpeed = main._cwKeyer.CWSpeed(radioNumber) - 2;

            // Untergrenze respektieren
            if (newSpeed < main._cwKeyer.CWMinimumSpeed)
                newSpeed = main._cwKeyer.CWMinimumSpeed;

            // DXLog intern auf neue Speed setzen
            main._cwKeyer.SetCWSpeed(radioNumber, newSpeed);

            // Und per CAT ans Radio (Elecraft KS-Kommando, 3-stellig)
            CATCommon radio = main.COMMainProvider.RadioObject(radioNumber);
            if (radio == null) return;

            radio.SendCustomCommand(String.Format("KS{0:D3};", newSpeed));
        }
    }
}
