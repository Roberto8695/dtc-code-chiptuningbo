using DtcDesk.Core.Models;
using DtcDesk.Data.Db;
using DtcDesk.Data.Repositories;
using System.Text;
using System.Text.Json;

namespace DtcDesk.WinForms;

public partial class ExportForm : Form
{
    private readonly DtcRepository _repository;
    private readonly ModuleFilterRepository _moduleRepository;

    public ExportForm()
    {
        InitializeComponent();

        var dbPath = ConnectionFactory.GetDefaultDatabasePath();
        var connectionFactory = new ConnectionFactory(dbPath);
        _repository = new DtcRepository(connectionFactory);
        _moduleRepository = new ModuleFilterRepository(connectionFactory.GetConnectionString());

        SetupUI();
    }

    private void SetupUI()
    {
        ApplyDarkTheme();

        btnExportJson.Click += async (s, e) => await ExportBackupAsync();
        btnCancel.Click += (s, e) => this.Close();

        lblInfo.Text = "Exportar backup DTC";
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
        lblFormat.ForeColor = textMain;

        chkIncludeModules.ForeColor = textMain;

        btnCancel.BackColor = separator;
        btnCancel.ForeColor = textMain;
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderSize = 0;

        btnExportJson.BackColor = accentYellow;
        btnExportJson.ForeColor = Color.Black;
        btnExportJson.FlatStyle = FlatStyle.Flat;
        btnExportJson.FlatAppearance.BorderSize = 0;
    }

    private async Task ExportBackupAsync()
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "Archivo JSON|*.json",
            FileName = $"dtc_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json"
        };

        if (saveDialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            var exportSummary = await ExportToJsonAsync(saveDialog.FileName, chkIncludeModules.Checked);
            var moduleInfo = exportSummary.Modules > 0
                ? $"\nMódulos: {exportSummary.Modules}\nReglas exactas: {exportSummary.ExactRules}\nKeywords: {exportSummary.Keywords}"
                : "\nMódulos: no incluidos";

            MessageBox.Show($"Exportación JSON exitosa:\n{saveDialog.FileName}\n\nCódigos: {exportSummary.Codes}{moduleInfo}",
                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al exportar: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task<(int Codes, int Modules, int ExactRules, int Keywords)> ExportToJsonAsync(string filePath, bool includeModules)
    {
        var codes = (await _repository.GetAllAsync()).ToList();

        var package = new DtcExportPackage
        {
            ExportedAtUtc = DateTime.UtcNow,
            Codes = codes
        };

        var modules = 0;
        var exactRules = 0;
        var keywords = 0;

        if (includeModules)
        {
            var filters = await _moduleRepository.GetAllFiltersAsync();
            var rules = await _moduleRepository.GetAllExactRulesAsync();
            var kws = await _moduleRepository.GetAllKeywordsAsync();

            package.Modules = filters;
            package.ExactRules = rules;
            package.Keywords = kws;

            modules = filters.Count;
            exactRules = rules.Count;
            keywords = kws.Count;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(package, options);
        File.WriteAllText(filePath, json, Encoding.UTF8);

        return (codes.Count, modules, exactRules, keywords);
    }
}
