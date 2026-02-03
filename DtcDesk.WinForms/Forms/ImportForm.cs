using CsvHelper;
using CsvHelper.Configuration;
using DtcDesk.Core.Models;
using DtcDesk.Data.Db;
using DtcDesk.Data.Repositories;
using System.Globalization;

namespace DtcDesk.WinForms;

public partial class ImportForm : Form
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly DtcRepository _repository;
    private string? _selectedFilePath;

    public ImportForm()
    {
        InitializeComponent();

        var dbPath = ConnectionFactory.GetDefaultDatabasePath();
        _connectionFactory = new ConnectionFactory(dbPath);
        _repository = new DtcRepository(_connectionFactory);

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
            Filter = "Archivos CSV|*.csv|Todos los archivos|*.*",
            Title = "Seleccionar archivo CSV con códigos DTC"
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

    private List<DtcCode> ReadCsvFile(string filePath)
    {
        var codes = new List<DtcCode>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            try
            {
                var code = new DtcCode
                {
                    Code = csv.GetField<string>("Code")?.ToUpperInvariant() ?? "",
                    Description = csv.GetField<string>("Description") ?? "",
                    Category = csv.GetField<string>("Category"),
                    Source = csv.GetField<string>("Source"),
                    Notes = csv.GetField<string>("Notes")
                };

                if (!string.IsNullOrWhiteSpace(code.Code) && !string.IsNullOrWhiteSpace(code.Description))
                {
                    codes.Add(code);
                }
            }
            catch
            {
                // Ignorar filas con errores
                continue;
            }
        }

        return codes;
    }
}
