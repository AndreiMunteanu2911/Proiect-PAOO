using System;
using System.Windows.Forms;

namespace ProiectPAOO
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
        /*
        private static void ApplyDarkTheme()
        {
            foreach (var prop in typeof(SystemColors).GetProperties())
            {
                if (prop.PropertyType == typeof(System.Drawing.Color))
                {
                    if (prop.Name.Contains("Window") || prop.Name.Contains("Control"))
                    {
                        typeof(SystemColors).GetProperty(prop.Name)?.SetValue(null, System.Drawing.Color.FromArgb(43, 43, 43));
                    }
                    else if (prop.Name.Contains("Text"))
                    {
                        typeof(SystemColors).GetProperty(prop.Name)?.SetValue(null, System.Drawing.Color.White);
                    }
                }
            }
        }
        */
    }
}