using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DtcDesk.WinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) => LogAndShowError("UI thread exception", e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                LogAndShowError("Unhandled exception", ex);
            };

            try
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                LogAndShowError("Fatal startup exception", ex);
            }
        }

        private static void LogAndShowError(string title, Exception? ex)
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var logPath = Path.Combine(baseDir, "startup.log");
                var message = new StringBuilder()
                    .AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {title}")
                    .AppendLine(ex?.ToString() ?? "<no exception>")
                    .AppendLine(new string('-', 80))
                    .ToString();

                File.AppendAllText(logPath, message, Encoding.UTF8);
                MessageBox.Show(
                    $"Se produjo un error al iniciar la app.\n\nDetalles:\n{ex?.Message}\n\nLog: {logPath}",
                    "Error de inicio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
                // Swallow any logging errors to avoid recursive failures.
            }
        }
    }
}
