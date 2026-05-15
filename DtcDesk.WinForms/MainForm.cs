using DtcDesk.Core.Models;
using DtcDesk.Core.Parsing;
using DtcDesk.Core.Services;
using DtcDesk.Data.Db;
using DtcDesk.Data.Repositories;
using DtcDesk.WinForms.Forms;
using System.Runtime.InteropServices;

namespace DtcDesk.WinForms;

public partial class MainForm : Form
{
    private sealed class ManualSelectionSnapshot
    {
        public string Code { get; init; } = string.Empty;
        public string CodeAlt { get; init; } = string.Empty;
        public bool Found { get; init; }
        public string? Description { get; init; }
        public string? Category { get; init; }
        public string? Source { get; init; }
        public string? Notes { get; init; }
        public string? FilterTag { get; init; }
        public string? Module { get; init; }
        public bool IsModuleDeleted { get; init; }
    }

    private const int GridZoomMin = 50;
    private const int GridZoomMax = 180;
    private const int GridZoomStep = 10;
    private const int GridZoomDefault = 100;

    private readonly DtcParser _parser;
    private readonly DtcRepository _repository;
    private readonly ConnectionFactory _connectionFactory;
    private readonly ModuleFilterRepository _moduleRepository;
    private DtcClassifierService? _classifier;
    private List<DtcLookupResult> _currentResults = new();
    private List<DtcModuleFilter> _moduleFilters = new();
    private readonly Dictionary<int, ManualSelectionSnapshot> _manualSelectionSnapshots = new();
    private readonly Dictionary<int, (string ModuleKey, ManualSelectionSnapshot Snapshot)> _moduleToggleSnapshots = new();
    private readonly SemaphoreSlim _clipboardSemaphore = new(1, 1);
    private bool _suppressSelectionChange;
    private bool _autoDeletingSelection;
    private bool _suppressAutoDelete;
    private bool _preserveSelectionOnNextDataBinding;
    private int _gridZoomPercent = GridZoomDefault;
    private FlowLayoutPanel? _moduleButtonsPanel;
    private Button? _btnManageModules;
    private int _dbTotalCodes = 0;
    
