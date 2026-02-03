using DtcDesk.Core.Models;
using System.Text;

namespace DtcDesk.WinForms;

public partial class ExportForm : Form
{
    private readonly List<DtcLookupResult> _results;

    public ExportForm(List<DtcLookupResult> results)
    {
        InitializeComponent();
        _results = results;

        SetupUI();
    }

    private void SetupUI()
    {
        ApplyDarkTheme();

        btnExportTxt.Click += (s, e) => ExportAs("txt");
        btnExportCsv.Click += (s, e) => ExportAs("csv");
        btnCancel.Click += (s, e) => this.Close();

        lblInfo.Text = $"Exportar {_results.Count} código(s) DTC";
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

        chkIncludeDescription.ForeColor = textMain;
        chkIncludeCategory.ForeColor = textMain;
        chkOnlyNotFound.ForeColor = textMain;

        btnExportTxt.BackColor = accentYellow;
        btnExportTxt.ForeColor = Color.Black;
        btnExportTxt.FlatStyle = FlatStyle.Flat;
        btnExportTxt.FlatAppearance.BorderSize = 0;

        btnExportCsv.BackColor = accentGreen;
        btnExportCsv.ForeColor = Color.White;
        btnExportCsv.FlatStyle = FlatStyle.Flat;
        btnExportCsv.FlatAppearance.BorderSize = 0;

        btnCancel.BackColor = separator;
        btnCancel.ForeColor = textMain;
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderSize = 0;
    }

    private void ExportAs(string format)
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = format == "txt" ? "Archivo de texto|*.txt" : "Archivo CSV|*.csv",
            FileName = $"dtc_codes_{DateTime.Now:yyyyMMdd_HHmmss}.{format}"
        };

        if (saveDialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            var results = chkOnlyNotFound.Checked 
                ? _results.Where(r => !r.Found).ToList() 
                : _results;

            if (results.Count == 0)
            {
                MessageBox.Show("No hay códigos para exportar con los filtros seleccionados.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (format == "txt")
            {
                ExportToTxt(saveDialog.FileName, results);
            }
            else
            {
                ExportToCsv(saveDialog.FileName, results);
            }

            MessageBox.Show($"Exportación exitosa:\n{saveDialog.FileName}\n\n{results.Count} código(s) exportado(s).",
                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al exportar: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportToTxt(string filePath, List<DtcLookupResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════");
        sb.AppendLine("  CÓDIGOS DTC - EXPORTACIÓN");
        sb.AppendLine($"  Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine($"  Total: {results.Count} código(s)");
        sb.AppendLine("═══════════════════════════════════════════════════════");
        sb.AppendLine();

        foreach (var result in results)
        {
            sb.AppendLine($"Código: {result.Code}");
            
            if (chkIncludeDescription.Checked)
            {
                sb.AppendLine($"  Descripción: {result.Description ?? "Sin descripción"}");
            }
            
            if (chkIncludeCategory.Checked)
            {
                sb.AppendLine($"  Categoría: {result.Category ?? "N/A"}");
                if (!string.IsNullOrEmpty(result.Source))
                {
                    sb.AppendLine($"  Fuente: {result.Source}");
                }
            }
            
            sb.AppendLine($"  Estado: {(result.Found ? "Encontrado" : "No encontrado")}");
            sb.AppendLine();
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    private void ExportToCsv(string filePath, List<DtcLookupResult> results)
    {
        var sb = new StringBuilder();
        
        // Encabezados
        var headers = new List<string> { "Código" };
        if (chkIncludeDescription.Checked) headers.Add("Descripción");
        if (chkIncludeCategory.Checked)
        {
            headers.Add("Categoría");
            headers.Add("Fuente");
        }
        headers.Add("Estado");
        
        sb.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

        // Datos
        foreach (var result in results)
        {
            var values = new List<string> { EscapeCsv(result.Code) };
            
            if (chkIncludeDescription.Checked)
            {
                values.Add(EscapeCsv(result.Description ?? ""));
            }
            
            if (chkIncludeCategory.Checked)
            {
                values.Add(EscapeCsv(result.Category ?? ""));
                values.Add(EscapeCsv(result.Source ?? ""));
            }
            
            values.Add(EscapeCsv(result.Found ? "Encontrado" : "No encontrado"));
            
            sb.AppendLine(string.Join(",", values));
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return $"\"{value}\"";
    }
}
