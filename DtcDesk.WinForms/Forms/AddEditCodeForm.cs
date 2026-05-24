using DtcDesk.Core.Models;
using DtcDesk.Data.Db;
using DtcDesk.Data.Repositories;

namespace DtcDesk.WinForms;

public partial class AddEditCodeForm : Form
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly DtcRepository _repository;
    private readonly ModuleFilterRepository _moduleRepository;
    private DtcCode? _existingCode;
    private readonly bool _isEditMode;
    private readonly string _currentObdType;
    private readonly List<string> _preselectedModules = new();
    private readonly List<string> _preselectedModuleDisplays = new();

    public DtcCode? DtcCode { get; private set; }

    // Constructor para añadir nuevo código
    public AddEditCodeForm(string obdType, string? prefilledCode = null)
    {
        InitializeComponent();
        _isEditMode = false;
        _currentObdType = string.IsNullOrWhiteSpace(obdType) ? "OBD-II" : obdType;

        var dbPath = ConnectionFactory.GetDefaultDatabasePath();
        _connectionFactory = new ConnectionFactory(dbPath);
        _repository = new DtcRepository(_connectionFactory);
        _moduleRepository = new ModuleFilterRepository(_connectionFactory.GetConnectionString());

        SetupUI();
        
        if (!string.IsNullOrWhiteSpace(prefilledCode))
        {
            txtCode.Text = prefilledCode.ToUpperInvariant();
        }

        this.Text = "Añadir Código DTC";
        btnSave.Text = "Añadir";
    }

    // Constructor para editar código existente
    public AddEditCodeForm(DtcCode existingCode)
    {
        InitializeComponent();
        _isEditMode = true;
        _existingCode = existingCode;
        _currentObdType = string.IsNullOrWhiteSpace(existingCode.ObdType) ? "OBD-II" : existingCode.ObdType;

        var dbPath = ConnectionFactory.GetDefaultDatabasePath();
        _connectionFactory = new ConnectionFactory(dbPath);
        _repository = new DtcRepository(_connectionFactory);
        _moduleRepository = new ModuleFilterRepository(_connectionFactory.GetConnectionString());

        SetupUI();
        LoadExistingCode();

        this.Text = $"Editar Código DTC - {existingCode.Code}";
        btnSave.Text = "Guardar Cambios";
        txtCode.ReadOnly = true; // No permitir cambiar el código en modo edición
    }

    private void SetupUI()
    {
        ApplyDarkTheme();

        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        
        // Auto-mayúsculas en código
        txtCode.CharacterCasing = CharacterCasing.Upper;
        txtCode.MaxLength = 5;
        
        // Categorías DTC
        cmbCategory.Items.Clear();
        if (_currentObdType == "OBD-I")
        {
            cmbCategory.Items.Add("No Aplica (OBD-I)");
            cmbCategory.SelectedIndex = 0;
            cmbCategory.Enabled = false;
        }
        else
        {
            cmbCategory.Items.AddRange(new object[]
            {
                "Powertrain",
                "Network",
                "Chassis",
                "Body"
            });
            cmbCategory.SelectedIndex = 0;
            cmbCategory.Enabled = true;
        }

        // Cargar módulos desde BD de forma asíncrona
        _ = LoadModulesAsync();
    }

    /// <summary>
    /// Carga la lista de módulos desde la BD y la aplica al ComboBox.
    /// </summary>
    private async Task LoadModulesAsync()
    {
        try
        {
            var filters = await _moduleRepository.GetAllFiltersAsync();

            // Volver al hilo de UI para actualizar el combo
            if (InvokeRequired)
                Invoke(() => PopulateModuleChecklist(filters));
            else
                PopulateModuleChecklist(filters);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AddEditForm] Error al cargar módulos: {ex.Message}");
        }
    }

    private void PopulateModuleChecklist(List<DtcModuleFilter> filters)
    {
        clbModules.Items.Clear();
        clbModules.DisplayMember = nameof(DtcModuleFilter.DisplayName);
        clbModules.ValueMember = nameof(DtcModuleFilter.Name);

        foreach (var f in filters)
        {
            clbModules.Items.Add(f, false);
        }

        ApplyModuleSelectionsFromList();
    }

    private void ApplyModuleSelectionsFromList()
    {
        if (clbModules.Items.Count == 0)
        {
            return;
        }

        var selectedNames = new HashSet<string>(_preselectedModules, StringComparer.OrdinalIgnoreCase);
        var selectedDisplays = new HashSet<string>(_preselectedModuleDisplays, StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < clbModules.Items.Count; i++)
        {
            if (clbModules.Items[i] is not DtcModuleFilter filter)
            {
                continue;
            }

            if (selectedNames.Contains(filter.Name) || selectedDisplays.Contains(filter.DisplayName))
            {
                clbModules.SetItemChecked(i, true);
            }
        }
    }

    private void ApplyDarkTheme()
    {
        var bgMain = ColorTranslator.FromHtml("#0F1E2B");
        var panelSide = ColorTranslator.FromHtml("#153C59");
        var textMain = ColorTranslator.FromHtml("#EAEAEA");
        var textSecondary = ColorTranslator.FromHtml("#B0B7BE");
        var accentYellow = ColorTranslator.FromHtml("#F8B41C");
        var separator = ColorTranslator.FromHtml("#2A3B4C");

        this.BackColor = bgMain;

        // Labels
        foreach (Control control in this.Controls)
        {
            if (control is Label label)
            {
                label.ForeColor = textMain;
            }
            else if (control is TextBox textBox)
            {
                textBox.BackColor = panelSide;
                textBox.ForeColor = textMain;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.BackColor = panelSide;
                comboBox.ForeColor = textMain;
                comboBox.FlatStyle = FlatStyle.Flat;
            }
            else if (control is CheckedListBox checkedList)
            {
                checkedList.BackColor = panelSide;
                checkedList.ForeColor = textMain;
                checkedList.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        // Botones
        btnSave.BackColor = accentYellow;
        btnSave.ForeColor = Color.Black;
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

        btnCancel.BackColor = separator;
        btnCancel.ForeColor = textMain;
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderSize = 0;
    }

    private async void LoadExistingCode()
    {
        if (_existingCode == null) return;

        txtCode.Text = _existingCode.Code;
        txtDescription.Text = _existingCode.Description;
        
        if (_currentObdType == "OBD-I")
            cmbCategory.Text = "No Aplica (OBD-I)";
        else
            cmbCategory.Text = _existingCode.Category ?? "Powertrain";

        txtSource.Text = _existingCode.Source ?? "";
        txtNotes.Text = _existingCode.Notes ?? "";

        // Buscar si este código tiene una regla exacta de módulo
        try
        {
            var exactRules = await _moduleRepository.GetAllExactRulesAsync();
            var rules = exactRules
                .Where(r => string.Equals(r.Code, _existingCode.Code, StringComparison.OrdinalIgnoreCase)
                    && (string.Equals(r.ObdType, _currentObdType, StringComparison.OrdinalIgnoreCase)
                        || string.IsNullOrWhiteSpace(r.ObdType)))
                .Select(r => r.FilterName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (rules.Count > 0)
            {
                _preselectedModules.AddRange(rules);
            }

            if (!string.IsNullOrWhiteSpace(_existingCode.Module))
            {
                _preselectedModuleDisplays.AddRange(SplitModuleList(_existingCode.Module));
            }

            ApplyModuleSelectionsFromList();
        }
        catch { /* silent — no es crítico */ }
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(txtCode.Text))
        {
            MessageBox.Show("El código DTC es obligatorio.", 
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCode.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(txtDescription.Text))
        {
            MessageBox.Show("La descripción es obligatoria.", 
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtDescription.Focus();
            return;
        }

        var code = txtCode.Text.Trim().ToUpperInvariant();

        // Validar formato
        if (!IsValidCodeFormat(code))
        {
            MessageBox.Show("El formato del código no es válido.\n\nFormatos aceptados:\n- P21DA (letra + 4 caracteres hexadecimales)\n- FFFF (4 caracteres hexadecimales)", 
                "Formato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCode.Focus();
            return;
        }

        try
        {
            Cursor = Cursors.WaitCursor;
            btnSave.Enabled = false;

            if (_isEditMode && _existingCode != null)
            {
                var selectedModules = GetSelectedModules();
                var filterTagList = BuildModuleList(selectedModules, m => m.Name);
                var moduleList = BuildModuleList(selectedModules, m => m.DisplayName);

                // Actualizar código existente
                _existingCode.Description = txtDescription.Text.Trim();
                _existingCode.Category = _currentObdType == "OBD-I" ? "Hex" : cmbCategory.Text;
                _existingCode.ObdType = _currentObdType;
                _existingCode.FilterTag = filterTagList;
                _existingCode.Module = moduleList;
                _existingCode.Source = txtSource.Text.Trim();
                _existingCode.Notes = txtNotes.Text.Trim();

                var updated = await _repository.UpdateAsync(_existingCode);
                
                if (updated)
                {
                    DtcCode = _existingCode;
                    await SaveModuleRulesAsync(_existingCode.Code, selectedModules);
                    MessageBox.Show("Código actualizado correctamente.", 
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el código.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Verificar si ya existe
                var exists = await _repository.ExistsAsync(code, _currentObdType);
                if (exists)
                {
                    var result = MessageBox.Show(
                        $"El código {code} ya existe en la base de datos.\n\n¿Deseas actualizarlo?",
                        "Código existente",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        return;
                    }

                    // Cargar y actualizar
                    var existingCode = await _repository.GetByCodeAsync(code, _currentObdType);
                    if (existingCode != null)
                    {
                        var selectedModules = GetSelectedModules();
                        var filterTagList = BuildModuleList(selectedModules, m => m.Name);
                        var moduleList = BuildModuleList(selectedModules, m => m.DisplayName);

                        existingCode.Description = txtDescription.Text.Trim();
                        existingCode.Category = _currentObdType == "OBD-I" ? "Hex" : cmbCategory.Text;
                        existingCode.ObdType = _currentObdType;
                        existingCode.FilterTag = filterTagList;
                        existingCode.Module = moduleList;
                        existingCode.Source = txtSource.Text.Trim();
                        existingCode.Notes = txtNotes.Text.Trim();

                        await _repository.UpdateAsync(existingCode);
                        await SaveModuleRulesAsync(code, selectedModules);
                        DtcCode = existingCode;
                    }
                }
                else
                {
                    var selectedModules = GetSelectedModules();
                    var filterTagList = BuildModuleList(selectedModules, m => m.Name);
                    var moduleList = BuildModuleList(selectedModules, m => m.DisplayName);

                    // Insertar nuevo código
                    var newCode = new DtcCode
                    {
                        Code = code,
                        Description = txtDescription.Text.Trim(),
                        Category = _currentObdType == "OBD-I" ? "Hex" : cmbCategory.Text,
                        ObdType = _currentObdType,
                        FilterTag = filterTagList,
                        Module = moduleList,
                        Source = txtSource.Text.Trim(),
                        Notes = txtNotes.Text.Trim(),
                        IsActive = true
                    };

                    var id = await _repository.InsertAsync(newCode);
                    newCode.Id = id;
                    DtcCode = newCode;
                    await SaveModuleRulesAsync(code, selectedModules);
                    
                    MessageBox.Show("Código añadido correctamente.", 
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar el código: {ex.Message}", 
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            btnSave.Enabled = true;
        }
    }

    /// <summary>
    /// Guarda las reglas exactas código→módulo según la selección múltiple.
    /// </summary>
    private async Task SaveModuleRulesAsync(string code, List<DtcModuleFilter> selectedModules)
    {
        try
        {
            await _moduleRepository.DeleteExactRuleByCodeAsync(code, _currentObdType);

            foreach (var selected in selectedModules)
            {
                if (string.IsNullOrWhiteSpace(selected.Name))
                {
                    continue;
                }

                await _moduleRepository.SaveExactRuleAsync(code, selected.Name, _currentObdType);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AddEditForm] Error al guardar módulo: {ex.Message}");
        }
    }

    private List<DtcModuleFilter> GetSelectedModules()
    {
        return clbModules.CheckedItems
            .OfType<DtcModuleFilter>()
            .Where(m => !string.IsNullOrWhiteSpace(m.Name))
            .ToList();
    }

    private static string? BuildModuleList(IEnumerable<DtcModuleFilter> modules, Func<DtcModuleFilter, string> selector)
    {
        var list = modules
            .Select(selector)
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return list.Count == 0 ? null : string.Join(", ", list);
    }

    private static List<string> SplitModuleList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new List<string>();
        }

        return value
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }

    private bool IsValidCodeFormat(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;

        if (_currentObdType == "OBD-I")
        {
            return System.Text.RegularExpressions.Regex.IsMatch(code, @"^[a-zA-Z0-9]{1,5}$");
        }

        // P-codes, C-codes, B-codes, U-codes (letra + 4 caracteres hexadecimales)
        if (System.Text.RegularExpressions.Regex.IsMatch(code, @"^[PCBU][0-9A-F]{4}$"))
            return true;

        // Códigos hexadecimales de 4 caracteres
        if (System.Text.RegularExpressions.Regex.IsMatch(code, @"^[0-9A-F]{4}$"))
            return true;

        return false;
    }
}
