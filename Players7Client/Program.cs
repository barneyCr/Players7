using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Players7Client
{
    static class Program
    {
        public static Action Callback = null;
        public static Properties.Settings GlobalSettings = global::Players7Client.Properties.Settings.Default;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
            if (Callback != null)
                Callback();
        }
    }
}
