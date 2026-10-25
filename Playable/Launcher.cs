using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace KantoSmashLauncher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string htmlPath = Path.Combine(baseDir, "Playable", "index.html");

            if (!File.Exists(htmlPath))
            {
                MessageBox.Show("No se encontró Playable\\index.html en: " + htmlPath, "Kanto Smash", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string fileUri = new Uri(htmlPath).AbsoluteUri;

            // Intentar abrir como ventana de aplicación de escritorio dedicada (sin barra de direcciones)
            string[] appBrowsers = new string[]
            {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
            };

            foreach (string browser in appBrowsers)
            {
                if (File.Exists(browser))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = browser,
                        Arguments = string.Format("--app=\"{0}\" --window-size=1366,800", fileUri),
                        UseShellExecute = true
                    });
                    return;
                }
            }

            // Respaldo: abrir con el visor por defecto del sistema
            Process.Start(new ProcessStartInfo
            {
                FileName = htmlPath,
                UseShellExecute = true
            });
        }
    }
}
