namespace DXLog.net
{
    public class AddPortableKeyOne : ScriptClass
    {
        public void Initialize(FrmMain main) { }
        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            const string suffix = "/P";

            main.Invoke((System.Action)(() =>
            {
                System.Windows.Forms.SendKeys.SendWait("^a");
                System.Windows.Forms.SendKeys.SendWait("^c");

                string call = "";
                try { call = System.Windows.Forms.Clipboard.GetText().Trim().ToUpper(); }
                catch { }

                if (string.IsNullOrEmpty(call) || call.Contains(" ") || call.Length > 20)
                {
                    return;
                }

                if (call.EndsWith(suffix))
                {
                    main.SetMainStatusText("AddPortable: /P bereits vorhanden.");
                    System.Windows.Forms.SendKeys.SendWait("{END}");
                    return;
                }

                string newCall = call + suffix;
                System.Windows.Forms.Clipboard.SetText(newCall);
                System.Windows.Forms.SendKeys.SendWait("^a");
                System.Windows.Forms.SendKeys.SendWait("^v");
                System.Windows.Forms.SendKeys.SendWait("{END}");
            }));
        }
    }
}