    // Controles para seleccionar tipo de OBD
    private RadioButton _rdoObd2 = new();
    private RadioButton _rdoObd1 = new();

    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    private void ApplyRoundedRegion(Control c, int radius)
    {
        if (c == null || c.Width == 0 || c.Height == 0) return;
        c.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, c.Width, c.Height, radius, radius));
    }

    private void MakeRounded(Control control, int radius)
    {
        if (control == null) return;
        ApplyRoundedRegion(control, radius);
        control.Resize -= (s, e) => ApplyRoundedRegion(control, radius); // prevenir duplicados en caso de re-llamadas
        control.Resize += (s, e) => ApplyRoundedRegion(control, radius);
    }

    public MainForm()
    {
        InitializeComponent();
        
        // Inicializar servicios
        _parser = new DtcParser();
        
        var dbPath = ConnectionFactory.GetDefaultDatabasePath();
        _connectionFactory = new ConnectionFactory(dbPath);
        
        // Inicializar base de datos (incluye nuevas tablas de módulos)
        var dbInitializer = new DbInitializer(_connectionFactory.GetConnectionString());
        dbInitializer.Initialize();
        
        _repository = new DtcRepository(_connectionFactory);
        _moduleRepository = new ModuleFilterRepository(_connectionFactory.GetConnectionString());
        
        // Configurar UI
        SetupUI();
        LoadStatistics();
        
        // Inicializar clasificador con reglas de BD (async fire-and-forget seguro en startup)
        _ = InitializeClassifierAsync();
    }

    /// <summary>
    /// Carga reglas desde BD, ejecuta seeding si es necesario, e inicializa el clasificador.
    /// </summary>
    private async Task InitializeClassifierAsync()
    {
        try
        {
            // Sembrar reglas del cliente si la BD está vacía
            await _moduleRepository.SeedDefaultRulesAsync();
            
            // Cargar reglas en memoria
            _moduleFilters = await _moduleRepository.GetAllFiltersAsync();
            var exactRules = await _moduleRepository.GetAllExactRulesAsync();
            var keywords = await _moduleRepository.GetAllKeywordsAsync();
            
            _classifier = new DtcClassifierService(exactRules, keywords);

            // Cargar botones de módulos desde el arranque, incluso antes de que se muestre la ventana.
            await LoadModuleButtonsAsync();
        }
        catch (Exception ex)
        {
            // No es crítico — la app funciona sin clasificador
            System.Diagnostics.Debug.WriteLine($"[Clasificador] Error al inicializar: {ex.Message}");
        }
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
        btnZoomIn.Click += BtnZoomIn_Click;
        btnZoomOut.Click += BtnZoomOut_Click;
        btnZoomReset.Click += BtnZoomReset_Click;
        btnCopyCodeColumn.Click += async (s, e) => await CopyWholeColumnToClipboardAsync("colCode");
        btnCopyCodeAltColumn.Click += async (s, e) => await CopyWholeColumnToClipboardAsync("colCodeAlt");
        btnClearSelectionTop.Click += (s, e) => ClearGridSelection();
        btnFormatObd.Click += (_, _) => SetObdMode(true);
        btnFormatSpn.Click += (_, _) => SetObdMode(false);
        
        // Configurar eventos del menú
        menuImportar.Click += MenuImportar_Click;
        menuExportar.Click += MenuExportar_Click;
        menuLimpiarDB.Click += MenuLimpiarDB_Click;
        menuSalir.Click += MenuSalir_Click;
        menuEstadisticas.Click += MenuEstadisticas_Click;
        
        dgvCodes.CellDoubleClick += DgvCodes_CellDoubleClick;
        dgvCodes.CellMouseDown += DgvCodes_CellMouseDown;
        dgvCodes.SelectionChanged += DgvCodes_SelectionChanged;
        dgvCodes.MouseWheel += DgvCodes_MouseWheel;
        dgvCodes.Scroll += (s, e) => AlignCopyColumnButtons();
        dgvCodes.ColumnWidthChanged += (s, e) => AlignCopyColumnButtons();
        dgvCodes.Resize += (s, e) => AlignCopyColumnButtons();

        // Contador de líneas en tiempo real
        txtInput.TextChanged += (s, e) =>
        {
            var lines = string.IsNullOrWhiteSpace(txtInput.Text) ? 0
                : txtInput.Text.Split('\n').Length;
            lblLineCount.Text = $"≡  {lines} línea{(lines == 1 ? "" : "s")}";
        };
        
        txtInput.Font = new Font("Consolas", 10F);
        ApplyGridZoom();
        AlignCopyColumnButtons();

        // Estado vacío visible al inicio
        ShowEmptyState(true);

        SetupDynamicModulePanel();

        // Fallback de seguridad: al mostrarse la ventana, asegurar que los botones estén renderizados.
        Shown += async (_, _) =>
        {
            if (_moduleButtonsPanel != null && _moduleButtonsPanel.Controls.Count == 0)
            {
                await LoadModuleButtonsAsync();
            }
        };

        // Buscador
        btnSearch.Click     += async (s, e) => await ExecuteSearchAsync();
        btnSearchClear.Click += (s, e) => ClearSearch();
        txtSearch.KeyDown   += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await ExecuteSearchAsync();
            }
        };
    }

    private void ShowEmptyState(bool show)
    {
        if (panelEmptyState == null || dgvCodes == null) return;
        dgvCodes.Visible = !show;
        panelEmptyState.Visible = show;
    }

    private void LoadLogo()
    {
        try
        {
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.jpg");
            if (File.Exists(logoPath))
            {
                var img = Image.FromFile(logoPath);
                picLogo.Image = img;
                picLogo.BackColor = Color.Transparent;
                // Reutilizar el mismo logo en el panel derecho si no hay uno separado
                picLogoRight.Image = img;
                picLogoRight.BackColor = Color.Transparent;
            }
            else
            {
                picLogo.Visible = false;
                picLogoRight.Visible = false;
            }
        }
        catch
        {
            picLogo.Visible = false;
            picLogoRight.Visible = false;
        }
    }

    private void ApplyDarkTheme()
    {
        // ─── Paleta de colores ───────────────────────────────────────────
        var bgMain       = ColorTranslator.FromHtml("#0F1E2B");
        var bgSide       = ColorTranslator.FromHtml("#153C59");
        var bgTop        = ColorTranslator.FromHtml("#102C44");
        var bgCard       = ColorTranslator.FromHtml("#112233");
        var textMain     = ColorTranslator.FromHtml("#EAEAEA");
        var textSecondary = ColorTranslator.FromHtml("#B0B7BE");
        var separator    = ColorTranslator.FromHtml("#2A3B4C");
        var accentYellow = ColorTranslator.FromHtml("#F8B41C");
        var accentHover  = ColorTranslator.FromHtml("#D89C17");
        var colorFound   = ColorTranslator.FromHtml("#27AE60");
        var colorNotFound = ColorTranslator.FromHtml("#E74C3C");

        // ─── Fondo principal ─────────────────────────────────────────────
        this.BackColor = bgMain;

        // ─── Panel superior ──────────────────────────────────────────────
        panelTop.BackColor = bgTop;
        lblTitle.ForeColor = textMain;
        lblTitle.BackColor = Color.Transparent;
        lblSubtitle.ForeColor = textSecondary;
        lblSubtitle.BackColor = Color.Transparent;
        lblStats.ForeColor = textSecondary; // invisible, mantenemos por compatibilidad

        // ─── Tarjetas de estadísticas ─────────────────────────────────────
        panelStatsBar.BackColor = bgSide;

        // Tarjeta TOTAL — borde izquierdo naranja
        StyleStatCard(panelStatTotal, bgCard, accentYellow);
        lblStatTotalIcon.ForeColor  = accentYellow;
        lblStatTotalIcon.BackColor  = Color.Transparent;
        lblStatTotalValue.ForeColor = textMain;
        lblStatTotalValue.BackColor = Color.Transparent;
        lblStatTotalLabel.ForeColor = textSecondary;
        lblStatTotalLabel.BackColor = Color.Transparent;

        // Tarjeta ENCONTRADOS — borde izquierdo verde
        StyleStatCard(panelStatFound, bgCard, colorFound);
        lblStatFoundIcon.ForeColor  = colorFound;
        lblStatFoundIcon.BackColor  = Color.Transparent;
        lblStatFoundValue.ForeColor = colorFound;
        lblStatFoundValue.BackColor = Color.Transparent;
        lblStatFoundLabel.ForeColor = textSecondary;
        lblStatFoundLabel.BackColor = Color.Transparent;

        // Tarjeta NO ENCONTRADOS — borde izquierdo rojo
        StyleStatCard(panelStatNotFound, bgCard, colorNotFound);
        lblStatNotFoundIcon.ForeColor  = colorNotFound;
        lblStatNotFoundIcon.BackColor  = Color.Transparent;
        lblStatNotFoundValue.ForeColor = colorNotFound;
        lblStatNotFoundValue.BackColor = Color.Transparent;
        lblStatNotFoundLabel.ForeColor = textSecondary;
        lblStatNotFoundLabel.BackColor = Color.Transparent;

        // ─── Panel izquierdo ─────────────────────────────────────────────
        panelLeft.BackColor = bgSide;
        lblInput.ForeColor = accentYellow;
        lblInput.BackColor = Color.Transparent;
        lblLineCount.ForeColor = textSecondary;
        lblLineCount.BackColor = Color.Transparent;
        txtInput.BackColor = bgMain;
        txtInput.ForeColor = textMain;
        txtInput.BorderStyle = BorderStyle.FixedSingle;

        // ─── Panel de filtros lateral ─────────────────────────────────────
        panelFilterSide.BackColor = bgSide;
        lblFilterTitle.ForeColor = accentYellow;
        lblFilterTitle.BackColor = Color.Transparent;

        var legacyButtons = new[] { btnFilterVNT, btnFilterDPF, btnFilterEGR,
                                    btnFilterNOX, btnFilterSCR, btnFilterMAF, btnFilterTVA };
        foreach (var lb in legacyButtons) { lb.Visible = false; lb.Enabled = false; }

        // ─── Panel derecho ────────────────────────────────────────────────
        panelRight.BackColor = bgMain;
        panelButtons.BackColor = bgMain;
        lblResults.ForeColor = accentYellow;
        lblResults.BackColor = Color.Transparent;

        // Contenedor principal del DataGridView (crea el efecto de borde y también se redondea)
        if (panelGridContainer != null)
        {
            panelGridContainer.BackColor = separator;
            MakeRounded(panelGridContainer, 8);
        }
        panelGridContainer.Padding = new Padding(1);

        // Estado vacío
        panelEmptyState.BackColor = bgTop;
        lblEmptyStateIcon.ForeColor = textSecondary;
        lblEmptyStateIcon.BackColor = Color.Transparent;
        lblEmptyStateTitle.ForeColor = textMain;
        lblEmptyStateTitle.BackColor = Color.Transparent;
        lblEmptyStateDesc.ForeColor = textSecondary;
        lblEmptyStateDesc.BackColor = Color.Transparent;

        // ─── DataGridView ─────────────────────────────────────────────────
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

        // ─── Botones de Formato - Izquierda ───────────────────────────────
        StyleButton(btnFormatObd, ColorTranslator.FromHtml("#F8CE5A"), Color.Black);
        MakeRounded(btnFormatObd, 5);

        StyleButton(btnFormatSpn, ColorTranslator.FromHtml("#263D52"), ColorTranslator.FromHtml("#B5C4D3"));
        MakeRounded(btnFormatSpn, 5);

        // ─── Botones Inferiores - Izquierda ───────────────────────────────
        StyleButton(btnParse, accentYellow, Color.Black);
        btnParse.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnParse.FlatAppearance.BorderSize = 0;
        MakeRounded(btnParse, 5);

        // btnClear — color diferenciado del botón principal
        StyleButton(btnClear, ColorTranslator.FromHtml("#1E3A4A"), ColorTranslator.FromHtml("#B0B7BE"));
        btnClear.FlatAppearance.BorderSize = 0;
        MakeRounded(btnClear, 5);

        // Botones Inferiores - Derecha
        StyleButton(btnAdd, bgSide, textMain);
        StyleButton(btnEdit, bgSide, Color.White); // Texto blanco puro
        MakeRounded(btnAdd, 5);
        MakeRounded(btnEdit, 5);
        btnEdit.Paint -= DrawAccentBorder; // prevenir doble subscripción
        btnEdit.Paint += DrawAccentBorder;

        // Botones de zoom
        StyleButton(btnZoomOut, bgTop, textMain);
        StyleButton(btnZoomReset, bgTop, textMain);
        StyleButton(btnZoomIn, bgTop, textMain);
        MakeRounded(btnZoomOut, 5);
        MakeRounded(btnZoomReset, 5);
        MakeRounded(btnZoomIn, 5);

        // Botones de copia de columna
        StyleButton(btnCopyCodeColumn,    bgTop, textSecondary);
        StyleButton(btnCopyCodeAltColumn, bgTop, textSecondary);
        StyleButton(btnClearSelectionTop, separator, textMain);
        btnCopyCodeColumn.FlatAppearance.BorderSize    = 1;
        btnCopyCodeColumn.FlatAppearance.BorderColor   = separator;
        btnCopyCodeAltColumn.FlatAppearance.BorderSize = 1;
        btnCopyCodeAltColumn.FlatAppearance.BorderColor= separator;
        btnClearSelectionTop.FlatAppearance.BorderSize = 1;
        btnClearSelectionTop.FlatAppearance.BorderColor= separator;
        btnCopyCodeColumn.Font    = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnCopyCodeAltColumn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnClearSelectionTop.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        panelColumnCopy.BackColor = bgMain;
        AlignCopyColumnButtons();

        // ─── Menú ─────────────────────────────────────────────────────────
        menuStrip.BackColor = bgSide;
        menuStrip.ForeColor = textMain;
        menuArchivo.ForeColor = textMain;
        menuHerramientas.ForeColor = textMain;

        // ─── Buscador ─────────────────────────────────────────────────────
        txtSearch.BackColor = bgMain;
        txtSearch.ForeColor = textMain;
        txtSearch.BorderStyle = BorderStyle.None; // Mejor interfaz al estar redondeado
        StyleButton(btnSearch, accentYellow, Color.White);
        btnSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        MakeRounded(btnSearch, 5);
        
        StyleButton(btnSearchClear, separator, textMain);
        btnSearchClear.FlatAppearance.BorderSize = 0;
        MakeRounded(btnSearchClear, 5);
        
        lblSearchMode.ForeColor = accentYellow;
        lblSearchMode.BackColor = Color.Transparent;

        // ─── Status Strip ────────────────────────────────────────────────
        statusStrip.BackColor = bgSide;
        statusStrip.ForeColor = textSecondary;
        foreach (ToolStripItem item in statusStrip.Items)
        {
            item.ForeColor = textSecondary;
            item.BackColor = bgSide;
        }
        statusLabelCount.ForeColor = accentYellow;

        // ─── Panel de módulos dinámico ────────────────────────────────────
        if (_moduleButtonsPanel != null)
            _moduleButtonsPanel.BackColor = bgSide;

        if (_btnManageModules != null)
        {
            StyleButton(_btnManageModules, accentYellow, Color.Black);
            _btnManageModules.FlatAppearance.BorderSize = 1;
            _btnManageModules.FlatAppearance.BorderColor = accentHover;
            _btnManageModules.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        }

        // ─── Botones de selección OBD ─────────────────────────────────────
        _rdoObd2.Text = "OBD-II";
        _rdoObd2.Appearance = Appearance.Button;
        _rdoObd2.FlatStyle = FlatStyle.Flat;
        _rdoObd2.FlatAppearance.BorderSize = 0;
        _rdoObd2.TextAlign = ContentAlignment.MiddleCenter;
        _rdoObd2.Size = new Size(135, 30);
        _rdoObd2.Location = new Point(12, 56); // Debajo del titulo
        _rdoObd2.Anchor = AnchorStyles.Top | AnchorStyles.Left; 
        _rdoObd2.Checked = true;
        _rdoObd2.Cursor = Cursors.Hand;
        _rdoObd2.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        _rdoObd1.Text = "OBD-I";
        _rdoObd1.Appearance = Appearance.Button;
        _rdoObd1.FlatStyle = FlatStyle.Flat;
        _rdoObd1.FlatAppearance.BorderSize = 0;
        _rdoObd1.TextAlign = ContentAlignment.MiddleCenter;
        _rdoObd1.Size = new Size(135, 30);
        _rdoObd1.Location = new Point(159, 56);
        _rdoObd1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        _rdoObd1.Cursor = Cursors.Hand;
        _rdoObd1.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        MakeRounded(_rdoObd2, 5);
        MakeRounded(_rdoObd1, 5);

        EventHandler styleToggles = (s, e) => {
            var isObd2 = _rdoObd2.Checked;
            _rdoObd2.BackColor = isObd2 ? accentYellow : separator;
            _rdoObd2.ForeColor = isObd2 ? Color.Black : textMain;
            _rdoObd1.BackColor = isObd2 ? separator : accentYellow;
            _rdoObd1.ForeColor = isObd2 ? textMain : Color.Black;

            btnFormatObd.BackColor = isObd2 ? accentYellow : separator;
            btnFormatObd.ForeColor = isObd2 ? Color.Black : textMain;
            btnFormatSpn.BackColor = isObd2 ? separator : accentYellow;
            btnFormatSpn.ForeColor = isObd2 ? textMain : Color.Black;
        };
        _rdoObd2.CheckedChanged += styleToggles;
        _rdoObd1.CheckedChanged += styleToggles;
        styleToggles(null, EventArgs.Empty);

        _rdoObd2.Visible = false;
        _rdoObd1.Visible = false;
        panelLeft.Controls.Add(_rdoObd2);
        panelLeft.Controls.Add(_rdoObd1);
    }

    private void SetObdMode(bool isObd2)
    {
        _rdoObd2.Checked = isObd2;
        _rdoObd1.Checked = !isObd2;
    }

    /// <summary>Dibuja el fondo con borde izquierdo de color para cada tarjeta de stat.</summary>
    private static void StyleStatCard(Panel panel, Color backColor, Color borderColor)
    {
        panel.BackColor = backColor;
        panel.Paint -= StatCard_Paint; // evitar doble suscripción
        panel.Paint += StatCard_Paint;
        panel.Tag = borderColor;
    }

    private static void StatCard_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Panel p) return;
        var borderColor = p.Tag is Color c ? c : Color.Orange;
        using var pen = new Pen(borderColor, 4);
        e.Graphics.DrawLine(pen, 0, 0, 0, p.Height);
    }

    private void StyleButton(Button btn, Color backColor, Color foreColor)
    {
        btn.BackColor = backColor;
        btn.ForeColor = foreColor;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        if (btn.Font == null || btn.Font.Name != "Segoe UI")
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btn.Cursor = Cursors.Hand;
    }

    /// <summary>Actualiza las 3 tarjetas de estadísticas y el contador inferior.</summary>
    private void UpdateStatsCards(int total, int found, int notFound)
    {
        lblStatTotalValue.Text    = total.ToString();
        lblStatFoundValue.Text    = found.ToString();
        lblStatNotFoundValue.Text = notFound.ToString();
        // Label legacy oculto — mantenemos por si hay referencias internas
        lblStats.Text = $"Total: {total} | Encontrados: {found} | No encontrados: {notFound}";
    }

    private void SetupDynamicModulePanel()
    {
        if (_moduleButtonsPanel != null) return;

        var accent = ColorTranslator.FromHtml("#F8B41C");
        var accentHover = ColorTranslator.FromHtml("#D89C17");

        _moduleButtonsPanel = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = true,
            AutoScroll    = true,
            Padding       = new Padding(16, 8, 16, 64),
            BackColor     = ColorTranslator.FromHtml("#153C59")
        };

        _btnManageModules = new Button
        {
            Text      = "+  Añadir Módulo",
            Height    = 38,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        StyleButton(_btnManageModules, accent, Color.Black);
        _btnManageModules.FlatAppearance.BorderSize = 0;
        _btnManageModules.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        MakeRounded(_btnManageModules, 6);
        _btnManageModules.Click += async (_, _) => await CreateCustomModuleAsync();

        _btnManageModules.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        UpdateModulePanelLayout();
        panelFilterSide.Resize += (_, _) => UpdateModulePanelLayout();
        _moduleButtonsPanel.SizeChanged += (_, _) => UpdateModuleButtonWidths();

        panelFilterSide.Controls.Add(_moduleButtonsPanel);
        panelFilterSide.Controls.Add(_btnManageModules);
        _moduleButtonsPanel.BringToFront();
    }

    private void UpdateModulePanelLayout()
    {
        if (_btnManageModules == null || panelFilterSide == null)
        {
            return;
        }

        var sidePadding = 12;
        var width = Math.Max(0, panelFilterSide.ClientSize.Width - (sidePadding * 2));
        _btnManageModules.Width = width;
        _btnManageModules.Left = sidePadding;
        _btnManageModules.Top = Math.Max(0, panelFilterSide.ClientSize.Height - _btnManageModules.Height - 12);

        UpdateModuleButtonWidths();
    }

    private void UpdateModuleButtonWidths()
    {
        if (_moduleButtonsPanel == null || panelFilterSide == null)
        {
            return;
        }

        const int gap = 12;
        var scrollBar = _moduleButtonsPanel.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
        var available = _moduleButtonsPanel.ClientSize.Width - _moduleButtonsPanel.Padding.Horizontal - scrollBar;
        if (available <= 0)
        {
            return;
        }

        // Two columns: width + gap + width + gap <= available
        var columnWidth = Math.Max(90, (available - (gap * 2)) / 2);

        foreach (Control control in _moduleButtonsPanel.Controls)
        {
            if (control is Button button)
            {
                button.Width = columnWidth;
                button.Margin = new Padding(0, 0, gap, 12);
            }
        }
    }

    private void DrawAccentBorder(object? sender, PaintEventArgs e)
    {
        if (sender is Control c)
        {
            var accent = ColorTranslator.FromHtml("#F8B41C");
            using var pen = new Pen(accent, 1.5f);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Dibuja un rectángulo redondeado al interior de los límites evitando el recorte del Region
            int r = 5;
            int d = r * 2;
            var rect = new Rectangle(0, 0, c.Width - 1, c.Height - 1);
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            
            e.Graphics.DrawPath(pen, path);
        }
    }

    private async Task LoadModuleButtonsAsync()
    {
        if (_moduleButtonsPanel == null) return;

        var bgBtn    = ColorTranslator.FromHtml("#102C44");
        var fgBtn    = ColorTranslator.FromHtml("#EAEAEA");
        var accent   = ColorTranslator.FromHtml("#F8B41C");
        var bgMenu   = ColorTranslator.FromHtml("#102C44");
        var fgMenu   = ColorTranslator.FromHtml("#EAEAEA");

        const int buttonGap = 12;
        var scrollBar = _moduleButtonsPanel.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
        var available = _moduleButtonsPanel.ClientSize.Width - _moduleButtonsPanel.Padding.Horizontal - scrollBar;
        var columnWidth = Math.Max(90, available > 0 ? (available - (buttonGap * 2)) / 2 : 130);

        _moduleFilters = await _moduleRepository.GetAllFiltersAsync();
        _moduleButtonsPanel.Controls.Clear();

        foreach (var filter in _moduleFilters)
        {
            // Ancho pensado para que entren dos columnas dentro del panel lateral de 312px.
            // Padding izquierdo 12, cada botón 130 + margen derecho 16 = 146.
            // Dos botones = 292. Más padding izq 12 = 304 (con pad extra en la derecha queda perfeto centrado).
            var moduleButton = new Button
            {
                Text      = filter.DisplayName,
                Width     = columnWidth,
                Height    = 36,
                Margin    = new Padding(0, 0, buttonGap, 12),
                Tag       = filter,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };

            StyleButton(moduleButton, bgBtn, fgBtn);
            moduleButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            moduleButton.FlatAppearance.BorderSize  = 0;
            MakeRounded(moduleButton, 5);
            moduleButton.Paint += DrawAccentBorder;
            moduleButton.Click += (_, _) => DeleteByModule(filter);

            // Hover effect
            moduleButton.MouseEnter += (_, _) =>
            {
                moduleButton.BackColor = ColorTranslator.FromHtml("#1a3d5c");
            };
            moduleButton.MouseLeave += (_, _) =>
            {
                moduleButton.BackColor = bgBtn;
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.BackColor = bgMenu;
            contextMenu.ForeColor = fgMenu;
            var editItem   = new ToolStripMenuItem("✏  Editar módulo");
            var deleteItem = new ToolStripMenuItem("🗑  Eliminar módulo");
            editItem.ForeColor   = fgMenu;
            deleteItem.ForeColor = fgMenu;

            editItem.Click   += async (_, _) => await EditCustomModuleAsync(filter);
            deleteItem.Click += async (_, _) => await DeleteCustomModuleAsync(filter);

            contextMenu.Items.Add(editItem);
            contextMenu.Items.Add(deleteItem);
            moduleButton.ContextMenuStrip = contextMenu;

            _moduleButtonsPanel.Controls.Add(moduleButton);
        }

        UpdateModuleButtonWidths();
    }

    private async Task CreateCustomModuleAsync()
    {
        using var editor = new CustomModuleEditorForm();
        if (editor.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _moduleRepository.CreateCustomFilterAsync(editor.ModuleDisplayName, editor.ModuleDescription, editor.ExactCodes);
            await RefreshClassifierAndButtonsAsync();
            MessageBox.Show("Módulo personalizado creado correctamente.", "Módulo creado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo crear el módulo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task EditCustomModuleAsync(DtcModuleFilter filter)
    {
        try
        {
            var codes = await _moduleRepository.GetExactCodesByFilterAsync(filter.Name);
            using var editor = new CustomModuleEditorForm(filter, codes);
            if (editor.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await _moduleRepository.UpdateCustomFilterAsync(filter.Id, editor.ModuleDisplayName, editor.ModuleDescription, editor.ExactCodes);
            await RefreshClassifierAndButtonsAsync();
            MessageBox.Show("Módulo actualizado correctamente.", "Módulo actualizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo actualizar el módulo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteCustomModuleAsync(DtcModuleFilter filter)
    {
        var result = MessageBox.Show(
            $"¿Eliminar el módulo '{filter.DisplayName}'?\n\nEsta acción también elimina sus códigos configurados.",
            "Confirmar eliminación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _moduleRepository.DeleteCustomFilterAsync(filter.Id);
            await RefreshClassifierAndButtonsAsync();
            MessageBox.Show("Módulo eliminado correctamente.", "Módulo eliminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo eliminar el módulo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RefreshClassifierAndButtonsAsync()
    {
        _moduleFilters = await _moduleRepository.GetAllFiltersAsync();
        var exactRules = await _moduleRepository.GetAllExactRulesAsync();
        var keywords = await _moduleRepository.GetAllKeywordsAsync();
        _classifier = new DtcClassifierService(exactRules, keywords);
        await LoadModuleButtonsAsync();

        if (_currentResults.Count > 0)
        {
            _classifier.ClassifyAll(_currentResults);
            ApplyModuleDisplayNames();
            dgvCodes.Refresh();
        }
    }

    private void SetupDataGridView()
    {
        dgvCodes.AutoGenerateColumns = false;
        dgvCodes.AllowUserToAddRows = false;
        dgvCodes.AllowUserToDeleteRows = false;
        dgvCodes.AllowUserToResizeColumns = false;
        dgvCodes.AllowUserToResizeRows = false;
        dgvCodes.ReadOnly = true;
        dgvCodes.SelectionMode = DataGridViewSelectionMode.CellSelect;
        dgvCodes.MultiSelect = true;
        dgvCodes.RowHeadersVisible = false;
        dgvCodes.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
        dgvCodes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        dgvCodes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        dgvCodes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvCodes.RowTemplate.Height = 35;
        dgvCodes.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        
        // Menú contextual para copiar
        SetupContextMenu();
        
        // Columnas
        dgvCodes.Columns.Clear();

        dgvCodes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colRowNumber",
            HeaderText = "REAL",
            DataPropertyName = "OriginalCode",
            Width = 80,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Consolas", 10F, FontStyle.Bold)
            }
        });
        
        dgvCodes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCode",
            HeaderText = "CÓDIGO",
            DataPropertyName = "Code",
            Width = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Consolas", 10F, FontStyle.Bold) }
        });
        
        dgvCodes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCodeAlt",
            HeaderText = "COL. FFFF",
            DataPropertyName = "CodeAlt",
            Width = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Consolas", 10F, FontStyle.Bold) }
        });
        
        dgvCodes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colDescription",
            HeaderText = "DESCRIPCIÓN",
            DataPropertyName = "Description",
            SortMode = DataGridViewColumnSortMode.NotSortable,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        dgvCodes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colStatus",
            HeaderText = "ESTADO",
            DataPropertyName = "Found",
            SortMode = DataGridViewColumnSortMode.NotSortable,
            Width = 100
        });
        
        // Formato condicional para el estado
        dgvCodes.CellFormatting += DgvCodes_CellFormatting;
        dgvCodes.DataBindingComplete += DgvCodes_DataBindingComplete;

        AlignCopyColumnButtons();
    }

    private void AlignCopyColumnButtons()
    {
        if (!dgvCodes.Columns.Contains("colCode") || !dgvCodes.Columns.Contains("colCodeAlt"))
        {
            return;
        }

        var codeRect = dgvCodes.GetColumnDisplayRectangle(dgvCodes.Columns["colCode"].Index, true);
        var codeAltRect = dgvCodes.GetColumnDisplayRectangle(dgvCodes.Columns["colCodeAlt"].Index, true);

        // Evitar valores negativos/ocultos cuando hay scroll extremo.
        var top = Math.Max(3, btnCopyCodeColumn.Top);
        var buttonHeight = 30;

        if (codeRect.Width > 0)
        {
            btnCopyCodeColumn.Left = Math.Max(0, codeRect.X);
            btnCopyCodeColumn.Width = Math.Max(70, codeRect.Width);
            btnCopyCodeColumn.Top = top;
            btnCopyCodeColumn.Height = buttonHeight;
        }

        if (codeAltRect.Width > 0)
        {
            btnCopyCodeAltColumn.Left = Math.Max(0, codeAltRect.X);
            btnCopyCodeAltColumn.Width = Math.Max(70, codeAltRect.Width);
            btnCopyCodeAltColumn.Top = top;
            btnCopyCodeAltColumn.Height = buttonHeight;
        }

        btnClearSelectionTop.Top = top;
        btnClearSelectionTop.Height = buttonHeight;
        btnClearSelectionTop.Width = 128;
        btnClearSelectionTop.Left = Math.Max(0, panelColumnCopy.Width - btnClearSelectionTop.Width - 8);
    }

    private void ApplyGridZoom()
    {
        var scale = _gridZoomPercent / 100f;

        dgvCodes.DefaultCellStyle.Font = new Font("Segoe UI", 9F * scale, FontStyle.Regular);
        dgvCodes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F * scale, FontStyle.Bold);

        if (dgvCodes.Columns.Contains("colCode"))
        {
            dgvCodes.Columns["colCode"].DefaultCellStyle.Font = new Font("Consolas", 10F * scale, FontStyle.Bold);
        }

        if (dgvCodes.Columns.Contains("colCodeAlt"))
        {
            dgvCodes.Columns["colCodeAlt"].DefaultCellStyle.Font = new Font("Consolas", 10F * scale, FontStyle.Bold);
        }

        dgvCodes.RowTemplate.Height = (int)Math.Clamp(35 * scale, 24, 70);
        dgvCodes.ColumnHeadersHeight = (int)Math.Clamp(28 * scale, 24, 56);

        foreach (DataGridViewRow row in dgvCodes.Rows)
        {
            row.Height = dgvCodes.RowTemplate.Height;
        }

        btnZoomReset.Text = $"{_gridZoomPercent}%";
    }

    private void ChangeGridZoom(int delta)
    {
        var newZoom = Math.Clamp(_gridZoomPercent + delta, GridZoomMin, GridZoomMax);
        if (newZoom == _gridZoomPercent)
        {
            return;
        }

        _gridZoomPercent = newZoom;
        ApplyGridZoom();
    }

    private void ResetGridZoom()
    {
        _gridZoomPercent = GridZoomDefault;
        ApplyGridZoom();
    }

    private void DgvCodes_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        if (_preserveSelectionOnNextDataBinding)
        {
            _preserveSelectionOnNextDataBinding = false;
            ReapplyManualSelectionToGrid();
            ReapplyModuleSelectionToGrid();
            return;
        }

        ClearGridSelection();

        // Asegurar que no quede la primera celda seleccionada automáticamente tras el binding.
        BeginInvoke((MethodInvoker)(() => ClearGridSelection()));
    }

    private void ClearGridSelection()
    {
        dgvCodes.ClearSelection();
        dgvCodes.CurrentCell = null;
    }

    private void SelectColumnCells(string columnName)
    {
        if (dgvCodes.Rows.Count == 0)
        {
            return;
        }

        if (!dgvCodes.Columns.Contains(columnName))
        {
            return;
        }

        _suppressSelectionChange = true;
        try
        {
            dgvCodes.ClearSelection();

            foreach (DataGridViewRow row in dgvCodes.Rows)
            {
                var cell = row.Cells[columnName];
                if (cell != null)
                {
                    cell.Selected = true;
                }
            }

            dgvCodes.CurrentCell = null;
        }
        finally
        {
            _suppressSelectionChange = false;
        }
    }

    private async Task CopyWholeColumnToClipboardAsync(string columnName)
    {
        if (_currentResults == null || _currentResults.Count == 0)
        {
            return;
        }

        if (!dgvCodes.Columns.Contains(columnName))
        {
            return;
        }

        var values = new List<string>(_currentResults.Count);
        foreach (var result in _currentResults)
        {
            string? value = columnName switch
            {
                "colCode" => result.Code,
                "colCodeAlt" => result.CodeAlt,
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(value.Trim());
            }
        }

        if (values.Count == 0)
        {
            return;
        }

        var copied = await TrySetClipboardTextAsync(string.Join(Environment.NewLine, values));
        if (!copied)
        {
            MessageBox.Show(
                "No se pudo acceder al portapapeles en este momento. Cierra aplicaciones que lo estén usando e intenta de nuevo.",
                "Portapapeles ocupado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        // No alterar selección después de copiar.
        // Limpiarla aquí provoca restauraciones no deseadas en el siguiente clic.
    }

    private static bool IsSelectableCodeColumn(DataGridViewColumn? column)
    {
        return column != null && (column.Name == "colCode" || column.Name == "colCodeAlt");
    }

    private void SetupContextMenu()
    {
        // Deshabilitar menú contextual por click derecho.
        dgvCodes.ContextMenuStrip = null;
        
        // También permitir Ctrl+C/Ctrl+Shift+C directamente y Backspace para borrar/reemplazar códigos
        dgvCodes.KeyDown += DgvCodes_KeyDown;
    }

    private async void DgvCodes_KeyDown(object? sender, KeyEventArgs e)
    {
        // Zoom rápido con teclado
        if (e.Control && (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus))
        {
            ChangeGridZoom(GridZoomStep);
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }

        if (e.Control && (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus))
        {
            ChangeGridZoom(-GridZoomStep);
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }

        if (e.Control && (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0))
        {
            ResetGridZoom();
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }

        // Copiar con Ctrl+Shift+C (vertical)
        if (e.Control && e.Shift && e.KeyCode == Keys.C)
        {
            await CopySelectedCellsToClipboardAsync(false);
            e.Handled = true;
        }

        // Copiar con Ctrl+C (horizontal)
        if (e.Control && !e.Shift && e.KeyCode == Keys.C)
        {
            await CopySelectedCellsToClipboardAsync(true);
            e.Handled = true;
        }

        // Deseleccionar con Escape
        if (e.KeyCode == Keys.Escape)
        {
            ClearGridSelection();
            e.Handled = true;
        }
        
        // Borrar/Reemplazar códigos con Backspace
        if (e.KeyCode == Keys.Back)
        {
            DeleteAndReplaceSelectedCodes();
            e.Handled = true;
        }
    }

    private void DgvCodes_MouseWheel(object? sender, MouseEventArgs e)
    {
        if ((ModifierKeys & Keys.Control) != Keys.Control)
        {
            return;
        }

        ChangeGridZoom(e.Delta > 0 ? GridZoomStep : -GridZoomStep);

        if (e is HandledMouseEventArgs handledArgs)
        {
            handledArgs.Handled = true;
        }
    }

    private void BtnZoomIn_Click(object? sender, EventArgs e)
    {
        ChangeGridZoom(GridZoomStep);
    }

    private void BtnZoomOut_Click(object? sender, EventArgs e)
    {
        ChangeGridZoom(-GridZoomStep);
    }

    private void BtnZoomReset_Click(object? sender, EventArgs e)
    {
        ResetGridZoom();
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
        _manualSelectionSnapshots.Clear();
        _moduleToggleSnapshots.Clear();
        _suppressSelectionChange = true;
        try
        {
            dgvCodes.DataSource = null;
            dgvCodes.DataSource = _currentResults;
            ClearGridSelection();
        }
        finally
        {
            _suppressSelectionChange = false;
        }

        // Actualizar estadísticas
        var found = _currentResults.Count(r => r.Found);
        var notFound = _currentResults.Count - found;
        UpdateStatsCards(_currentResults.Count, found, notFound);

        MessageBox.Show($"Se reemplazaron {selectedCodeCells.Count} código(s) con '{selectedReplacement}'.",
            "Códigos Reemplazados", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task CopySelectedCellsToClipboardAsync(bool horizontal)
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
                    // Solo en horizontal: si es la columna de código, quitar el prefijo (P, U, etc.)
                    if (horizontal && cell.OwningColumn.Name == "colCode" && value.Length > 1 && char.IsLetter(value[0]))
                    {
                        value = value.Substring(1);
                    }
                    values.Add(value);
                }
            }

            string outputText;
            if (horizontal)
            {
                outputText = string.Join(" ", values);
            }
            else
            {
                outputText = string.Join(Environment.NewLine, values);
            }

            if (!string.IsNullOrEmpty(outputText))
            {
                if (!await TrySetClipboardTextAsync(outputText))
                {
                    MessageBox.Show(
                        "No se pudo acceder al portapapeles en este momento. Cierra aplicaciones que lo estén usando e intenta de nuevo.",
                        "Portapapeles ocupado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al copiar: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CopyAllDataToClipboardAsync()
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
                if (!await TrySetClipboardDataObjectAsync(dataObj))
                {
                    MessageBox.Show(
                        "No se pudo acceder al portapapeles en este momento. Cierra aplicaciones que lo estén usando e intenta de nuevo.",
                        "Portapapeles ocupado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
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

    private Task<bool> TrySetClipboardTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Task.FromResult(false);
        }

        return TrySetClipboardWithLockAsync(() => Clipboard.SetDataObject(text, true, 3, 40));
    }

    private Task<bool> TrySetClipboardDataObjectAsync(DataObject dataObj)
    {
        return TrySetClipboardWithLockAsync(() => Clipboard.SetDataObject(dataObj, true, 3, 40));
    }

    private async Task<bool> TrySetClipboardWithLockAsync(Action action)
    {
        await _clipboardSemaphore.WaitAsync();
        try
        {
            return await TryClipboardOperationAsync(action);
        }
        finally
        {
            _clipboardSemaphore.Release();
        }
    }

    private static async Task<bool> TryClipboardOperationAsync(Action action)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                action();
                return true;
            }
            catch (ExternalException)
            {
                if (attempt >= maxAttempts)
                {
                    return false;
                }

                await Task.Delay(GetClipboardRetryDelay(attempt));
            }
        }

        return false;
    }

    private static int GetClipboardRetryDelay(int attempt)
    {
        // Backoff corto para no congelar la UI cuando el portapapeles está temporalmente ocupado.
        return Math.Min(50 * attempt, 150);
    }

    private void DgvCodes_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        // Mantener el código tal como se pegó (no quitar prefijos)
        if (e.RowIndex >= 0 && e.RowIndex < dgvCodes.Rows.Count)
        {
            var rowResult = dgvCodes.Rows[e.RowIndex].DataBoundItem as DtcLookupResult;
            if (rowResult?.IsModuleDeleted == true && e.CellStyle != null)
            {
                e.CellStyle.BackColor = ColorTranslator.FromHtml("#2E8B57");
                e.CellStyle.SelectionBackColor = ColorTranslator.FromHtml("#1F5F3E");
                e.CellStyle.ForeColor = Color.White;
                e.CellStyle.SelectionForeColor = Color.White;
            }
        }
        
        if (dgvCodes.Columns[e.ColumnIndex].Name == "colStatus" && e.Value != null)
        {
            var found = (bool)e.Value;
            e.Value = found ? "✓ Encontrado" : "⚠ No encontrado";
            
            if (!found && e.CellStyle != null)
            {
                e.CellStyle.ForeColor = ColorTranslator.FromHtml("#D9534F");
                if (e.CellStyle.Font != null)
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
            }
        }
        
        // Resaltar descripción vacía
        if (dgvCodes.Columns[e.ColumnIndex].Name == "colDescription" && 
            (e.Value == null || string.IsNullOrWhiteSpace(e.Value.ToString())))
        {
            e.Value = "--- Sin descripción ---";
            if (e.CellStyle != null)
            {
                e.CellStyle.ForeColor = ColorTranslator.FromHtml("#B0B7BE");
                if (e.CellStyle.Font != null)
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Italic);
            }
        }

        if (dgvCodes.Columns[e.ColumnIndex].Name == "colModule")
        {
            var moduleValue = e.Value?.ToString();
            var normalized = NormalizeModuleForDisplay(moduleValue);
            e.Value = normalized;

            if (normalized == "-" && e.CellStyle != null)
            {
                e.CellStyle.ForeColor = ColorTranslator.FromHtml("#B0B7BE");
            }
        }

    }

    private static string NormalizeModuleForDisplay(string? moduleValue)
    {
        if (string.IsNullOrWhiteSpace(moduleValue))
        {
            return "-";
        }

        var cleaned = new string(moduleValue
            .Where(ch => char.IsLetterOrDigit(ch) || ch == ' ' || ch == '-' || ch == '_')
            .ToArray())
            .Trim();

        if (string.IsNullOrWhiteSpace(cleaned) || !cleaned.Any(char.IsLetterOrDigit))
        {
            return "-";
        }

        return cleaned.ToUpperInvariant();
    }

    private async void BtnParse_Click(object? sender, EventArgs e)
    {
        var inputToParse = StripInputLineNumbers(txtInput.Text);

        if (string.IsNullOrWhiteSpace(inputToParse))
        {
            MessageBox.Show("Por favor, pega códigos DTC en el área de texto.", 
                "Entrada vacía", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Mantener el área de pegado sin numeración visual.
        if (!string.Equals(txtInput.Text, inputToParse, StringComparison.Ordinal))
        {
            txtInput.Text = inputToParse;
        }

        try
        {
            Cursor = Cursors.WaitCursor;
            btnParse.Enabled = false;

            string currentObdType = _rdoObd2.Checked ? "OBD-II" : "OBD-I";

            // Parsear códigos con información de categoría
            var parsedCodes = ParseCodesWithCategory(inputToParse, currentObdType);
            
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
                if (currentObdType == "OBD-I")
                {
                    allCodesToSearch.Add(parsed.OriginalCode);
                }
                else if (parsed.WasCOrD)
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
            var rawDbCodes = await _repository.GetByCodesAsync(allCodesToSearch);
            // Aislar estrictamente a los códigos del tipo activo para evitar descripciones cruzadas
            var dbCodes = rawDbCodes.Where(c => c.ObdType == currentObdType).ToList();

            // Crear diccionario de resultados encontrados
            var dbCodesDict = dbCodes.ToDictionary(c => c.Code, c => c);

            // Crear resultados solo para la categoría correspondiente
            _currentResults = new List<DtcLookupResult>();
            _manualSelectionSnapshots.Clear();
            _moduleToggleSnapshots.Clear();
            
            foreach (var parsed in parsedCodes)
            {
                // Determinar la categoría y código de búsqueda según el código original
                string prefix = "";
                string searchCode = parsed.OriginalCode;
                
                if (currentObdType == "OBD-II")
                {
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
                }
                else
                {
                    // OBD-I usa el nombre real siempre para la categoría por defecto (Hex)
                    prefix = "Hex";
                }
                
                // Buscar en múltiples variantes asegurando que nada de OBD-II cruce
                DtcCode? foundCode = null;
                if (dbCodesDict.ContainsKey(parsed.OriginalCode))
                {
                    foundCode = dbCodesDict[parsed.OriginalCode];
                }
                else if (currentObdType == "OBD-II" && dbCodesDict.ContainsKey(searchCode))
                {
                    foundCode = dbCodesDict[searchCode];
                }
                else if (currentObdType == "OBD-II" && dbCodesDict.ContainsKey(parsed.ConvertedCode))
                {
                    foundCode = dbCodesDict[parsed.ConvertedCode];
                }
                
                var found = foundCode != null;
                
                _currentResults.Add(new DtcLookupResult
                {
                    OriginalCode = parsed.OriginalCode,
                    Code = parsed.OriginalCode, // Mantener formato original (C301, P0420, etc.)
                    CodeAlt = parsed.OriginalCode, // Inicializar con el mismo código
                    Found = found,
                    Description = found ? foundCode!.Description : null,
                    Category = found ? foundCode!.Category : GetCategoryFromPrefix(prefix),
                    Source = found ? foundCode!.Source : null,
                    Notes = found ? foundCode!.Notes : null,
                    FilterTag = found ? foundCode!.FilterTag : null,
                    Module = found ? GetDisplayModule(foundCode!) : null,
                    ObdType = found ? foundCode!.ObdType : currentObdType,
                    DtcId = found ? foundCode!.Id : null
                });
            }

            // Clasificar módulo de cada resultado (hybrid: exacto + keywords)
            _classifier?.ClassifyAll(_currentResults);
            ApplyModuleDisplayNames();

            // Mostrar resultados sin disparar auto-borrado por selección inicial del grid
            _suppressAutoDelete = true;
            _suppressSelectionChange = true;
            try
            {
                dgvCodes.DataSource = null;
                dgvCodes.DataSource = _currentResults;
                ClearGridSelection();
            }
            finally
            {
                _suppressSelectionChange = false;
                _suppressAutoDelete = false;
            }

            BeginInvoke((MethodInvoker)(() => ClearGridSelection()));
            
            // Actualizar tarjetas de estadísticas
            var foundCount = _currentResults.Count(r => r.Found);
            var notFound = _currentResults.Count - foundCount;
            UpdateStatsCards(_currentResults.Count, foundCount, notFound);
            ShowEmptyState(_currentResults.Count == 0);
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

    private List<ParsedCodeInfo> ParseCodesWithCategory(string input, string obdType)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<ParsedCodeInfo>();

        var codes = new List<ParsedCodeInfo>();

        if (obdType == "OBD-I")
        {
            // OBD-I: extrae números o combinaciones alfanuméricas de hasta 5 caracteres (ej. 10, 11, 2013)
            var obd1Pattern = new System.Text.RegularExpressions.Regex(@"\b[0-9A-Z]{1,5}\b", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
            var matches = obd1Pattern.Matches(input);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var originalCode = match.Value.ToUpperInvariant();
                codes.Add(new ParsedCodeInfo
                {
                    OriginalCode = originalCode,
                    ConvertedCode = originalCode,
                    WasCOrD = false
                });
            }
            return codes;
        }

        // Patrón OBD-II regular
        var hexPattern = new System.Text.RegularExpressions.Regex(@"\b(?:[PU][0-9A-F]{4}|[CD][0-9A-F]{3}|[0-9A-F]{4})\b", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        var hexMatches = hexPattern.Matches(input);

        foreach (System.Text.RegularExpressions.Match match in hexMatches)
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

    private static string StripInputLineNumbers(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var lines = input
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(line => System.Text.RegularExpressions.Regex.Replace(
                line,
                @"^\s*(?:L\s*)?\d+\s*[:.)-]\s*",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            .ToList();

        return string.Join(Environment.NewLine, lines);
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

    private string? GetDisplayModule(DtcCode dtcCode)
    {
        if (!string.IsNullOrWhiteSpace(dtcCode.Module))
            return dtcCode.Module!.ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(dtcCode.FilterTag))
            return dtcCode.FilterTag!.ToUpperInvariant();

        return null;
    }

    private void ApplyModuleDisplayNames()
    {
        if (_currentResults.Count == 0 || _moduleFilters.Count == 0)
        {
            return;
        }

        var filterDisplayByName = _moduleFilters
            .ToDictionary(f => f.Name, f => f.DisplayName, StringComparer.OrdinalIgnoreCase);

        foreach (var result in _currentResults)
        {
            if (string.IsNullOrWhiteSpace(result.FilterTag))
            {
                continue;
            }

            if (filterDisplayByName.TryGetValue(result.FilterTag, out var displayName))
            {
                result.Module = displayName;
            }
        }
    }

    private Task ExecuteSearchAsync()
    {
        var term = txtSearch.Text.Trim();

        if (string.IsNullOrWhiteSpace(term))
        {
            ClearSearch();
            return Task.CompletedTask;
        }

        if (_currentResults == null || _currentResults.Count == 0)
        {
            lblSearchMode.Text = "Sin resultados cargados";
            return Task.CompletedTask;
        }

        var matchingRows = _currentResults
            .Select((r, i) => new { Result = r, Index = i })
            .Where(x => (x.Result.Code ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase)
                     || (x.Result.CodeAlt ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase)
                     || (x.Result.Description ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Index)
            .ToList();

        _suppressSelectionChange = true;
        try
        {
            foreach (var rowIndex in matchingRows)
            {
                if (rowIndex >= 0 && rowIndex < dgvCodes.Rows.Count)
                {
                    var cell = dgvCodes.Rows[rowIndex].Cells["colCode"];
                    if (cell != null)
                    {
                        cell.Selected = true;
                    }
                }
            }

            if (matchingRows.Count > 0)
            {
                // Llevar la primera coincidencia a la vista sin romper la selección acumulada.
                dgvCodes.FirstDisplayedScrollingRowIndex = Math.Max(0, matchingRows[0]);
                dgvCodes.CurrentCell = null;
                lblSearchMode.Text = $"{matchingRows.Count} coincidencia(s)";
            }
            else
            {
                lblSearchMode.Text = "Sin coincidencias";
            }
        }
        finally
        {
            _suppressSelectionChange = false;
        }

        return Task.CompletedTask;
    }

    private void ClearSearch()
    {
        txtSearch.Clear();
        lblSearchMode.Text = string.Empty;
        ClearGridSelection();
        txtSearch.Focus();
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        txtInput.Clear();
        dgvCodes.DataSource = null;
        _currentResults.Clear();
        _manualSelectionSnapshots.Clear();
        _moduleToggleSnapshots.Clear();
        UpdateStatsCards(0, 0, 0);
        ShowEmptyState(true);
        txtInput.Focus();
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        // Si hay un código seleccionado sin descripción, pre-llenarlo
        string? prefilledCode = null;
        
        // Obtener la fila desde la celda seleccionada
        DataGridViewRow? selectedRow = null;
        
        if (dgvCodes.SelectedCells.Count > 0)
        {
            var selectedCell = dgvCodes.SelectedCells[0];
            selectedRow = dgvCodes.Rows[selectedCell.RowIndex];
        }
        else if (dgvCodes.CurrentRow != null)
        {
            selectedRow = dgvCodes.CurrentRow;
        }
        
        if (selectedRow != null)
        {
            var selectedResult = selectedRow.DataBoundItem as DtcLookupResult;
            if (selectedResult != null && !selectedResult.Found)
            {
                prefilledCode = selectedResult.Code;
            }
        }

        string currentObdType = _rdoObd2.Checked ? "OBD-II" : "OBD-I";
        var addForm = new AddEditCodeForm(currentObdType, prefilledCode);
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
        // Obtener la fila desde la celda seleccionada
        DataGridViewRow? selectedRow = null;
        
        if (dgvCodes.SelectedCells.Count > 0)
        {
            // Obtener la primera celda seleccionada
            var selectedCell = dgvCodes.SelectedCells[0];
            selectedRow = dgvCodes.Rows[selectedCell.RowIndex];
        }
        else if (dgvCodes.CurrentRow != null)
        {
            selectedRow = dgvCodes.CurrentRow;
        }

        if (selectedRow == null)
        {
            MessageBox.Show("Por favor, selecciona un código para editar.", 
                "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedResult = selectedRow.DataBoundItem as DtcLookupResult;
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
            _dbTotalCodes = (int)count;
            this.Text = $"DtcDesk - Diccionario de Códigos DTC ({count:N0} códigos en BD)";

            // Actualizar barra de estado inferior
            if (statusLabelCount != null)
                statusLabelCount.Text = $"  🗄  {count:N0} códigos disponibles";
            if (statusLabelInfo != null)
            {
                var today = DateTime.Now.ToString("dd/MM/yyyy");
                statusLabelInfo.Text = $"ℹ  Lista actualizada: {today} — Base de datos ecu tuning";
            }
        }
        catch
        {
            this.Text = "DtcDesk - Diccionario de Códigos DTC";
        }
    }

    private async void DgvCodes_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

        // Solo permitir editar/añadir con doble clic en la columna DESCRIPCIÓN.
        if (!string.Equals(dgvCodes.Columns[e.ColumnIndex].Name, "colDescription", StringComparison.Ordinal))
        {
            return;
        }
        
        // Doble clic abre editar/añadir usando la fila clickeada,
        // sin depender de la selección actual de celdas.
        var selectedResult = dgvCodes.Rows[e.RowIndex].DataBoundItem as DtcLookupResult;
        if (selectedResult == null) return;

        if (selectedResult.Found)
        {
            try
            {
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
                    BtnParse_Click(sender, EventArgs.Empty); // Re-parsear para actualizar
                    LoadStatistics();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el código: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            string currentObdType = _rdoObd2.Checked ? "OBD-II" : "OBD-I";
            var addForm = new AddEditCodeForm(currentObdType, selectedResult.Code);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                if (addForm.DtcCode != null && _currentResults.Any(r => r.Code == addForm.DtcCode.Code))
                {
                    BtnParse_Click(sender, EventArgs.Empty); // Re-parsear para actualizar
                }

                LoadStatistics();
            }
        }
    }

    private void DgvCodes_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (_suppressSelectionChange || _autoDeletingSelection || _suppressAutoDelete)
        {
            return;
        }

        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        var column = dgvCodes.Columns[e.ColumnIndex];
        if (!IsSelectableCodeColumn(column))
        {
            return;
        }
    }

    private void DgvCodes_SelectionChanged(object? sender, EventArgs e)
    {
        if (_suppressSelectionChange || _autoDeletingSelection || _suppressAutoDelete)
        {
            return;
        }

        _suppressSelectionChange = true;
        try
        {
            var cellsToDeselect = dgvCodes.SelectedCells
                .Cast<DataGridViewCell>()
                .Where(cell => !IsSelectableCodeColumn(cell.OwningColumn))
                .ToList();

            foreach (var cell in cellsToDeselect)
            {
                cell.Selected = false;
            }

            if (dgvCodes.CurrentCell != null && !IsSelectableCodeColumn(dgvCodes.CurrentCell.OwningColumn))
            {
                dgvCodes.CurrentCell = null;
            }
        }
        finally
        {
            _suppressSelectionChange = false;
        }

        var selectedRows = dgvCodes.SelectedCells
            .Cast<DataGridViewCell>()
            .Where(cell => IsSelectableCodeColumn(cell.OwningColumn))
            .Select(cell => cell.RowIndex)
            .Where(rowIndex => rowIndex >= 0 && rowIndex < _currentResults.Count)
            .Distinct()
            .ToList();

        ApplyManualSelectionState(selectedRows);

        // Habilitar/deshabilitar botones según selección
        var hasSelection = selectedRows.Count > 0;

        var isFound = false;

        if (hasSelection)
        {
            var firstRowIndex = selectedRows[0];
            if (firstRowIndex >= 0 && firstRowIndex < _currentResults.Count)
            {
                isFound = _currentResults[firstRowIndex].Found;
            }
        }

        // btnEdit.Enabled = hasSelection && isFound; // Comentado para evitar que Windows lo ponga en gris feo. Ya está validado a nivel del evento en BtnEdit_Click!
    }

    private void ApplyManualSelectionState(List<int> selectedRows)
    {
        if (_currentResults.Count == 0)
        {
            return;
        }

        var selectedSet = selectedRows.ToHashSet();

        _autoDeletingSelection = true;
        try
        {
            // Restaurar filas que ya no están seleccionadas (segundo clic = toggle off).
            var rowsToRestore = _manualSelectionSnapshots.Keys
                .Where(rowIndex => !selectedSet.Contains(rowIndex))
                .ToList();

            foreach (var rowIndex in rowsToRestore)
            {
                if (rowIndex < 0 || rowIndex >= _currentResults.Count)
                {
                    _manualSelectionSnapshots.Remove(rowIndex);
                    continue;
                }

                var snapshot = _manualSelectionSnapshots[rowIndex];
                var result = _currentResults[rowIndex];

                result.Code = snapshot.Code;
                result.CodeAlt = snapshot.CodeAlt;
                result.Found = snapshot.Found;
                result.Description = snapshot.Description;
                result.Category = snapshot.Category;
                result.Source = snapshot.Source;
                result.Notes = snapshot.Notes;
                result.FilterTag = snapshot.FilterTag;
                result.Module = snapshot.Module;
                result.IsModuleDeleted = snapshot.IsModuleDeleted;

                _manualSelectionSnapshots.Remove(rowIndex);
            }

            // Permitir restaurar filas borradas por módulo al deseleccionarlas manualmente.
            var moduleRowsToRestore = _moduleToggleSnapshots.Keys
                .Where(rowIndex => !selectedSet.Contains(rowIndex))
                .ToList();

            foreach (var rowIndex in moduleRowsToRestore)
            {
                if (rowIndex < 0 || rowIndex >= _currentResults.Count)
                {
                    _moduleToggleSnapshots.Remove(rowIndex);
                    continue;
                }

                var snapshot = _moduleToggleSnapshots[rowIndex].Snapshot;
                var result = _currentResults[rowIndex];

                result.Code = snapshot.Code;
                result.CodeAlt = snapshot.CodeAlt;
                result.Found = snapshot.Found;
                result.Description = snapshot.Description;
                result.Category = snapshot.Category;
                result.Source = snapshot.Source;
                result.Notes = snapshot.Notes;
                result.FilterTag = snapshot.FilterTag;
                result.Module = snapshot.Module;
                result.IsModuleDeleted = snapshot.IsModuleDeleted;

                _moduleToggleSnapshots.Remove(rowIndex);
            }

            // Aplicar conversión en filas seleccionadas que aún no tenían snapshot.
            foreach (var rowIndex in selectedSet)
            {
                if (_manualSelectionSnapshots.ContainsKey(rowIndex))
                {
                    continue;
                }

                if (_moduleToggleSnapshots.ContainsKey(rowIndex))
                {
                    continue;
                }

                var result = _currentResults[rowIndex];

                // Si ya fue borrado por módulo, mantener ese estado permanente.
                if (result.IsModuleDeleted)
                {
                    continue;
                }

                _manualSelectionSnapshots[rowIndex] = new ManualSelectionSnapshot
                {
                    Code = result.Code,
                    CodeAlt = result.CodeAlt,
                    Found = result.Found,
                    Description = result.Description,
                    Category = result.Category,
                    Source = result.Source,
                    Notes = result.Notes,
                    FilterTag = result.FilterTag,
                    Module = result.Module,
                    IsModuleDeleted = result.IsModuleDeleted
                };

                result.Code = "0000";
                result.CodeAlt = "FFFF";
                result.Found = false;
                result.Category = "Hex";
                result.Source = null;
                result.Notes = null;
                result.FilterTag = null;
                result.Module = null;
                result.IsModuleDeleted = true;
            }

            dgvCodes.Refresh();

            var found = _currentResults.Count(r => r.Found);
            var notFound = _currentResults.Count - found;
            UpdateStatsCards(_currentResults.Count, found, notFound);
        }
        finally
        {
            _autoDeletingSelection = false;
        }
    }

    private void ReapplyManualSelectionToGrid()
    {
        if (_manualSelectionSnapshots.Count == 0 || dgvCodes.Rows.Count == 0)
        {
            return;
        }

        var invalidKeys = _manualSelectionSnapshots.Keys
            .Where(rowIndex => rowIndex < 0 || rowIndex >= _currentResults.Count || rowIndex >= dgvCodes.Rows.Count)
            .ToList();

        foreach (var rowIndex in invalidKeys)
        {
            _manualSelectionSnapshots.Remove(rowIndex);
        }

        foreach (var rowIndex in _manualSelectionSnapshots.Keys.OrderBy(i => i))
        {
            var cell = dgvCodes.Rows[rowIndex].Cells["colCode"];
            if (cell != null)
            {
                cell.Selected = true;
            }
        }

        dgvCodes.CurrentCell = null;
    }

    private void ReapplyModuleSelectionToGrid()
    {
        if (_moduleToggleSnapshots.Count == 0 || dgvCodes.Rows.Count == 0)
        {
            return;
        }

        var invalidKeys = _moduleToggleSnapshots.Keys
            .Where(rowIndex => rowIndex < 0 || rowIndex >= _currentResults.Count || rowIndex >= dgvCodes.Rows.Count)
            .ToList();

        foreach (var rowIndex in invalidKeys)
        {
            _moduleToggleSnapshots.Remove(rowIndex);
        }

        foreach (var rowIndex in _moduleToggleSnapshots.Keys.OrderBy(i => i))
        {
            var cell = dgvCodes.Rows[rowIndex].Cells["colCode"];
            if (cell != null)
            {
                cell.Selected = true;
            }
        }

        dgvCodes.CurrentCell = null;
    }

    private void DeleteByModule(DtcModuleFilter module)
    {
        var moduleKey = module.Name;
        var moduleLabel = module.DisplayName;

        if (_currentResults == null || _currentResults.Count == 0)
        {
            MessageBox.Show(
                $"No hay códigos cargados.\nPrimero procesa algunos códigos DTC.",
                $"Sin datos — {moduleLabel}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        // Toggle: si ya se aplicó este módulo antes, restaurar esas filas.
        var rowsToRestore = _moduleToggleSnapshots
            .Where(kvp => string.Equals(kvp.Value.ModuleKey, moduleKey, StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Key)
            .Where(index => index >= 0 && index < _currentResults.Count)
            .Distinct()
            .ToList();

        if (rowsToRestore.Count > 0)
        {
            foreach (var rowIndex in rowsToRestore)
            {
                var snapshot = _moduleToggleSnapshots[rowIndex].Snapshot;
                var result = _currentResults[rowIndex];

                result.Code = snapshot.Code;
                result.CodeAlt = snapshot.CodeAlt;
                result.Found = snapshot.Found;
                result.Description = snapshot.Description;
                result.Category = snapshot.Category;
                result.Source = snapshot.Source;
                result.Notes = snapshot.Notes;
                result.FilterTag = snapshot.FilterTag;
                result.Module = snapshot.Module;
                result.IsModuleDeleted = snapshot.IsModuleDeleted;

                _moduleToggleSnapshots.Remove(rowIndex);
            }

            _suppressAutoDelete = true;
            _suppressSelectionChange = true;
            try
            {
                _preserveSelectionOnNextDataBinding = true;
                dgvCodes.DataSource = null;
                dgvCodes.DataSource = _currentResults;
                ReapplyManualSelectionToGrid();
                ReapplyModuleSelectionToGrid();
            }
            finally
            {
                _suppressSelectionChange = false;
                _suppressAutoDelete = false;
            }

            var restoredFound = _currentResults.Count(r => r.Found);
            var restoredNotFound = _currentResults.Count - restoredFound;
            UpdateStatsCards(_currentResults.Count, restoredFound, restoredNotFound);
            return;
        }

        // Buscar los códigos que corresponden al módulo
        var toReplace = _currentResults
            .Where(r => string.Equals(r.FilterTag, moduleKey, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(r.Module, moduleLabel, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (toReplace.Count == 0)
        {
            MessageBox.Show(
                $"No se encontraron códigos clasificados como [{moduleLabel}] en los resultados actuales.\n\n" +
                $"Verifica que los códigos tengan descripción o sean los códigos exactos del módulo.",
                $"Sin coincidencias — {moduleLabel}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        // Pedir confirmación mostrando los códigos afectados
        var codesPreview = string.Join(", ", toReplace.Take(10).Select(r => r.Code));
        if (toReplace.Count > 10) codesPreview += $" ... y {toReplace.Count - 10} más";

        var confirm = MessageBox.Show(
            $"Se encontraron {toReplace.Count} código(s) del módulo [{moduleLabel}]:\n{codesPreview}\n\n" +
            $"¿Reemplazar todos con '0000' / 'FFFF'?",
            $"Borrar módulo {moduleLabel}",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
            return;

        // Reemplazar
        for (var i = 0; i < _currentResults.Count; i++)
        {
            var result = _currentResults[i];
            var isTarget = string.Equals(result.FilterTag, moduleKey, StringComparison.OrdinalIgnoreCase)
                || string.Equals(result.Module, moduleLabel, StringComparison.OrdinalIgnoreCase);

            if (!isTarget)
            {
                continue;
            }

            // Si esta fila tenía snapshot manual, eliminarlo para evitar restauraciones inconsistentes.
            _manualSelectionSnapshots.Remove(i);

            _moduleToggleSnapshots[i] = (moduleKey, new ManualSelectionSnapshot
            {
                Code = result.Code,
                CodeAlt = result.CodeAlt,
                Found = result.Found,
                Description = result.Description,
                Category = result.Category,
                Source = result.Source,
                Notes = result.Notes,
                FilterTag = result.FilterTag,
                Module = result.Module,
                IsModuleDeleted = result.IsModuleDeleted
            });

            result.Code        = "0000";
            result.CodeAlt     = "FFFF";
            result.Found       = false;
            result.Category    = "Hex";
            result.Source      = null;
            result.Notes       = null;
            result.FilterTag   = null;   // Limpiar tag tras borrar
            result.Module      = null;
            result.IsModuleDeleted = true;
        }

        // Refrescar grid
        _suppressAutoDelete = true;
        _suppressSelectionChange = true;
        try
        {
            _preserveSelectionOnNextDataBinding = true;
            dgvCodes.DataSource = null;
            dgvCodes.DataSource = _currentResults;
            ReapplyManualSelectionToGrid();
            ReapplyModuleSelectionToGrid();
        }
        finally
        {
            _suppressSelectionChange = false;
            _suppressAutoDelete = false;
        }

        // Actualizar estadísticas
        var found    = _currentResults.Count(r => r.Found);
        var notFound = _currentResults.Count - found;
        UpdateStatsCards(_currentResults.Count, found, notFound);
    }
}

// Clase para personalizar los colores del menú contextual
internal class CustomMenuColorTable : ProfessionalColorTable
{
    public override Color MenuItemSelected => ColorTranslator.FromHtml("#F8B41C"); // Amarillo marca
    public override Color MenuItemSelectedGradientBegin => ColorTranslator.FromHtml("#153C59");
    public override Color MenuItemSelectedGradientEnd => ColorTranslator.FromHtml("#153C59");
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
    private bool _isSelecting;
    private bool _isDeselectingDrag;
    private readonly HashSet<(int Row, int Column)> _processedCells = new();
    private int _lastDragRow = -1;
    private int _lastDragColumn = -1;

    protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e)
    {
        // Encabezado sin comportamiento: no ordenar ni alterar selección.
    }

    private void ApplyDragStateToCell(int rowIndex, int columnIndex)
    {
        var cell = this[columnIndex, rowIndex];
        cell.Selected = !_isDeselectingDrag;
    }

    private void ApplyDragStateBetweenCells(int fromRow, int fromColumn, int toRow, int toColumn)
    {
        var startRow = Math.Min(fromRow, toRow);
        var endRow = Math.Max(fromRow, toRow);
        var startColumn = Math.Min(fromColumn, toColumn);
        var endColumn = Math.Max(fromColumn, toColumn);

        for (var row = startRow; row <= endRow; row++)
        {
            for (var column = startColumn; column <= endColumn; column++)
            {
                var cellKey = (row, column);
                if (_processedCells.Contains(cellKey))
                {
                    continue;
                }

                ApplyDragStateToCell(row, column);
                _processedCells.Add(cellKey);
            }
        }
    }

    protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            // Ignorar por completo clics en encabezados.
            return;
        }

        // Solo procesar clicks izquierdos en celdas válidas
        if (e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            _isSelecting = true;
            _processedCells.Clear();

            var clickedCell = this[e.ColumnIndex, e.RowIndex];
            _isDeselectingDrag = clickedCell.Selected;

            ApplyDragStateToCell(e.RowIndex, e.ColumnIndex);
            _processedCells.Add((e.RowIndex, e.ColumnIndex));
            _lastDragRow = e.RowIndex;
            _lastDragColumn = e.ColumnIndex;

            // Evitar que el comportamiento base limpie o re-seleccione celdas de forma automática.
            return;
        }

        base.OnCellMouseDown(e);
    }

    protected override void OnCellMouseMove(DataGridViewCellMouseEventArgs e)
    {
        // Arrastre continuo para seleccionar o deseleccionar según el estado inicial.
        if (_isSelecting && e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            if (_lastDragRow < 0 || _lastDragColumn < 0)
            {
                _lastDragRow = e.RowIndex;
                _lastDragColumn = e.ColumnIndex;
            }

            if (_lastDragRow == e.RowIndex && _lastDragColumn == e.ColumnIndex)
            {
                return;
            }

            // Rellenar rango entre la celda anterior y la actual evita saltos al arrastrar rápido.
            ApplyDragStateBetweenCells(_lastDragRow, _lastDragColumn, e.RowIndex, e.ColumnIndex);
            _lastDragRow = e.RowIndex;
            _lastDragColumn = e.ColumnIndex;
            return;
        }

        base.OnCellMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isSelecting = false;
        _isDeselectingDrag = false;
        _processedCells.Clear();
        _lastDragRow = -1;
        _lastDragColumn = -1;
        base.OnMouseUp(e);
    }
}

