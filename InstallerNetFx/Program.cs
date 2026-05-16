using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;

internal static class Program
{
    private const string AppName = "DTCDesk";
    private const string ExeName = "DTCDesk.exe";

    [STAThread]
    private static void Main()
    {
        try
        {
            string installDir = Path.Combine(
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

            string exePath = Path.Combine(installDir, ExeName);
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
        Assembly assembly = Assembly.GetExecutingAssembly();
        using (Stream payload = assembly.GetManifestResourceStream("Payload.zip"))
        {
            if (payload == null)
            {
                throw new InvalidOperationException("No se encontro el paquete interno de instalacion.");
            }

            using (ZipArchive archive = new ZipArchive(payload, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.Name))
                    {
                        continue;
                    }

                    string destination = Path.GetFullPath(Path.Combine(installDir, entry.FullName));
                    string safeRoot = installDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    if (!destination.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("El paquete contiene una ruta no valida.");
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    using (Stream source = entry.Open())
                    using (FileStream target = new FileStream(destination, FileMode.Create, FileAccess.Write))
                    {
                        source.CopyTo(target);
                    }
                }
            }
        }
    }

    private static void CreateShortcut(string folder, string installDir)
    {
        Directory.CreateDirectory(folder);

        string exePath = Path.Combine(installDir, ExeName);
        string shortcutPath = Path.Combine(folder, AppName + ".lnk");
        Type shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType == null)
        {
            throw new COMException("No se pudo abrir WScript.Shell.");
        }

        object shell = Activator.CreateInstance(shellType);
        object shortcut = shellType.InvokeMember(
            "CreateShortcut",
            BindingFlags.InvokeMethod,
            null,
            shell,
            new object[] { shortcutPath });

        Type shortcutType = shortcut.GetType();
        shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { exePath });
        shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { installDir });
        shortcutType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut, new object[] { exePath });
        shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
    }

    private static void ShowMessage(string text, string caption, uint icon)
    {
        MessageBoxW(IntPtr.Zero, text, caption, 0x00000000 | icon);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
