using System.Text.RegularExpressions;
using DtcDesk.Core.Models;

namespace DtcDesk.WinForms.Forms;

public class CustomModuleEditorForm : Form
{
    private readonly TextBox _txtDisplayName;
    private readonly TextBox _txtDescription;
    private readonly TextBox _txtCodes;
    private readonly Label _lblCount;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;
    private readonly Dictionary<string, string?> _existingCodeTypes = new(StringComparer.OrdinalIgnoreCase);

    public string ModuleDisplayName => _txtDisplayName.Text.Trim();
    public string? ModuleDescription => string.IsNullOrWhiteSpace(_txtDescription.Text) ? null : _txtDescription.Text.Trim();
    public List<string> ExactCodes { get; private set; } = new();
    public List<DtcModuleRule> ExactRules { get; private set; } = new();

    public CustomModuleEditorForm(DtcModuleFilter? filter = null, IEnumerable<string>? existingCodes = null)
    {
        Text = filter == null ? "Nuevo módulo personalizado" : "Editar módulo personalizado";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(640, 520);

        var lblName = new Label
        {
            Text = "Nombre del botón/módulo:",
            AutoSize = true,
            Location = new Point(18, 16)
        };

        _txtDisplayName = new TextBox
        {
            Location = new Point(18, 36),
            Width = 594,
            MaxLength = 40
        };

        var lblDescription = new Label
        {
            Text = "Descripción (opcional):",
            AutoSize = true,
            Location = new Point(18, 70)
        };

        _txtDescription = new TextBox
        {
            Location = new Point(18, 90),
            Width = 594,
            MaxLength = 240
        };

        var lblCodes = new Label
        {
            Text = "Códigos a eliminar (uno por línea o separados por coma/espacio):",
            AutoSize = true,
            Location = new Point(18, 124)
        };

        _txtCodes = new TextBox
        {
            Location = new Point(18, 146),
            Width = 594,
            Height = 300,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10F)
        };
        _txtCodes.TextChanged += (_, _) => UpdateCountLabel();

        _lblCount = new Label
        {
            AutoSize = true,
            Location = new Point(18, 454),
            Text = "Códigos válidos: 0"
        };

        _btnSave = new Button
        {
            Text = "Guardar",
            Width = 110,
            Height = 34,
            Location = new Point(386, 470),
            DialogResult = DialogResult.None
        };
        _btnSave.Click += (_, _) => SaveAndClose();

        _btnCancel = new Button
        {
            Text = "Cancelar",
            Width = 110,
            Height = 34,
            Location = new Point(502, 470),
            DialogResult = DialogResult.Cancel
        };

        Controls.Add(lblName);
        Controls.Add(_txtDisplayName);
        Controls.Add(lblDescription);
        Controls.Add(_txtDescription);
        Controls.Add(lblCodes);
        Controls.Add(_txtCodes);
        Controls.Add(_lblCount);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);

        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        if (filter != null)
        {
            _txtDisplayName.Text = filter.DisplayName;
            _txtDescription.Text = filter.Description ?? string.Empty;
        }

        if (existingCodes != null)
        {
            _txtCodes.Text = string.Join(Environment.NewLine, existingCodes);
        }

        UpdateCountLabel();
    }

    public CustomModuleEditorForm(DtcModuleFilter filter, IEnumerable<DtcModuleRule> existingRules)
        : this(filter, existingRules.Select(r => r.Code))
    {
        foreach (var rule in existingRules)
        {
            _existingCodeTypes[rule.Code] = rule.ObdType;
        }
    }

    private void SaveAndClose()
    {
        if (string.IsNullOrWhiteSpace(_txtDisplayName.Text))
        {
            MessageBox.Show("Debes ingresar un nombre para el módulo.", "Nombre requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtDisplayName.Focus();
            return;
        }

        var parsedCodes = ParseCodes(_txtCodes.Text);
        if (parsedCodes.Count == 0)
        {
            MessageBox.Show("Debes ingresar al menos un código válido para el módulo.", "Códigos requeridos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtCodes.Focus();
            return;
        }

        ExactCodes = parsedCodes;
        ExactRules = parsedCodes
            .Select(code => new DtcModuleRule
            {
                Code = code,
                ObdType = _existingCodeTypes.TryGetValue(code, out var obdType) ? obdType : "OBD-II"
            })
            .ToList();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void UpdateCountLabel()
    {
        var parsedCodes = ParseCodes(_txtCodes.Text);
        _lblCount.Text = $"Códigos válidos: {parsedCodes.Count}";
    }

    private static List<string> ParseCodes(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<string>();
        }

        var tokens = Regex.Split(input, @"[\s,;]+")
            .Select(t => t.Trim().ToUpperInvariant())
            .Where(t => !string.IsNullOrWhiteSpace(t));

        return tokens
            .Where(IsValidDtcToken)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsValidDtcToken(string token)
    {
        return Regex.IsMatch(token, @"^(?:[PU][0-9A-F]{4}|[CD][0-9A-F]{3}|[0-9A-F]{4}|\d{2,4})$");
    }
}
