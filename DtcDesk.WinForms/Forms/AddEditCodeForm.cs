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
    private string? _preselectedModule;  // Usado en modo edición para mostrar el módulo actual

    public DtcCode? DtcCode { get; private set; }

    // Constructor para añadir nuevo código
    public AddEditCodeForm(string? prefilledCode = null)
    {
        InitializeComponent();
        _isEditMode = false;

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
        cmbCategory.Items.AddRange(new object[]
        {
            "Powertrain",
            "Network"
        });
        cmbCategory.SelectedIndex = 0;

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
                Invoke(() => PopulateModuleCombo(filters));
            else
                PopulateModuleCombo(filters);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AddEditForm] Error al cargar módulos: {ex.Message}");
        }
    }

    private void PopulateModuleCombo(List<DtcModuleFilter> filters)
    {
        cmbModule.Items.Clear();
        cmbModule.Items.Add("(Ninguno)");  // Primera opción en blanco

        foreach (var f in filters)
            cmbModule.Items.Add(f.Name);

        // Preseleccionar el módulo si ya se conocía
        if (!string.IsNullOrWhiteSpace(_preselectedModule))
        {
            var idx = cmbModule.Items.IndexOf(_preselectedModule);
            cmbModule.SelectedIndex = idx >= 0 ? idx : 0;
        }
        else
        {
            cmbModule.SelectedIndex = 0;  // "(Ninguno)"
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
        cmbCategory.Text = _existingCode.Category ?? "Powertrain";
        txtSource.Text = _existingCode.Source ?? "";
        txtNotes.Text = _existingCode.Notes ?? "";

        // Buscar si este código tiene una regla exacta de módulo
        try
        {
            var exactRules = await _moduleRepository.GetAllExactRulesAsync();
            var rule = exactRules.FirstOrDefault(r =>
                string.Equals(r.Code, _existingCode.Code, StringComparison.OrdinalIgnoreCase));

            if (rule != null)
                _preselectedModule = rule.FilterName;
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
                // Actualizar código existente
                _existingCode.Description = txtDescription.Text.Trim();
                _existingCode.Category = cmbCategory.Text;
                _existingCode.Source = txtSource.Text.Trim();
                _existingCode.Notes = txtNotes.Text.Trim();

                var updated = await _repository.UpdateAsync(_existingCode);
                
                if (updated)
                {
                    DtcCode = _existingCode;
                    await SaveModuleRuleAsync(_existingCode.Code);
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
                var exists = await _repository.ExistsAsync(code);
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
                    var existingCode = await _repository.GetByCodeAsync(code);
                    if (existingCode != null)
                    {
                        existingCode.Description = txtDescription.Text.Trim();
                        existingCode.Category = cmbCategory.Text;
                        existingCode.Source = txtSource.Text.Trim();
                        existingCode.Notes = txtNotes.Text.Trim();

                        await _repository.UpdateAsync(existingCode);
                        await SaveModuleRuleAsync(code);
                        DtcCode = existingCode;
                    }
                }
                else
                {
                    // Insertar nuevo código
                    var newCode = new DtcCode
                    {
                        Code = code,
                        Description = txtDescription.Text.Trim(),
                        Category = cmbCategory.Text,
                        Source = txtSource.Text.Trim(),
                        Notes = txtNotes.Text.Trim(),
                        IsActive = true
                    };

                    var id = await _repository.InsertAsync(newCode);
                    newCode.Id = id;
                    DtcCode = newCode;
                    await SaveModuleRuleAsync(code);
                    
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
    /// Guarda o elimina la regla exacta código→módulo según la selección del ComboBox.
    /// </summary>
    private async Task SaveModuleRuleAsync(string code)
    {
        try
        {
            var selected = cmbModule.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected) || selected == "(Ninguno)")
            {
                // Eliminar regla si existía
                await _moduleRepository.DeleteExactRuleByCodeAsync(code);
            }
            else
            {
                await _moduleRepository.SaveExactRuleAsync(code, selected);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AddEditForm] Error al guardar módulo: {ex.Message}");
        }
    }

    private bool IsValidCodeFormat(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;

        // P-codes, C-codes, B-codes, U-codes (letra + 4 caracteres hexadecimales)
        if (System.Text.RegularExpressions.Regex.IsMatch(code, @"^[PCBU][0-9A-F]{4}$"))
            return true;

        // Códigos hexadecimales de 4 caracteres
        if (System.Text.RegularExpressions.Regex.IsMatch(code, @"^[0-9A-F]{4}$"))
            return true;

        return false;
    }
}
