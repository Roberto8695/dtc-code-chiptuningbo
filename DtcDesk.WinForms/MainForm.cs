using DtcDesk.Core.Models;
using DtcDesk.Core.Parsing;
using DtcDesk.Data.Db;
using DtcDesk.Data.Repositories;

namespace DtcDesk.WinForms;

public partial class MainForm : Form
{
    private readonly DtcParser _parser;
    private readonly DtcRepository _repository;
    private readonly ConnectionFactory _connectionFactory;
    private List<DtcLookupResult> _currentResults = new();

    public MainForm()
    {
        InitializeComponent();
        
        // Inicializar servicios
        _parser = new DtcParser();
        
        var dbPath = ConnectionFactory.GetDefaultDatabasePath();
        _connectionFactory = new ConnectionFactory(dbPath);
        
        // Inicializar base de datos
        var dbInitializer = new DbInitializer(_connectionFactory.GetConnectionString());
        dbInitializer.Initialize();
        
        _repository = new DtcRepository(_connectionFactory);
        
        // Configurar UI
        SetupUI();
        LoadStatistics();
    }

    private void SetupUI()
    {
        // Aplicar tema oscuro
        ApplyDarkTheme();
        
        // Configurar DataGridView
        SetupDataGridView();
        
        // Cargar logo
        LoadLogo();
        
        // Configurar eventos
        btnParse.Click += BtnParse_Click;
        btnClear.Click += BtnClear_Click;
        btnAdd.Click += BtnAdd_Click;
        btnEdit.Click += BtnEdit_Click;
        btnDelete.Click += BtnDelete_Click;
        btnExport.Click += BtnExport_Click;
        btnImport.Click += BtnImport_Click;
        
        dgvCodes.CellDoubleClick += DgvCodes_CellDoubleClick;
        dgvCodes.SelectionChanged += DgvCodes_SelectionChanged;
        
        txtInput.Font = new Font("Consolas", 10F);
    }

    private void LoadLogo()
    {
        try
        {
            // Intentar cargar el logo desde el directorio de la aplicación
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.jpg");
            
            if (File.Exists(logoPath))
            {
                picLogo.Image = Image.FromFile(logoPath);
                picLogo.BackColor = Color.Transparent;
            }
            else
            {
                // Si no existe el logo, usar el título sin logo
                picLogo.Visible = false;
            }
        }
        catch
        {
            // Si hay error al cargar, ocultar el PictureBox
            picLogo.Visible = false;
        }
    }

    private void ApplyDarkTheme()
    {
        // Paleta de colores del design.md
        var bgMain = ColorTranslator.FromHtml("#0F1E2B");
        var bgSide = ColorTranslator.FromHtml("#153C59");
        var bgTop = ColorTranslator.FromHtml("#102C44");
        var textMain = ColorTranslator.FromHtml("#EAEAEA");
        var textSecondary = ColorTranslator.FromHtml("#B0B7BE");
        var separator = ColorTranslator.FromHtml("#2A3B4C");
        var accentYellow = ColorTranslator.FromHtml("#F8B41C");
        var accentHover = ColorTranslator.FromHtml("#D89C17");
        
        // Fondo principal
        this.BackColor = bgMain;
        
        // Panel superior
        panelTop.BackColor = bgTop;
        lblTitle.ForeColor = textMain;
        lblStats.ForeColor = textSecondary;
        
        // Panel izquierdo (entrada)
        panelLeft.BackColor = bgSide;
        lblInput.ForeColor = textMain;
        txtInput.BackColor = bgMain;
        txtInput.ForeColor = textMain;
        txtInput.BorderStyle = BorderStyle.FixedSingle;
        
        // Panel derecho (resultados)
        panelRight.BackColor = bgMain;
        lblResults.ForeColor = textMain;
        
        // DataGridView
        dgvCodes.BackgroundColor = bgMain;
        dgvCodes.GridColor = separator;
        dgvCodes.BorderStyle = BorderStyle.None;
        dgvCodes.DefaultCellStyle.BackColor = bgTop;
        dgvCodes.DefaultCellStyle.ForeColor = textMain;
        dgvCodes.DefaultCellStyle.SelectionBackColor = accentYellow;
        dgvCodes.DefaultCellStyle.SelectionForeColor = Color.Black;
        dgvCodes.ColumnHeadersDefaultCellStyle.BackColor = bgSide;
        dgvCodes.ColumnHeadersDefaultCellStyle.ForeColor = textMain;
        dgvCodes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgvCodes.EnableHeadersVisualStyles = false;
        dgvCodes.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#0A1520");
        
        // Botones con acento amarillo
        StyleButton(btnParse, accentYellow, Color.Black);
        StyleButton(btnClear, separator, textMain);
        StyleButton(btnAdd, accentYellow, Color.Black);
        StyleButton(btnEdit, accentHover, Color.Black);
        StyleButton(btnDelete, ColorTranslator.FromHtml("#D9534F"), Color.White);
        StyleButton(btnExport, ColorTranslator.FromHtml("#5CB85C"), Color.White);
        StyleButton(btnImport, bgSide, textMain);
    }

