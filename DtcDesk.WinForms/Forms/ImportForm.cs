using DtcDesk.Core.Models;
using DtcDesk.Data.Db;
using DtcDesk.Data.Repositories;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace DtcDesk.WinForms;

public partial class ImportForm : Form
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly DtcRepository _repository;
    private readonly ModuleFilterRepository _moduleRepository;
    private string? _selectedFilePath;

    public bool ModulesImported { get; private set; }

    public ImportForm()
    {
        InitializeComponent();

        var dbPath = ConnectionFactory.GetDefaultDatabasePath();
        _connectionFactory = new ConnectionFactory(dbPath);
        _repository = new DtcRepository(_connectionFactory);
        _moduleRepository = new ModuleFilterRepository(_connectionFactory.GetConnectionString());

        SetupUI();
    }

    private void SetupUI()
    {
        ApplyDarkTheme();

        btnSelectFile.Click += BtnSelectFile_Click;
        btnImport.Click += BtnImport_Click;
        btnCancel.Click += (s, e) => this.Close();

        btnImport.Enabled = false;
    }

    private void ApplyDarkTheme()
    {
        var bgMain = ColorTranslator.FromHtml("#0F1E2B");
        var textMain = ColorTranslator.FromHtml("#EAEAEA");
        var accentYellow = ColorTranslator.FromHtml("#F8B41C");
        var accentGreen = ColorTranslator.FromHtml("#5CB85C");
        var separator = ColorTranslator.FromHtml("#2A3B4C");

        this.BackColor = bgMain;
        lblInfo.ForeColor = textMain;
        lblFile.ForeColor = textMain;
        txtFilePath.BackColor = separator;
        txtFilePath.ForeColor = textMain;

        btnSelectFile.BackColor = separator;
        btnSelectFile.ForeColor = textMain;
        btnSelectFile.FlatStyle = FlatStyle.Flat;
        btnSelectFile.FlatAppearance.BorderSize = 0;

        btnImport.BackColor = accentGreen;
        btnImport.ForeColor = Color.White;
        btnImport.FlatStyle = FlatStyle.Flat;
        btnImport.FlatAppearance.BorderSize = 0;

        btnCancel.BackColor = separator;
        btnCancel.ForeColor = textMain;
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderSize = 0;
    }

    private void BtnSelectFile_Click(object? sender, EventArgs e)
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "Archivos CSV o JSON|*.csv;*.json|Archivos CSV|*.csv|Archivos JSON|*.json|Todos los archivos|*.*",
            Title = "Seleccionar archivo CSV o JSON con códigos DTC"
        };

        if (openDialog.ShowDialog() == DialogResult.OK)
        {
            _selectedFilePath = openDialog.FileName;
            txtFilePath.Text = _selectedFilePath;
            btnImport.Enabled = true;
        }
    }

    private async void BtnImport_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedFilePath))
            return;

        try
        {
            Cursor = Cursors.WaitCursor;
            btnImport.Enabled = false;
            btnSelectFile.Enabled = false;

            var extension = Path.GetExtension(_selectedFilePath).ToLowerInvariant();
            if (extension == ".json")
            {
                var package = ReadJsonPackage(_selectedFilePath);
                if (package.Codes.Count == 0)
                {
                    MessageBox.Show("El archivo JSON no contiene códigos válidos.",
                        "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var includesModules = package.Modules != null && package.Modules.Count > 0;
                var moduleInfo = includesModules
                    ? $"\n\nMódulos: {package.Modules!.Count}\nReglas exactas: {package.ExactRules?.Count ?? 0}\nKeywords: {package.Keywords?.Count ?? 0}"
                    : "";

                var result = MessageBox.Show(
                    $"Se encontraron {package.Codes.Count} código(s) en el archivo JSON.{moduleInfo}\n\n¿Deseas importarlos a la base de datos?",
                    "Confirmar importación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var importedCount = await _repository.UpsertManyAsync(package.Codes);

                    if (includesModules)
                    {
                        await _moduleRepository.ImportFiltersAsync(
                            package.Modules!,
                            package.ExactRules ?? new List<DtcModuleRule>(),
                            package.Keywords ?? new List<DtcModuleKeyword>());
                        ModulesImported = true;
                    }

                    MessageBox.Show(
                        $"Importación completada.\n\nTotal procesados: {package.Codes.Count}\nInsertados/actualizados: {importedCount}",
                        "Éxito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                var codes = ReadCsvFile(_selectedFilePath);

                if (codes.Count == 0)
                {
                    MessageBox.Show("El archivo CSV no contiene códigos válidos.",
                        "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Se encontraron {codes.Count} código(s) en el archivo.\n\n¿Deseas importarlos a la base de datos?",
                    "Confirmar importación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var importedCount = await _repository.BulkInsertAsync(codes);

                    MessageBox.Show(
                        $"Importación completada.\n\nTotal procesados: {codes.Count}\nInsertados/actualizados: {importedCount}",
                        "Éxito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al importar: {ex.Message}\n\n{ex.StackTrace}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            btnImport.Enabled = true;
            btnSelectFile.Enabled = true;
        }
    }

    private static DtcExportPackage ReadJsonPackage(string filePath)
    {
        var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var package = JsonSerializer.Deserialize<DtcExportPackage>(json, options) ?? new DtcExportPackage();

        package.Codes = package.Codes
            .Where(c => !string.IsNullOrWhiteSpace(c.Code))
            .Select(c =>
            {
                c.Code = c.Code.Trim().ToUpperInvariant();
                c.ObdType = string.IsNullOrWhiteSpace(c.ObdType) ? "OBD-II" : c.ObdType;
                c.Description = string.IsNullOrWhiteSpace(c.Description) ? "Sin descripción" : c.Description.Trim();
                return c;
            })
            .ToList();

        if (package.Modules != null)
        {
            package.Modules = package.Modules
                .Where(m => !string.IsNullOrWhiteSpace(m.Name) && !string.IsNullOrWhiteSpace(m.DisplayName))
                .Select(m =>
                {
                    m.Name = m.Name.Trim();
                    m.DisplayName = m.DisplayName.Trim();
                    return m;
                })
                .ToList();
        }

        return package;
    }

    private List<DtcCode> ReadCsvFile(string filePath)
    {
        var codes = new List<DtcCode>();

        List<string> lines = new();
        
        try
        {
            // Usar FileStream con FileShare.Read para permitir acceso compartido al archivo
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: false))
            using (var streamReader = new StreamReader(fileStream, System.Text.Encoding.UTF8))
            {
                string? line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
        }
        catch (IOException ex)
        {
            throw new IOException($"No se puede acceder al archivo. Asegúrese de que no está abierto en otro programa.\nDetalles: {ex.Message}", ex);
        }
        
        if (lines.Count <= 1)
            return codes;

        // Saltar cabecera: Code,Description,Category,Source,Notes,FilterTag,Module
        foreach (var rawLine in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            var cols = ParseLooseCsvLine(rawLine);
            if (cols == null)
                continue;

            var rawCode = cols[0].Trim().ToUpperInvariant();
            if (!IsImportCodeValid(rawCode))
                continue;

            var description = cols[1].Trim();

            var code = new DtcCode
            {
                Code = rawCode,
                Description = string.IsNullOrWhiteSpace(description) ? "Sin descripción" : description,
                Category = NullIfEmpty(cols[2]),
                Source = NullIfEmpty(cols[3]),
                Notes = NullIfEmpty(cols[4]),
                FilterTag = NullIfEmpty(cols[5]),
                Module = NullIfEmpty(cols[6])?.ToUpperInvariant()
            };

            codes.Add(code);
        }

        return codes;
    }

    private static string[]? ParseLooseCsvLine(string line)
    {
        var parts = line.Split(',');

        if (parts.Length < 2)
            return null;

        if (parts.Length < 7)
        {
            var padded = new string[7];
            for (var i = 0; i < parts.Length; i++)
                padded[i] = parts[i];

            for (var i = parts.Length; i < 7; i++)
                padded[i] = string.Empty;

            return padded;
        }

        if (parts.Length == 7)
            return parts;

        // Soporta líneas con comas extra en Description sin comillas.
        var code = parts[0];
        var module = parts[^1];
        var filterTag = parts[^2];
        var notes = parts[^3];
        var source = parts[^4];
        var category = parts[^5];
        var description = string.Join(",", parts.Skip(1).Take(parts.Length - 6));

        return new[] { code, description, category, source, notes, filterTag, module };
    }

    private static string? NullIfEmpty(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static bool IsImportCodeValid(string code)
    {
        return Regex.IsMatch(code, @"^(?:[PU][0-9A-F]{4}|[CD][0-9A-F]{3}|[0-9A-F]{4})$", RegexOptions.IgnoreCase);
    }
}
