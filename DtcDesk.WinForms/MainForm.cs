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
        
        // Configurar eventos del menú
        menuImportar.Click += MenuImportar_Click;
        menuExportar.Click += MenuExportar_Click;
        menuLimpiarDB.Click += MenuLimpiarDB_Click;
        menuSalir.Click += MenuSalir_Click;
        menuEstadisticas.Click += MenuEstadisticas_Click;
        
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
        
        // Estilo del menú
        menuStrip.BackColor = bgSide;
        menuStrip.ForeColor = textMain;
        menuArchivo.ForeColor = textMain;
        menuHerramientas.ForeColor = textMain;
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
        dgvCodes.SelectionMode = DataGridViewSelectionMode.CellSelect;
        dgvCodes.MultiSelect = true;
        dgvCodes.RowHeadersVisible = false;
        dgvCodes.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
        
        // Menú contextual para copiar
        SetupContextMenu();
        
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
            Name = "colCodeAlt",
            HeaderText = "CÓDIGO ALT",
            DataPropertyName = "CodeAlt",
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
            Name = "colStatus",
            HeaderText = "ESTADO",
            DataPropertyName = "Found",
            Width = 100
        });
        
        // Formato condicional para el estado
        dgvCodes.CellFormatting += DgvCodes_CellFormatting;
    }

    private void SetupContextMenu()
    {
        var contextMenu = new ContextMenuStrip();
        
        // Configurar renderer personalizado para colores
        contextMenu.Renderer = new ToolStripProfessionalRenderer(new CustomMenuColorTable());
        
        // Opción Copiar
        var copyMenuItem = new ToolStripMenuItem("Copiar");
        copyMenuItem.ShortcutKeys = Keys.Control | Keys.C;
        copyMenuItem.Click += (sender, e) => CopySelectedCellsToClipboard();
        
        // Opción Copiar Todo
        var copyAllMenuItem = new ToolStripMenuItem("Copiar Todo (Tabla Completa)");
        copyAllMenuItem.Click += (sender, e) => CopyAllDataToClipboard();
        
        // Opción Borrar (reemplazar con 0000/FFFF)
        var deleteMenuItem = new ToolStripMenuItem("Borrar");
        deleteMenuItem.Click += (sender, e) => DeleteAndReplaceSelectedCodes();
        
        contextMenu.Items.Add(copyMenuItem);
        contextMenu.Items.Add(copyAllMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(deleteMenuItem);
        
        // Aplicar tema oscuro al menú contextual
        contextMenu.BackColor = ColorTranslator.FromHtml("#102C44");
        contextMenu.ForeColor = ColorTranslator.FromHtml("#EAEAEA");
        
        dgvCodes.ContextMenuStrip = contextMenu;
        
        // También permitir Ctrl+C directamente y Backspace para borrar/reemplazar códigos
        dgvCodes.KeyDown += DgvCodes_KeyDown;
    }

    private void DgvCodes_KeyDown(object? sender, KeyEventArgs e)
    {
        // Copiar con Ctrl+C
        if (e.Control && e.KeyCode == Keys.C)
        {
            CopySelectedCellsToClipboard();
            e.Handled = true;
        }
        
        // Borrar/Reemplazar códigos con Backspace
        if (e.KeyCode == Keys.Back)
        {
            DeleteAndReplaceSelectedCodes();
            e.Handled = true;
        }
    }

    private void DeleteAndReplaceSelectedCodes()
    {
        if (dgvCodes.SelectedCells.Count == 0 || _currentResults == null || _currentResults.Count == 0)
            return;

        // Obtener solo las celdas de la columna CÓDIGO que están seleccionadas
        var selectedCodeCells = dgvCodes.SelectedCells
            .Cast<DataGridViewCell>()
            .Where(cell => cell.OwningColumn.Name == "colCode")
            .ToList();

        if (selectedCodeCells.Count == 0)
        {
            MessageBox.Show("Por favor, selecciona códigos de la columna CÓDIGO para reemplazar.",
                "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Reemplazar directamente con "0000"
        string selectedReplacement = "0000";
        
        foreach (var cell in selectedCodeCells)
        {
            if (cell.RowIndex >= 0 && cell.RowIndex < _currentResults.Count)
            {
                var result = _currentResults[cell.RowIndex];
                result.Code = selectedReplacement;
                result.CodeAlt = "FFFF"; // En la columna alternativa mostrar FFFF
                result.Description = "Sin resultados";
                result.Found = false;
                result.Category = "Hex";
                result.Source = null;
                result.Notes = null;
            }
        }

        // Refrescar el DataGridView
        dgvCodes.DataSource = null;
        dgvCodes.DataSource = _currentResults;

        // Actualizar estadísticas
        var found = _currentResults.Count(r => r.Found);
        var notFound = _currentResults.Count - found;
        lblStats.Text = $"Total: {_currentResults.Count} | Encontrados: {found} | No encontrados: {notFound}";

        MessageBox.Show($"Se reemplazaron {selectedCodeCells.Count} código(s) con '{selectedReplacement}'.",
            "Códigos Reemplazados", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void CopySelectedCellsToClipboard()
    {
        if (dgvCodes.GetCellCount(DataGridViewElementStates.Selected) == 0)
            return;

        try
        {
            // Obtener las celdas seleccionadas y ordenarlas
            var selectedCells = dgvCodes.SelectedCells
                .Cast<DataGridViewCell>()
                .OrderBy(cell => cell.RowIndex)
                .ThenBy(cell => cell.ColumnIndex)
                .ToList();

            // Recopilar los valores
            var values = new List<string>();
            foreach (var cell in selectedCells)
            {
                var value = cell.Value?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Si es la columna de código, quitar el prefijo (P, U, etc.)
                    if (cell.OwningColumn.Name == "colCode" && value.Length > 1 && char.IsLetter(value[0]))
                    {
                        value = value.Substring(1);
                    }
                    values.Add(value);
                }
            }

            // Convertir a formato horizontal (separado por espacios)
            var horizontalText = string.Join(" ", values);

            if (!string.IsNullOrEmpty(horizontalText))
            {
                Clipboard.SetText(horizontalText);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al copiar: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CopyAllDataToClipboard()
    {
        if (_currentResults == null || _currentResults.Count == 0)
            return;

        try
        {
            // Seleccionar todas las celdas temporalmente
            dgvCodes.SelectAll();
            
            // Copiar
            DataObject dataObj = dgvCodes.GetClipboardContent();
            if (dataObj != null)
            {
                Clipboard.SetDataObject(dataObj);
            }
            
            // Limpiar selección
            dgvCodes.ClearSelection();
            
            MessageBox.Show($"Se copiaron {_currentResults.Count} filas al portapapeles.", 
                "Copiado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al copiar: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvCodes_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        // Mantener el código tal como se pegó (no quitar prefijos)
        
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

            // Parsear códigos con información de categoría
            var parsedCodes = ParseCodesWithCategory(txtInput.Text);
            
            if (parsedCodes.Count == 0)
            {
                MessageBox.Show("No se encontraron códigos DTC válidos en el texto pegado.", 
                    "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Construir lista de códigos a buscar según su origen
            var allCodesToSearch = new List<string>();
            foreach (var parsed in parsedCodes)
            {
                if (parsed.WasCOrD)
                {
                    // Si empezaba con C o D, buscar tanto el original como el transformado en categoría U
                    allCodesToSearch.Add(parsed.OriginalCode);
                    allCodesToSearch.Add("U" + parsed.ConvertedCode);
                }
                else if (parsed.OriginalCode.StartsWith("P") || parsed.OriginalCode.StartsWith("U"))
                {
                    // Si ya tiene prefijo P o U, usar tal cual
                    allCodesToSearch.Add(parsed.OriginalCode);
                }
                else
                {
                    // Si es hex puro (4 dígitos), buscar con P y sin prefijo
                    allCodesToSearch.Add("P" + parsed.ConvertedCode);
                    allCodesToSearch.Add(parsed.ConvertedCode);
                }
            }

            // Buscar en base de datos
            var dbCodes = await _repository.GetByCodesAsync(allCodesToSearch);
            
            // Crear diccionario de resultados encontrados
            var dbCodesDict = dbCodes.ToDictionary(c => c.Code, c => c);

            // Crear resultados solo para la categoría correspondiente
            _currentResults = new List<DtcLookupResult>();
            
            foreach (var parsed in parsedCodes)
            {
                // Determinar la categoría y código de búsqueda según el código original
                string prefix;
                string searchCode;
                
                if (parsed.WasCOrD)
                {
                    // Códigos C/D → Network (U)
                    prefix = "U";
                    searchCode = "U" + parsed.ConvertedCode;
                }
                else if (parsed.OriginalCode.StartsWith("P"))
                {
                    // Ya tiene prefijo P → Powertrain
                    prefix = "P";
                    searchCode = parsed.OriginalCode;
                }
                else if (parsed.OriginalCode.StartsWith("U"))
                {
                    // Ya tiene prefijo U → Network
                    prefix = "U";
                    searchCode = parsed.OriginalCode;
                }
                else
                {
                    // Hex puro → Powertrain por defecto
                    prefix = "P";
                    searchCode = "P" + parsed.ConvertedCode;
                }
                
                // Buscar en múltiples variantes
                DtcCode? foundCode = null;
                if (dbCodesDict.ContainsKey(parsed.OriginalCode))
                {
                    foundCode = dbCodesDict[parsed.OriginalCode];
                }
                else if (dbCodesDict.ContainsKey(searchCode))
                {
                    foundCode = dbCodesDict[searchCode];
                }
                else if (dbCodesDict.ContainsKey(parsed.ConvertedCode))
                {
                    foundCode = dbCodesDict[parsed.ConvertedCode];
                }
                
                var found = foundCode != null;
                
                _currentResults.Add(new DtcLookupResult
                {
                    Code = parsed.OriginalCode, // Mantener formato original (C301, P0420, etc.)
                    CodeAlt = parsed.OriginalCode, // Inicializar con el mismo código
                    Found = found,
                    Description = found ? foundCode!.Description : null,
                    Category = found ? foundCode!.Category : GetCategoryFromPrefix(prefix),
                    Source = found ? foundCode!.Source : null,
                    Notes = found ? foundCode!.Notes : null,
                    DtcId = found ? foundCode!.Id : null
                });
            }

            // Mostrar resultados
            dgvCodes.DataSource = null;
            dgvCodes.DataSource = _currentResults;
            
            // Actualizar estadísticas
            var foundCount = _currentResults.Count(r => r.Found);
            var notFound = _currentResults.Count - foundCount;
            lblStats.Text = $"Total: {_currentResults.Count} | Encontrados: {foundCount} | No encontrados: {notFound}";
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

    private List<string> ParseHexCodesOnly(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();

        // Patrón para códigos: 4 caracteres hex O códigos que empiezan con C/D seguidos de 3 hex
        var hexPattern = new System.Text.RegularExpressions.Regex(@"\b[0-9A-F]{4}\b", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        var matches = hexPattern.Matches(input);
        var codes = new List<string>();

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var code = match.Value.ToUpperInvariant();
            
            // Convertir códigos C→0 y D→1 (ej: C29E→029E, D11E→111E)
            if (code.StartsWith("C"))
            {
                code = "0" + code.Substring(1);
            }
            else if (code.StartsWith("D"))
            {
                code = "1" + code.Substring(1);
            }
            
            codes.Add(code);
        }

        return codes;
    }

    private class ParsedCodeInfo
    {
        public string OriginalCode { get; set; } = "";
        public string ConvertedCode { get; set; } = "";
        public bool WasCOrD { get; set; }
    }

    private List<ParsedCodeInfo> ParseCodesWithCategory(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<ParsedCodeInfo>();

        // Patrón que captura: 
        // - P/U + 4 hex (P0420, U0360)
        // - C/D + 3 hex (C301, D11E)  
        // - 4 hex puros (0420, 079A)
        var hexPattern = new System.Text.RegularExpressions.Regex(@"\b(?:[PU][0-9A-F]{4}|[CD][0-9A-F]{3}|[0-9A-F]{4})\b", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        var matches = hexPattern.Matches(input);
        var codes = new List<ParsedCodeInfo>();

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var originalCode = match.Value.ToUpperInvariant();
            var convertedCode = originalCode;
            var wasCOrD = false;
            
            // Convertir códigos C→0 y D→1 y marcar que eran C o D
            if (originalCode.StartsWith("C"))
            {
                convertedCode = "0" + originalCode.Substring(1);
                wasCOrD = true;
            }
            else if (originalCode.StartsWith("D"))
            {
                convertedCode = "1" + originalCode.Substring(1);
                wasCOrD = true;
            }
            
            codes.Add(new ParsedCodeInfo
            {
                OriginalCode = originalCode,
                ConvertedCode = convertedCode,
                WasCOrD = wasCOrD
            });
        }

        return codes;
    }

    private string GetCategoryFromPrefix(string prefix)
    {
        return prefix switch
        {
            "P" => "Powertrain",
            "U" => "Network",
            _ => "Unknown"
        };
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
        
        if (dgvCodes.CurrentRow != null)
        {
            var selectedResult = dgvCodes.CurrentRow.DataBoundItem as DtcLookupResult;
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
        if (dgvCodes.CurrentRow == null)
        {
            MessageBox.Show("Por favor, selecciona un código para editar.", 
                "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedResult = dgvCodes.CurrentRow.DataBoundItem as DtcLookupResult;
        if (selectedResult == null || !selectedResult.Found)
        {
            MessageBox.Show("Este código no existe en la base de datos. Usa 'Añadir' para agregarlo.", 
                "Código no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // Cargar código completo desde BD
            DtcCode? dtcCode = null;
            if (selectedResult.DtcId.HasValue)
            {
                dtcCode = await _repository.GetByIdAsync(selectedResult.DtcId.Value);
            }
            else
            {
                dtcCode = await _repository.GetByCodeAsync(selectedResult.Code);
            }

            if (dtcCode == null)
            {
                MessageBox.Show("No se pudo cargar el código para editar.",
                    "Código no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

    // Métodos del menú
    private void MenuImportar_Click(object? sender, EventArgs e)
    {
        BtnImport_Click(sender, e);
    }

    private void MenuExportar_Click(object? sender, EventArgs e)
    {
        BtnExport_Click(sender, e);
    }

    private async void MenuLimpiarDB_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "¿Estás seguro de que deseas eliminar TODOS los códigos de la base de datos?\n\n" +
            "Esta acción NO se puede deshacer.",
            "Confirmar eliminación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2
        );

        if (result != DialogResult.Yes)
            return;

        try
        {
            Cursor = Cursors.WaitCursor;
            
            var deleted = await _repository.DeleteAllAsync();
            
            MessageBox.Show($"Se eliminaron {deleted:N0} códigos de la base de datos.",
                "Eliminación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Limpiar resultados y actualizar estadísticas
            BtnClear_Click(sender, e);
            LoadStatistics();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al limpiar la base de datos: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void MenuSalir_Click(object? sender, EventArgs e)
    {
        Application.Exit();
    }

    private async void MenuEstadisticas_Click(object? sender, EventArgs e)
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            
            var total = await _repository.GetCountAsync();
            
            // Obtener conteo por categorías
            var categoryCounts = await GetCategoryCountsAsync();
            
            var message = $"ESTADÍSTICAS DE LA BASE DE DATOS\n\n" +
                         $"Total de códigos: {total:N0}\n\n" +
                         $"Por categoría:\n" +
                         $"  • Powertrain (P): {categoryCounts["Powertrain"]:N0}\n" +
                         $"  • Network (U): {categoryCounts["Network"]:N0}\n" +
                         $"  • Otros: {categoryCounts["Other"]:N0}";
            
            MessageBox.Show(message, "Estadísticas de la Base de Datos",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al obtener estadísticas: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private async Task<Dictionary<string, int>> GetCategoryCountsAsync()
    {
        var allCodes = await _repository.GetAllAsync();
        
        return new Dictionary<string, int>
        {
            ["Powertrain"] = allCodes.Count(c => c.Category == "Powertrain"),
            ["Network"] = allCodes.Count(c => c.Category == "Network"),
            ["Other"] = allCodes.Count(c => c.Category != "Powertrain" && c.Category != "Network")
        };
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
        var hasSelection = dgvCodes.CurrentRow != null;
        var isFound = false;

        if (hasSelection)
        {
            var selectedResult = dgvCodes.CurrentRow.DataBoundItem as DtcLookupResult;
            isFound = selectedResult?.Found ?? false;
        }

        btnEdit.Enabled = hasSelection && isFound;
    }
}

// Clase para personalizar los colores del menú contextual
internal class CustomMenuColorTable : ProfessionalColorTable
{
    public override Color MenuItemSelected => ColorTranslator.FromHtml("#F8B41C"); // Amarillo marca
    public override Color MenuItemSelectedGradientBegin => ColorTranslator.FromHtml("#F8B41C");
    public override Color MenuItemSelectedGradientEnd => ColorTranslator.FromHtml("#F8B41C");
    public override Color MenuItemBorder => ColorTranslator.FromHtml("#D89C17");
    public override Color MenuItemPressedGradientBegin => ColorTranslator.FromHtml("#D89C17");
    public override Color MenuItemPressedGradientEnd => ColorTranslator.FromHtml("#D89C17");
    public override Color ImageMarginGradientBegin => ColorTranslator.FromHtml("#102C44");
    public override Color ImageMarginGradientEnd => ColorTranslator.FromHtml("#102C44");
    public override Color ToolStripDropDownBackground => ColorTranslator.FromHtml("#102C44");
}

// DataGridView con selección acumulativa (permite arrastre sin perder selección anterior)
public class CumulativeSelectionDataGridView : DataGridView
{
    private bool _isSelecting = false;
    private bool _shouldAddToSelection = false;
    
    protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e)
    {
        // Solo procesar clicks izquierdos en celdas válidas
        if (e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            _isSelecting = true;
            
            // Si no se presionó Ctrl, queremos agregar a la selección existente de todos modos
            _shouldAddToSelection = (ModifierKeys & Keys.Control) == 0;
            
            if (_shouldAddToSelection)
            {
                // Prevenir el comportamiento por defecto que limpia la selección
                // y manejar la selección manualmente
                var cell = this[e.ColumnIndex, e.RowIndex];
                
                // Alternar la selección de la celda actual
                cell.Selected = !cell.Selected;
                
                // Cancelar el evento para que no limpie la selección
                return;
            }
        }
        
        base.OnCellMouseDown(e);
    }
    
    protected override void OnCellMouseMove(DataGridViewCellMouseEventArgs e)
    {
        // Permitir arrastre para seleccionar múltiples celdas
        if (_isSelecting && e.RowIndex >= 0 && e.ColumnIndex >= 0 && _shouldAddToSelection)
        {
            var cell = this[e.ColumnIndex, e.RowIndex];
            
            // Solo seleccionar celdas durante el arrastre (no deseleccionar)
            if (!cell.Selected)
            {
                cell.Selected = true;
            }
        }
        
        base.OnCellMouseMove(e);
    }
    
    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isSelecting = false;
        _shouldAddToSelection = false;
        base.OnMouseUp(e);
    }
}