    private void StyleButton(Button btn, Color backColor, Color foreColor)
    {
        btn.BackColor = backColor;
        btn.ForeColor = foreColor;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btn.Cursor = Cursors.Hand;
    }

    private void SetupDataGridView()
    {
        dgvCodes.AutoGenerateColumns = false;
        dgvCodes.AllowUserToAddRows = false;
        dgvCodes.AllowUserToDeleteRows = false;
        dgvCodes.ReadOnly = true;
        dgvCodes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvCodes.MultiSelect = false;
        dgvCodes.RowHeadersVisible = false;
        
        // Columnas
        dgvCodes.Columns.Clear();
        
        dgvCodes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCode",
            HeaderText = "CÓDIGO",
            DataPropertyName = "Code",
            Width = 100,
            DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Consolas", 10F, FontStyle.Bold) }
        });
        
        dgvCodes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colDescription",
            HeaderText = "DESCRIPCIÓN",
            DataPropertyName = "Description",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        
        dgvCodes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCategory",
            HeaderText = "CATEGORÍA",
            DataPropertyName = "Category",
            Width = 100
        });
        
        dgvCodes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colStatus",
            HeaderText = "ESTADO",
            DataPropertyName = "Found",
            Width = 100
        });
        
        // Formato condicional para el estado
        dgvCodes.CellFormatting += DgvCodes_CellFormatting;
    }

    private void DgvCodes_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (dgvCodes.Columns[e.ColumnIndex].Name == "colStatus" && e.Value != null)
        {
            var found = (bool)e.Value;
            e.Value = found ? "✓ Encontrado" : "⚠ No encontrado";
            
            if (!found)
            {
                e.CellStyle.ForeColor = ColorTranslator.FromHtml("#D9534F");
                e.CellStyle.Font = new Font(e.CellStyle.Font!, FontStyle.Bold);
            }
        }
        
        // Resaltar descripción vacía
        if (dgvCodes.Columns[e.ColumnIndex].Name == "colDescription" && 
            (e.Value == null || string.IsNullOrWhiteSpace(e.Value.ToString())))
        {
            e.Value = "--- Sin descripción ---";
            e.CellStyle.ForeColor = ColorTranslator.FromHtml("#B0B7BE");
            e.CellStyle.Font = new Font(e.CellStyle.Font!, FontStyle.Italic);
        }
    }

    private async void BtnParse_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtInput.Text))
        {
            MessageBox.Show("Por favor, pega códigos DTC en el área de texto.", 
                "Entrada vacía", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Cursor = Cursors.WaitCursor;
            btnParse.Enabled = false;

            // Parsear códigos (incluyendo duplicados)
            var codes = _parser.Parse(txtInput.Text, removeDuplicates: false);
            
            if (codes.Count == 0)
            {
                MessageBox.Show("No se encontraron códigos DTC válidos en el texto pegado.", 
                    "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Buscar en base de datos
            var dbCodes = await _repository.GetByCodesAsync(codes);
            var dbCodesDict = dbCodes.ToDictionary(c => c.Code, c => c);

            // Crear resultados
            _currentResults = codes.Select(code => new DtcLookupResult
            {
                Code = code,
                Found = dbCodesDict.ContainsKey(code),
                Description = dbCodesDict.ContainsKey(code) ? dbCodesDict[code].Description : null,
                Category = dbCodesDict.ContainsKey(code) ? dbCodesDict[code].Category : _parser.GetCodeCategory(code),
                Source = dbCodesDict.ContainsKey(code) ? dbCodesDict[code].Source : null,
                Notes = dbCodesDict.ContainsKey(code) ? dbCodesDict[code].Notes : null,
                DtcId = dbCodesDict.ContainsKey(code) ? dbCodesDict[code].Id : null
            }).ToList();

            // Mostrar en grid
            dgvCodes.DataSource = null;
            dgvCodes.DataSource = _currentResults;

            // Actualizar estadísticas (contar duplicados también)
            var found = _currentResults.Count(r => r.Found);
            var notFound = _currentResults.Count - found;
            var uniqueCodes = _currentResults.Select(r => r.Code).Distinct().Count();
            lblStats.Text = $"Total: {_currentResults.Count} ({uniqueCodes} únicos) | Encontrados: {found} | No encontrados: {notFound}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al procesar códigos: {ex.Message}", 
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            btnParse.Enabled = true;
        }
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        txtInput.Clear();
        dgvCodes.DataSource = null;
        _currentResults.Clear();
        lblStats.Text = "Total: 0 | Encontrados: 0 | No encontrados: 0";
        txtInput.Focus();
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        // Si hay un código seleccionado sin descripción, pre-llenarlo
        string? prefilledCode = null;
        
        if (dgvCodes.SelectedRows.Count > 0)
        {
            var selectedResult = dgvCodes.SelectedRows[0].DataBoundItem as DtcLookupResult;
            if (selectedResult != null && !selectedResult.Found)
            {
                prefilledCode = selectedResult.Code;
            }
        }

        var addForm = new AddEditCodeForm(prefilledCode);
        if (addForm.ShowDialog() == DialogResult.OK)
        {
            // Refrescar si el código estaba en la lista actual
            if (_currentResults.Any(r => r.Code == addForm.DtcCode!.Code))
            {
                BtnParse_Click(sender, e); // Re-parsear para actualizar
            }
            
            LoadStatistics();
        }
    }

    private async void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dgvCodes.SelectedRows.Count == 0)
        {
            MessageBox.Show("Por favor, selecciona un código para editar.", 
                "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedResult = dgvCodes.SelectedRows[0].DataBoundItem as DtcLookupResult;
        if (selectedResult == null || !selectedResult.Found || !selectedResult.DtcId.HasValue)
        {
            MessageBox.Show("Este código no existe en la base de datos. Usa 'Añadir' para agregarlo.", 
                "Código no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // Cargar código completo desde BD
            var dtcCode = await _repository.GetByCodeAsync(selectedResult.Code);
            if (dtcCode == null) return;

            var editForm = new AddEditCodeForm(dtcCode);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                BtnParse_Click(sender, e); // Re-parsear para actualizar
                LoadStatistics();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar el código: {ex.Message}", 
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvCodes.SelectedRows.Count == 0)
        {
            MessageBox.Show("Por favor, selecciona un código para eliminar.", 
                "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedResult = dgvCodes.SelectedRows[0].DataBoundItem as DtcLookupResult;
        if (selectedResult == null || !selectedResult.Found || !selectedResult.DtcId.HasValue)
        {
            MessageBox.Show("Este código no existe en la base de datos.", 
                "Código no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"¿Estás seguro de eliminar el código {selectedResult.Code}?\n\nDescripción: {selectedResult.Description}",
            "Confirmar eliminación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            try
            {
                await _repository.DeleteAsync(selectedResult.DtcId.Value);
                MessageBox.Show("Código eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                BtnParse_Click(sender, e); // Re-parsear para actualizar
                LoadStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el código: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void BtnExport_Click(object? sender, EventArgs e)
    {
        if (_currentResults.Count == 0)
        {
            MessageBox.Show("No hay códigos para exportar.", 
                "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var exportForm = new ExportForm(_currentResults);
        exportForm.ShowDialog();
    }

    private void BtnImport_Click(object? sender, EventArgs e)
    {
        var importForm = new ImportForm();
        if (importForm.ShowDialog() == DialogResult.OK)
        {
            LoadStatistics();
        }
    }

    private async void LoadStatistics()
    {
        try
        {
            var count = await _repository.GetCountAsync();
            this.Text = $"DtcDesk - Diccionario de Códigos DTC ({count:N0} códigos en BD)";
        }
        catch
        {
            this.Text = "DtcDesk - Diccionario de Códigos DTC";
        }
    }

    private void DgvCodes_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        
        // Doble clic abre editar/añadir
        var selectedResult = dgvCodes.Rows[e.RowIndex].DataBoundItem as DtcLookupResult;
        if (selectedResult == null) return;

        if (selectedResult.Found)
        {
            BtnEdit_Click(sender, e);
        }
        else
        {
            BtnAdd_Click(sender, e);
        }
    }

    private void DgvCodes_SelectionChanged(object? sender, EventArgs e)
    {
        // Habilitar/deshabilitar botones según selección
        var hasSelection = dgvCodes.SelectedRows.Count > 0;
        var isFound = false;

        if (hasSelection)
        {
            var selectedResult = dgvCodes.SelectedRows[0].DataBoundItem as DtcLookupResult;
            isFound = selectedResult?.Found ?? false;
        }

        btnEdit.Enabled = hasSelection && isFound;
        btnDelete.Enabled = hasSelection && isFound;
    }
}
