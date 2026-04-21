namespace DtcDesk.WinForms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            menuStrip = new MenuStrip();
            menuArchivo = new ToolStripMenuItem();
            menuImportar = new ToolStripMenuItem();
            menuExportar = new ToolStripMenuItem();
            menuSeparador1 = new ToolStripSeparator();
            menuLimpiarDB = new ToolStripMenuItem();
            menuSeparador2 = new ToolStripSeparator();
            menuSalir = new ToolStripMenuItem();
            menuHerramientas = new ToolStripMenuItem();
            menuEstadisticas = new ToolStripMenuItem();
            panelTop = new Panel();
            picLogoRight = new PictureBox();
            picLogo = new PictureBox();
            lblSubtitle = new Label();
            lblTitle = new Label();
            txtSearch = new TextBox();
            btnSearch = new Button();
            btnSearchClear = new Button();
            lblSearchMode = new Label();
            panelStatsBar = new Panel();
            panelStatTotal = new Panel();
            lblStatTotalIcon = new Label();
            lblStatTotalValue = new Label();
            lblStatTotalLabel = new Label();
            panelStatFound = new Panel();
            lblStatFoundIcon = new Label();
            lblStatFoundValue = new Label();
            lblStatFoundLabel = new Label();
            panelStatNotFound = new Panel();
            lblStatNotFoundIcon = new Label();
            lblStatNotFoundValue = new Label();
            lblStatNotFoundLabel = new Label();
            panelLeft = new Panel();
            lblLineCount = new Label();
            btnClear = new Button();
            btnParse = new Button();
            txtInput = new TextBox();
            lblInput = new Label();
            panelRight = new Panel();
            dgvCodes = new CumulativeSelectionDataGridView();
            panelGridContainer = new Panel();
            panelEmptyState = new Panel();
            lblEmptyStateIcon = new Label();
            lblEmptyStateTitle = new Label();
            lblEmptyStateDesc = new Label();
            panelColumnCopy = new Panel();
            btnClearSelectionTop = new Button();
            btnCopyCodeAltColumn = new Button();
            btnCopyCodeColumn = new Button();
            panelButtons = new Panel();
            btnZoomIn = new Button();
            btnZoomReset = new Button();
            btnZoomOut = new Button();
            btnEdit = new Button();
            btnAdd = new Button();
            lblResults = new Label();
            panelFilterSide = new Panel();
            lblFilterTitle = new Label();
            btnFilterVNT = new Button();
            btnFilterDPF = new Button();
            btnFilterEGR = new Button();
            btnFilterNOX = new Button();
            btnFilterSCR = new Button();
            btnFilterMAF = new Button();
            btnFilterTVA = new Button();
            statusStrip = new StatusStrip();
            statusLabelInfo = new ToolStripStatusLabel();
            statusLabelSep1 = new ToolStripStatusLabel();
            statusLabelMode = new ToolStripStatusLabel();
            statusLabelSep2 = new ToolStripStatusLabel();
            statusLabelCount = new ToolStripStatusLabel();

            menuStrip.SuspendLayout();
            panelTop.SuspendLayout();
            panelStatsBar.SuspendLayout();
            panelStatTotal.SuspendLayout();
            panelStatFound.SuspendLayout();
            panelStatNotFound.SuspendLayout();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            panelGridContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCodes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picLogoRight).BeginInit();
            panelEmptyState.SuspendLayout();
            panelButtons.SuspendLayout();
            panelFilterSide.SuspendLayout();
            SuspendLayout();

            // ──────────────────────────────────────────────────────────────
            // menuStrip
            // ──────────────────────────────────────────────────────────────
            menuStrip.Items.AddRange(new ToolStripItem[] { menuArchivo, menuHerramientas });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1400, 24);
            menuStrip.TabIndex = 0;
            menuStrip.Padding = new Padding(4, 2, 0, 2);

            menuArchivo.DropDownItems.AddRange(new ToolStripItem[] { menuImportar, menuExportar, menuSeparador1, menuLimpiarDB, menuSeparador2, menuSalir });
            menuArchivo.Name = "menuArchivo";
            menuArchivo.Size = new Size(70, 20);
            menuArchivo.Text = "📁  Archivo";

            menuImportar.Name = "menuImportar";
            menuImportar.Size = new Size(200, 22);
            menuImportar.Text = "📥  Importar CSV...";

            menuExportar.Name = "menuExportar";
            menuExportar.Size = new Size(200, 22);
            menuExportar.Text = "📤  Exportar...";

            menuSeparador1.Name = "menuSeparador1";
            menuSeparador1.Size = new Size(197, 6);

            menuLimpiarDB.Name = "menuLimpiarDB";
            menuLimpiarDB.Size = new Size(200, 22);
            menuLimpiarDB.Text = "🗑️  Limpiar Base de Datos...";

            menuSeparador2.Name = "menuSeparador2";
            menuSeparador2.Size = new Size(197, 6);

            menuSalir.Name = "menuSalir";
            menuSalir.Size = new Size(200, 22);
            menuSalir.Text = "✖  Salir";

            menuHerramientas.DropDownItems.AddRange(new ToolStripItem[] { menuEstadisticas });
            menuHerramientas.Name = "menuHerramientas";
            menuHerramientas.Size = new Size(100, 20);
            menuHerramientas.Text = "🔧  Herramientas";

            menuEstadisticas.Name = "menuEstadisticas";
            menuEstadisticas.Size = new Size(200, 22);
            menuEstadisticas.Text = "📊  Ver Estadísticas DB";

            // ──────────────────────────────────────────────────────────────
            // panelTop  (altura 110: logo + título + búsqueda)
            // ──────────────────────────────────────────────────────────────
            panelTop.Controls.Add(picLogo);
            panelTop.Controls.Add(lblTitle);
            panelTop.Controls.Add(lblSubtitle);
            panelTop.Controls.Add(txtSearch);
            panelTop.Controls.Add(btnSearch);
            panelTop.Controls.Add(btnSearchClear);
            panelTop.Controls.Add(lblSearchMode);
            panelTop.Controls.Add(picLogoRight);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 24);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1400, 100);
            panelTop.TabIndex = 1;

            // picLogo — izquierda (logo AUTO TUNER actual)
            picLogo.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            picLogo.Location = new Point(10, 8);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(130, 50);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 2;
            picLogo.TabStop = false;

            // lblTitle — centrado (reserva espacio para los logos laterales)
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(150, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1100, 34);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "DTC DESK";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // lblSubtitle — debajo del título centrado
            lblSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblSubtitle.Location = new Point(150, 40);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(1100, 18);
            lblSubtitle.TabIndex = 7;
            lblSubtitle.Text = "- Diccionario de Códigos DTC -   v2.1";
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;

            // picLogoRight — derecha (logo ECU Tuning)
            picLogoRight.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            picLogoRight.Location = new Point(1268, 8);
            picLogoRight.Name = "picLogoRight";
            picLogoRight.Size = new Size(120, 60);
            picLogoRight.SizeMode = PictureBoxSizeMode.Zoom;
            picLogoRight.TabIndex = 8;
            picLogoRight.TabStop = false;

            // ── Grupo de búsqueda — centrado en el panel de forma fija ──
            // Panel 1400px ancho, logos laterales ~150px c/u → zona central 420-1250
            // Grupo: txtSearch(420) + 6 + btnSearch(100) + 6 + btnSearchClear(90) = 622px
            // Inicio grupo: (1400-622)/2 = 389px  → redondeado a 395 para centrar mejor
            const int searchGroupLeft = 395;

            // txtSearch — entrada principal
            txtSearch.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtSearch.Font = new Font("Segoe UI", 10.5F);
            txtSearch.Location = new Point(searchGroupLeft, 66);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(420, 28);
            txtSearch.TabIndex = 10;
            txtSearch.PlaceholderText = "🔍  Buscar código DTC, descripción o módulo...";

            // btnSearch — botón naranja BUSCAR
            btnSearch.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSearch.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSearch.Location = new Point(searchGroupLeft + 426, 66);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(100, 28);
            btnSearch.TabIndex = 11;
            btnSearch.Text = "BUSCAR";
            btnSearch.UseVisualStyleBackColor = true;

            // btnSearchClear — botón gris LIMPIAR
            btnSearchClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSearchClear.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSearchClear.Location = new Point(searchGroupLeft + 532, 66);
            btnSearchClear.Name = "btnSearchClear";
            btnSearchClear.Size = new Size(90, 28);
            btnSearchClear.TabIndex = 12;
            btnSearchClear.Text = "LIMPIAR";
            btnSearchClear.UseVisualStyleBackColor = true;

            // lblSearchMode — indicador de coincidencias debajo del input
            lblSearchMode.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblSearchMode.AutoSize = true;
            lblSearchMode.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblSearchMode.Location = new Point(searchGroupLeft, 98);
            lblSearchMode.Name = "lblSearchMode";
            lblSearchMode.TabIndex = 13;
            lblSearchMode.Text = "";

            // ──────────────────────────────────────────────────────────────
            // panelStatsBar  (barra compacta de estadísticas)
            // ──────────────────────────────────────────────────────────────
            panelStatsBar.Controls.Add(panelStatTotal);
            panelStatsBar.Controls.Add(panelStatFound);
            panelStatsBar.Controls.Add(panelStatNotFound);
            panelStatsBar.Dock = DockStyle.Top;
            panelStatsBar.Location = new Point(0, 124);
            panelStatsBar.Name = "panelStatsBar";
            panelStatsBar.Size = new Size(1400, 48);
            panelStatsBar.Padding = new Padding(12, 6, 12, 6);
            panelStatsBar.TabIndex = 5;

            // ── Tarjeta TOTAL ──
            panelStatTotal.Controls.Add(lblStatTotalIcon);
            panelStatTotal.Controls.Add(lblStatTotalValue);
            panelStatTotal.Controls.Add(lblStatTotalLabel);
            panelStatTotal.Location = new Point(12, 6);
            panelStatTotal.Name = "panelStatTotal";
            panelStatTotal.Size = new Size(155, 36);
            panelStatTotal.TabIndex = 0;
            panelStatTotal.Padding = new Padding(6, 2, 6, 2);

            lblStatTotalIcon.AutoSize = false;
            lblStatTotalIcon.Location = new Point(6, 2);
            lblStatTotalIcon.Name = "lblStatTotalIcon";
            lblStatTotalIcon.Size = new Size(28, 32);
            lblStatTotalIcon.Font = new Font("Segoe UI Emoji", 14F);
            lblStatTotalIcon.Text = "🛢";
            lblStatTotalIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblStatTotalIcon.TabIndex = 0;

            lblStatTotalValue.AutoSize = false;
            lblStatTotalValue.Location = new Point(38, 2);
            lblStatTotalValue.Name = "lblStatTotalValue";
            lblStatTotalValue.Size = new Size(65, 20);
            lblStatTotalValue.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblStatTotalValue.Text = "0";
            lblStatTotalValue.TextAlign = ContentAlignment.MiddleLeft;
            lblStatTotalValue.TabIndex = 1;

            lblStatTotalLabel.AutoSize = false;
            lblStatTotalLabel.Location = new Point(38, 22);
            lblStatTotalLabel.Name = "lblStatTotalLabel";
            lblStatTotalLabel.Size = new Size(95, 12);
            lblStatTotalLabel.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
            lblStatTotalLabel.Text = "TOTAL";
            lblStatTotalLabel.TextAlign = ContentAlignment.MiddleLeft;
            lblStatTotalLabel.TabIndex = 2;

            // ── Tarjeta ENCONTRADOS ──
            panelStatFound.Controls.Add(lblStatFoundIcon);
            panelStatFound.Controls.Add(lblStatFoundValue);
            panelStatFound.Controls.Add(lblStatFoundLabel);
            panelStatFound.Location = new Point(175, 6);
            panelStatFound.Name = "panelStatFound";
            panelStatFound.Size = new Size(185, 36);
            panelStatFound.TabIndex = 1;
            panelStatFound.Padding = new Padding(6, 2, 6, 2);

            lblStatFoundIcon.AutoSize = false;
            lblStatFoundIcon.Location = new Point(6, 2);
            lblStatFoundIcon.Name = "lblStatFoundIcon";
            lblStatFoundIcon.Size = new Size(28, 32);
            lblStatFoundIcon.Font = new Font("Segoe UI Emoji", 14F);
            lblStatFoundIcon.Text = "✅";
            lblStatFoundIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblStatFoundIcon.TabIndex = 0;

            lblStatFoundValue.AutoSize = false;
            lblStatFoundValue.Location = new Point(38, 2);
            lblStatFoundValue.Name = "lblStatFoundValue";
            lblStatFoundValue.Size = new Size(65, 20);
            lblStatFoundValue.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblStatFoundValue.Text = "0";
            lblStatFoundValue.TextAlign = ContentAlignment.MiddleLeft;
            lblStatFoundValue.TabIndex = 1;

            lblStatFoundLabel.AutoSize = false;
            lblStatFoundLabel.Location = new Point(38, 22);
            lblStatFoundLabel.Name = "lblStatFoundLabel";
            lblStatFoundLabel.Size = new Size(120, 12);
            lblStatFoundLabel.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
            lblStatFoundLabel.Text = "ENCONTRADOS";
            lblStatFoundLabel.TextAlign = ContentAlignment.MiddleLeft;
            lblStatFoundLabel.TabIndex = 2;

            // ── Tarjeta NO ENCONTRADOS ──
            panelStatNotFound.Controls.Add(lblStatNotFoundIcon);
            panelStatNotFound.Controls.Add(lblStatNotFoundValue);
            panelStatNotFound.Controls.Add(lblStatNotFoundLabel);
            panelStatNotFound.Location = new Point(368, 6);
            panelStatNotFound.Name = "panelStatNotFound";
            panelStatNotFound.Size = new Size(205, 36);
            panelStatNotFound.TabIndex = 2;
            panelStatNotFound.Padding = new Padding(6, 2, 6, 2);

            lblStatNotFoundIcon.AutoSize = false;
            lblStatNotFoundIcon.Location = new Point(6, 2);
            lblStatNotFoundIcon.Name = "lblStatNotFoundIcon";
            lblStatNotFoundIcon.Size = new Size(28, 32);
            lblStatNotFoundIcon.Font = new Font("Segoe UI Emoji", 14F);
            lblStatNotFoundIcon.Text = "❎";
            lblStatNotFoundIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblStatNotFoundIcon.TabIndex = 0;

            lblStatNotFoundValue.AutoSize = false;
            lblStatNotFoundValue.Location = new Point(38, 2);
            lblStatNotFoundValue.Name = "lblStatNotFoundValue";
            lblStatNotFoundValue.Size = new Size(65, 20);
            lblStatNotFoundValue.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblStatNotFoundValue.Text = "0";
            lblStatNotFoundValue.TextAlign = ContentAlignment.MiddleLeft;
            lblStatNotFoundValue.TabIndex = 1;

            lblStatNotFoundLabel.AutoSize = false;
            lblStatNotFoundLabel.Location = new Point(38, 22);
            lblStatNotFoundLabel.Name = "lblStatNotFoundLabel";
            lblStatNotFoundLabel.Size = new Size(145, 12);
            lblStatNotFoundLabel.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
            lblStatNotFoundLabel.Text = "NO ENCONTRADOS";
            lblStatNotFoundLabel.TextAlign = ContentAlignment.MiddleLeft;
            lblStatNotFoundLabel.TabIndex = 2;

            // lblStats — hidden (mantenemos la referencia para compatibilidad pero invisible)
            lblStats = new Label();
            lblStats.AutoSize = true;
            lblStats.Font = new Font("Segoe UI", 9F);
            lblStats.Location = new Point(0, 0);
            lblStats.Name = "lblStats";
            lblStats.Size = new Size(0, 0);
            lblStats.TabIndex = 99;
            lblStats.Visible = false;

            // ──────────────────────────────────────────────────────────────
            // panelLeft
            // ──────────────────────────────────────────────────────────────
            panelLeft.Controls.Add(lblLineCount);
            panelLeft.Controls.Add(btnClear);
            panelLeft.Controls.Add(btnParse);
            panelLeft.Controls.Add(txtInput);
            panelLeft.Controls.Add(lblInput);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 172);
            panelLeft.Name = "panelLeft";
            panelLeft.Padding = new Padding(12, 12, 12, 12);
            panelLeft.Size = new Size(310, 510);
            panelLeft.TabIndex = 2;

            // lblInput
            lblInput.AutoSize = false;
            lblInput.Dock = DockStyle.Top;
            lblInput.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInput.Height = 36;
            lblInput.Name = "lblInput";
            lblInput.TabIndex = 0;
            lblInput.Text = "📋  PEGAR CÓDIGOS DTC AQUÍ";
            lblInput.TextAlign = ContentAlignment.MiddleLeft;
            lblInput.Padding = new Padding(0, 0, 0, 4);

            // txtInput
            txtInput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtInput.Location = new Point(12, 56);
            txtInput.Multiline = true;
            txtInput.Name = "txtInput";
            txtInput.ScrollBars = ScrollBars.Vertical;
            txtInput.Size = new Size(282, 350);
            txtInput.TabIndex = 1;
            txtInput.Font = new Font("Consolas", 10F);
            txtInput.PlaceholderText = "Pega aquí tus códigos DTC...\r\n\r\nEjemplos:\r\nMID128 SID231 FMI5\r\nP20EE";

            // lblLineCount
            lblLineCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblLineCount.AutoSize = true;
            lblLineCount.Font = new Font("Segoe UI", 8F);
            lblLineCount.Location = new Point(12, 412);
            lblLineCount.Name = "lblLineCount";
            lblLineCount.TabIndex = 4;
            lblLineCount.Text = "≡  0 líneas";

            // btnParse
            btnParse.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnParse.Location = new Point(12, 470);
            btnParse.Name = "btnParse";
            btnParse.Size = new Size(200, 38);
            btnParse.TabIndex = 2;
            btnParse.Text = "▶  PROCESAR CÓDIGOS";
            
            btnParse.UseVisualStyleBackColor = true;

            // btnClear
            btnClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClear.Location = new Point(218, 470);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(80, 38);
            btnClear.TabIndex = 3;
            btnClear.Text = "🗑  Limpiar";
            btnClear.UseVisualStyleBackColor = true;

            // ──────────────────────────────────────────────────────────────
            // panelRight
            // ──────────────────────────────────────────────────────────────
            panelRight.Controls.Add(panelGridContainer);
            panelRight.Controls.Add(panelColumnCopy);
            panelRight.Controls.Add(panelButtons);
            panelRight.Controls.Add(lblResults);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(310, 172);
            panelRight.Name = "panelRight";
            panelRight.Padding = new Padding(10, 8, 10, 8);
            panelRight.Size = new Size(968, 510);
            panelRight.TabIndex = 3;

            // lblResults
            lblResults.AutoSize = false;
            lblResults.Dock = DockStyle.Top;
            lblResults.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResults.Location = new Point(10, 8);
            lblResults.Name = "lblResults";
            lblResults.Height = 32;
            lblResults.Padding = new Padding(2, 0, 0, 0);
            lblResults.TabIndex = 0;
            lblResults.Text = "☰  RESULTADOS";

            // panelColumnCopy
            panelColumnCopy.Controls.Add(btnClearSelectionTop);
            panelColumnCopy.Controls.Add(btnCopyCodeAltColumn);
            panelColumnCopy.Controls.Add(btnCopyCodeColumn);
            panelColumnCopy.Dock = DockStyle.Top;
            panelColumnCopy.Location = new Point(10, 40);
            panelColumnCopy.Name = "panelColumnCopy";
            panelColumnCopy.Size = new Size(948, 36);
            panelColumnCopy.TabIndex = 3;
            panelColumnCopy.Padding = new Padding(0, 3, 0, 3);

            btnClearSelectionTop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearSelectionTop.Location = new Point(810, 3);
            btnClearSelectionTop.Name = "btnClearSelectionTop";
            btnClearSelectionTop.Size = new Size(128, 28);
            btnClearSelectionTop.TabIndex = 2;
            btnClearSelectionTop.Text = "Deseleccionar";
            btnClearSelectionTop.UseVisualStyleBackColor = true;

            btnCopyCodeAltColumn.Location = new Point(108, 3);
            btnCopyCodeAltColumn.Name = "btnCopyCodeAltColumn";
            btnCopyCodeAltColumn.Size = new Size(104, 28);
            btnCopyCodeAltColumn.TabIndex = 1;
            btnCopyCodeAltColumn.Text = "Copiar COL. FFFF";
            btnCopyCodeAltColumn.UseVisualStyleBackColor = true;

            btnCopyCodeColumn.Location = new Point(0, 3);
            btnCopyCodeColumn.Name = "btnCopyCodeColumn";
            btnCopyCodeColumn.Size = new Size(104, 28);
            btnCopyCodeColumn.TabIndex = 0;
            btnCopyCodeColumn.Text = "Copiar CÓDIGO";
            btnCopyCodeColumn.UseVisualStyleBackColor = true;

            // panelGridContainer — wrapper con borde visual de 1px
            panelGridContainer.Controls.Add(panelEmptyState);
            panelGridContainer.Controls.Add(dgvCodes);
            panelGridContainer.Dock = DockStyle.Fill;
            panelGridContainer.Location = new Point(10, 76);
            panelGridContainer.Name = "panelGridContainer";
            panelGridContainer.Padding = new Padding(1);
            panelGridContainer.TabIndex = 6;

            // dgvCodes — fill dentro del panelGridContainer
            dgvCodes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCodes.Dock = DockStyle.Fill;
            dgvCodes.Location = new Point(1, 1);
            dgvCodes.Name = "dgvCodes";
            dgvCodes.RowTemplate.Height = 35;
            dgvCodes.Size = new Size(946, 378);
            dgvCodes.TabIndex = 2;

            // panelEmptyState — se muestra sobre el dgvCodes cuando no hay resultados
            panelEmptyState.Controls.Add(lblEmptyStateIcon);
            panelEmptyState.Controls.Add(lblEmptyStateTitle);
            panelEmptyState.Controls.Add(lblEmptyStateDesc);
            panelEmptyState.Dock = DockStyle.Fill;
            panelEmptyState.Location = new Point(1, 1);
            panelEmptyState.Name = "panelEmptyState";
            panelEmptyState.Size = new Size(946, 378);
            panelEmptyState.TabIndex = 5;

            lblEmptyStateIcon.AutoSize = false;
            lblEmptyStateIcon.Dock = DockStyle.None;
            lblEmptyStateIcon.Anchor = AnchorStyles.None;
            lblEmptyStateIcon.Font = new Font("Segoe UI Emoji", 42F);
            lblEmptyStateIcon.Location = new Point(374, 107);
            lblEmptyStateIcon.Name = "lblEmptyStateIcon";
            lblEmptyStateIcon.Size = new Size(100, 80);
            lblEmptyStateIcon.Text = "🔍";
            lblEmptyStateIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblEmptyStateIcon.TabIndex = 0;

            lblEmptyStateTitle.AutoSize = false;
            lblEmptyStateTitle.Anchor = AnchorStyles.None;
            lblEmptyStateTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblEmptyStateTitle.Location = new Point(274, 195);
            lblEmptyStateTitle.Name = "lblEmptyStateTitle";
            lblEmptyStateTitle.Size = new Size(400, 30);
            lblEmptyStateTitle.Text = "Aún no hay resultados";
            lblEmptyStateTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblEmptyStateTitle.TabIndex = 1;

            lblEmptyStateDesc.AutoSize = false;
            lblEmptyStateDesc.Anchor = AnchorStyles.None;
            lblEmptyStateDesc.Font = new Font("Segoe UI", 9.5F);
            lblEmptyStateDesc.Location = new Point(174, 231);
            lblEmptyStateDesc.Name = "lblEmptyStateDesc";
            lblEmptyStateDesc.Size = new Size(600, 24);
            lblEmptyStateDesc.Text = "Pega códigos DTC y presiona \"Procesar Códigos\" para ver los resultados.";
            lblEmptyStateDesc.TextAlign = ContentAlignment.MiddleCenter;
            lblEmptyStateDesc.TabIndex = 2;

            // panelButtons — fondo inferior con los botones de acción
            panelButtons.Controls.Add(btnZoomIn);
            panelButtons.Controls.Add(btnZoomReset);
            panelButtons.Controls.Add(btnZoomOut);
            panelButtons.Controls.Add(btnEdit);
            panelButtons.Controls.Add(btnAdd);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(10, 454);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(948, 48);
            panelButtons.TabIndex = 1;
            panelButtons.Padding = new Padding(0, 8, 0, 0);

            btnZoomIn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnZoomIn.Location = new Point(873, 10);
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new Size(68, 36);
            btnZoomIn.TabIndex = 4;
            btnZoomIn.Text = "Lupa +";
            btnZoomIn.UseVisualStyleBackColor = true;

            btnZoomReset.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnZoomReset.Location = new Point(800, 10);
            btnZoomReset.Name = "btnZoomReset";
            btnZoomReset.Size = new Size(68, 36);
            btnZoomReset.TabIndex = 3;
            btnZoomReset.Text = "100%";
            btnZoomReset.UseVisualStyleBackColor = true;

            btnZoomOut.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnZoomOut.Location = new Point(727, 10);
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new Size(68, 36);
            btnZoomOut.TabIndex = 2;
            btnZoomOut.Text = "Lupa -";
            btnZoomOut.UseVisualStyleBackColor = true;

            btnEdit.Location = new Point(128, 10);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(110, 36);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "✏  Editar";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Enabled = false;

            btnAdd.Location = new Point(10, 10);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(110, 36);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "+ Añadir";
            btnAdd.UseVisualStyleBackColor = true;

            // ──────────────────────────────────────────────────────────────
            // panelFilterSide — módulos a la derecha
            // ──────────────────────────────────────────────────────────────
            panelFilterSide.Controls.Add(btnFilterTVA);
            panelFilterSide.Controls.Add(btnFilterMAF);
            panelFilterSide.Controls.Add(btnFilterSCR);
            panelFilterSide.Controls.Add(btnFilterNOX);
            panelFilterSide.Controls.Add(btnFilterEGR);
            panelFilterSide.Controls.Add(btnFilterDPF);
            panelFilterSide.Controls.Add(btnFilterVNT);
            panelFilterSide.Controls.Add(lblFilterTitle);
            panelFilterSide.Dock = DockStyle.Right;
            panelFilterSide.Name = "panelFilterSide";
            panelFilterSide.Padding = new Padding(4, 8, 4, 8);
            panelFilterSide.Size = new Size(150, 510);
            panelFilterSide.TabIndex = 10;

            lblFilterTitle.AutoSize = false;
            lblFilterTitle.Dock = DockStyle.Top;
            lblFilterTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFilterTitle.Height = 32;
            lblFilterTitle.Name = "lblFilterTitle";
            lblFilterTitle.Text = "⚙️  MÓDULOS";
            lblFilterTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblFilterTitle.TabIndex = 0;

            // Legacy filter buttons (hidden, replaced by dynamic panel)
            btnFilterVNT.Location = new Point(8, 50);
            btnFilterVNT.Name = "btnFilterVNT";
            btnFilterVNT.Size = new Size(80, 38);
            btnFilterVNT.TabIndex = 1;
            btnFilterVNT.Text = "VNT";
            btnFilterVNT.UseVisualStyleBackColor = true;

            btnFilterDPF.Location = new Point(8, 96);
            btnFilterDPF.Name = "btnFilterDPF";
            btnFilterDPF.Size = new Size(80, 38);
            btnFilterDPF.TabIndex = 2;
            btnFilterDPF.Text = "DPF";
            btnFilterDPF.UseVisualStyleBackColor = true;

            btnFilterEGR.Location = new Point(8, 142);
            btnFilterEGR.Name = "btnFilterEGR";
            btnFilterEGR.Size = new Size(80, 38);
            btnFilterEGR.TabIndex = 3;
            btnFilterEGR.Text = "EGR";
            btnFilterEGR.UseVisualStyleBackColor = true;

            btnFilterNOX.Location = new Point(8, 188);
            btnFilterNOX.Name = "btnFilterNOX";
            btnFilterNOX.Size = new Size(80, 38);
            btnFilterNOX.TabIndex = 4;
            btnFilterNOX.Text = "NOX";
            btnFilterNOX.UseVisualStyleBackColor = true;

            btnFilterSCR.Location = new Point(8, 234);
            btnFilterSCR.Name = "btnFilterSCR";
            btnFilterSCR.Size = new Size(80, 38);
            btnFilterSCR.TabIndex = 5;
            btnFilterSCR.Text = "SCR";
            btnFilterSCR.UseVisualStyleBackColor = true;

            btnFilterMAF.Location = new Point(8, 280);
            btnFilterMAF.Name = "btnFilterMAF";
            btnFilterMAF.Size = new Size(80, 38);
            btnFilterMAF.TabIndex = 6;
            btnFilterMAF.Text = "MAF";
            btnFilterMAF.UseVisualStyleBackColor = true;

            btnFilterTVA.Location = new Point(8, 326);
            btnFilterTVA.Name = "btnFilterTVA";
            btnFilterTVA.Size = new Size(80, 38);
            btnFilterTVA.TabIndex = 7;
            btnFilterTVA.Text = "TVA";
            btnFilterTVA.UseVisualStyleBackColor = true;

            // ──────────────────────────────────────────────────────────────
            // StatusStrip — barra de estado inferior
            // ──────────────────────────────────────────────────────────────
            statusStrip = new StatusStrip();
            statusLabelInfo = new ToolStripStatusLabel();
            statusLabelSep1 = new ToolStripStatusLabel();
            statusLabelMode = new ToolStripStatusLabel();
            statusLabelSep2 = new ToolStripStatusLabel();
            statusLabelCount = new ToolStripStatusLabel();

            statusStrip.Items.AddRange(new ToolStripItem[]
            {
                statusLabelInfo,
                statusLabelSep1,
                statusLabelMode,
                statusLabelSep2,
                statusLabelCount
            });
            statusStrip.Location = new Point(0, 682);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1400, 22);
            statusStrip.TabIndex = 20;
            statusStrip.SizingGrip = false;

            statusLabelInfo.Name = "statusLabelInfo";
            statusLabelInfo.Text = "ℹ  Lista actualizada — Base de datos ECU TUNING";
            statusLabelInfo.Spring = false;
            statusLabelInfo.AutoSize = true;

            statusLabelSep1.Name = "statusLabelSep1";
            statusLabelSep1.Text = "    ";
            statusLabelSep1.Spring = true;

            statusLabelMode.Name = "statusLabelMode";
            statusLabelMode.Text = "🌙  Modo Oscuro  •";
            statusLabelMode.Spring = false;

            statusLabelSep2.Name = "statusLabelSep2";
            statusLabelSep2.Text = "    ";
            statusLabelSep2.Spring = false;

            statusLabelCount.Name = "statusLabelCount";
            statusLabelCount.Text = "0 códigos disponibles";
            statusLabelCount.Spring = false;

            // ──────────────────────────────────────────────────────────────
            // MainForm
            // ──────────────────────────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1400, 704);
            Controls.Add(panelRight);
            Controls.Add(panelFilterSide);
            Controls.Add(panelLeft);
            Controls.Add(panelStatsBar);
            Controls.Add(panelTop);
            Controls.Add(menuStrip);
            Controls.Add(statusStrip);
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(1200, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DtcDesk - Diccionario de Códigos DTC";

            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelStatsBar.ResumeLayout(false);
            panelStatTotal.ResumeLayout(false);
            panelStatFound.ResumeLayout(false);
            panelStatNotFound.ResumeLayout(false);
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            panelGridContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCodes).EndInit();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            ((System.ComponentModel.ISupportInitialize)picLogoRight).EndInit();
            panelEmptyState.ResumeLayout(false);
            panelButtons.ResumeLayout(false);
            panelFilterSide.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        // ── Top bar ──
        private Panel panelTop;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblStats;
        private PictureBox picLogo;
        private PictureBox picLogoRight;

        // ── Stats cards ──
        private Panel panelStatsBar;
        private Panel panelStatTotal;
        private Label lblStatTotalIcon;
        private Label lblStatTotalValue;
        private Label lblStatTotalLabel;
        private Panel panelStatFound;
        private Label lblStatFoundIcon;
        private Label lblStatFoundValue;
        private Label lblStatFoundLabel;
        private Panel panelStatNotFound;
        private Label lblStatNotFoundIcon;
        private Label lblStatNotFoundValue;
        private Label lblStatNotFoundLabel;

        // ── Left panel ──
        private Panel panelLeft;
        private Label lblInput;
        private TextBox txtInput;
        private Button btnParse;
        private Button btnClear;
        private Label lblLineCount;

        // ── Right panel ──
        private Panel panelRight;
        private Label lblResults;
        private CumulativeSelectionDataGridView dgvCodes;
        private Panel panelGridContainer;
        private Panel panelEmptyState;
        private Label lblEmptyStateIcon;
        private Label lblEmptyStateTitle;
        private Label lblEmptyStateDesc;
        private Panel panelColumnCopy;
        private Button btnCopyCodeColumn;
        private Button btnCopyCodeAltColumn;
        private Button btnClearSelectionTop;
        private Panel panelButtons;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnZoomOut;
        private Button btnZoomReset;
        private Button btnZoomIn;

        // ── Menu ──
        private MenuStrip menuStrip;
        private ToolStripMenuItem menuArchivo;
        private ToolStripMenuItem menuImportar;
        private ToolStripMenuItem menuExportar;
        private ToolStripSeparator menuSeparador1;
        private ToolStripMenuItem menuLimpiarDB;
        private ToolStripSeparator menuSeparador2;
        private ToolStripMenuItem menuSalir;
        private ToolStripMenuItem menuHerramientas;
        private ToolStripMenuItem menuEstadisticas;

        // ── Filter side panel ──
        private Panel panelFilterSide;
        private Label lblFilterTitle;
        private Button btnFilterVNT;
        private Button btnFilterDPF;
        private Button btnFilterEGR;
        private Button btnFilterNOX;
        private Button btnFilterSCR;
        private Button btnFilterMAF;
        private Button btnFilterTVA;

        // ── Search ──
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnSearchClear;
        private Label lblSearchMode;

        // ── Status strip ──
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabelInfo;
        private ToolStripStatusLabel statusLabelSep1;
        private ToolStripStatusLabel statusLabelMode;
        private ToolStripStatusLabel statusLabelSep2;
        private ToolStripStatusLabel statusLabelCount;
    }
}
