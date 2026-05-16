using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DTCDeskInstaller;

internal static class Program
{
    private const string AppName = "DTCDesk";
    private const string ExeName = "DTCDesk.exe";

    [STAThread]
    private static void Main()
    {
        try
        {
            var installDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppName);

            Directory.CreateDirectory(installDir);
            ExtractPayload(installDir);
            CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), installDir);
            CreateShortcut(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft",
                "Windows",
                "Start Menu",
                "Programs"), installDir);

            var exePath = Path.Combine(installDir, ExeName);
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = installDir,
                UseShellExecute = true
            });

            ShowMessage(
                "DTCDesk se instalo o actualizo correctamente.\nLos datos del usuario se conservan automaticamente.",
                "Instalacion completada",
                0x00000040);
        }
        catch (Exception ex)
        {
            ShowMessage(
                "No se pudo instalar DTCDesk:\n\n" + ex.Message,
                "Error de instalacion",
                0x00000010);
        }
    }

    private static void ExtractPayload(string installDir)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .First(name => name.EndsWith("Payload.zip", StringComparison.OrdinalIgnoreCase));

        using var payload = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("No se encontro el paquete interno de instalacion.");
        using var archive = new ZipArchive(payload, ZipArchiveMode.Read);

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                continue;
            }

            var destination = Path.GetFullPath(Path.Combine(installDir, entry.FullName));
            if (!destination.StartsWith(installDir, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("El paquete contiene una ruta no valida.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            entry.ExtractToFile(destination, overwrite: true);
        }
    }

    private static void CreateShortcut(string folder, string installDir)
    {
        Directory.CreateDirectory(folder);

        var exePath = Path.Combine(installDir, ExeName);
        var shortcutPath = Path.Combine(folder, AppName + ".lnk");
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new COMException("No se pudo abrir WScript.Shell.");
        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new COMException("No se pudo crear WScript.Shell.");
        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = exePath;
        shortcut.WorkingDirectory = installDir;
        shortcut.IconLocation = exePath;
        shortcut.Save();
    }

    private static void ShowMessage(string text, string caption, uint icon)
    {
        MessageBoxW(IntPtr.Zero, text, caption, 0x00000000 | icon);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
