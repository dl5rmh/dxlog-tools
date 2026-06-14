using System;

namespace DXLog.net
{
    public class SetCWSpeed : ScriptClass
    {
        FrmMain mainForm;

        public void Initialize(FrmMain main)
        {
            mainForm = main;
        }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            int targetSpeed = 19;
            int radio = cdata.FocusedRadio;

            if (main._cwKeyer != null)
                main._cwKeyer.SetCWSpeed(radio, targetSpeed);
        }
    }
}
