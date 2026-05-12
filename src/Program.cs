using System;
using System.Windows.Forms;
using CyberSecurityAwarenessBot.UI;

namespace CyberSecurityAwarenessBot
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
            Application.Run(new MainForm());
        }
    }
}
