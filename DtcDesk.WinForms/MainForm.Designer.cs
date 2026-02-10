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
            lblStats = new Label();
            lblTitle = new Label();
            picLogo = new PictureBox();
            panelLeft = new Panel();
            btnClear = new Button();
            btnParse = new Button();
            txtInput = new TextBox();
            lblInput = new Label();
            panelRight = new Panel();
            dgvCodes = new CumulativeSelectionDataGridView();
            panelButtons = new Panel();
            btnEdit = new Button();
            btnAdd = new Button();
            lblResults = new Label();
            menuStrip.SuspendLayout();
            panelTop.SuspendLayout();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCodes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            panelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { menuArchivo, menuHerramientas });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1400, 24);
            menuStrip.TabIndex = 0;
            // 
            // menuArchivo
            // 
            menuArchivo.DropDownItems.AddRange(new ToolStripItem[] { menuImportar, menuExportar, menuSeparador1, menuLimpiarDB, menuSeparador2, menuSalir });
            menuArchivo.Name = "menuArchivo";
            menuArchivo.Size = new Size(60, 20);
            menuArchivo.Text = "Archivo";
            // 
            // menuImportar
            // 
            menuImportar.Name = "menuImportar";
            menuImportar.Size = new Size(200, 22);
            menuImportar.Text = "Importar CSV...";
            // 
            // menuExportar
            // 
            menuExportar.Name = "menuExportar";
            menuExportar.Size = new Size(200, 22);
            menuExportar.Text = "Exportar...";
            // 
            // menuSeparador1
            // 
            menuSeparador1.Name = "menuSeparador1";
            menuSeparador1.Size = new Size(197, 6);
            // 
            // menuLimpiarDB
            // 
            menuLimpiarDB.Name = "menuLimpiarDB";
            menuLimpiarDB.Size = new Size(200, 22);
            menuLimpiarDB.Text = "Limpiar Base de Datos...";
            // 
            // menuSeparador2
            // 
            menuSeparador2.Name = "menuSeparador2";
            menuSeparador2.Size = new Size(197, 6);
            // 
            // menuSalir
            // 
            menuSalir.Name = "menuSalir";
            menuSalir.Size = new Size(200, 22);
            menuSalir.Text = "Salir";
            // 
            // menuHerramientas
            // 
            menuHerramientas.DropDownItems.AddRange(new ToolStripItem[] { menuEstadisticas });
            menuHerramientas.Name = "menuHerramientas";
            menuHerramientas.Size = new Size(90, 20);
            menuHerramientas.Text = "Herramientas";
            // 
            // menuEstadisticas
            // 
            menuEstadisticas.Name = "menuEstadisticas";
            menuEstadisticas.Size = new Size(200, 22);
            menuEstadisticas.Text = "Ver Estadísticas DB";
            // 
            // panelTop
            // 
            panelTop.Controls.Add(picLogo);
            panelTop.Controls.Add(lblStats);
            panelTop.Controls.Add(lblTitle);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 24);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1400, 80);
            panelTop.TabIndex = 1;
            // 
            // lblStats
            // 
            lblStats.AutoSize = true;
            lblStats.Font = new Font("Segoe UI", 9F);
            lblStats.Location = new Point(20, 45);
            lblStats.Name = "lblStats";
            lblStats.Size = new Size(250, 15);
            lblStats.TabIndex = 1;
            lblStats.Text = "Total: 0 | Encontrados: 0 | No encontrados: 0";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.Location = new Point(15, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(350, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "DTC DESK - Diccionario de Códigos";
            // 
            // picLogo
            // 
            picLogo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            picLogo.Location = new Point(1230, 10);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(160, 60);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 2;
            picLogo.TabStop = false;
            // 
            // panelLeft
            // 
            panelLeft.Controls.Add(btnClear);
            panelLeft.Controls.Add(btnParse);
            panelLeft.Controls.Add(txtInput);
            panelLeft.Controls.Add(lblInput);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 104);
            panelLeft.Name = "panelLeft";
            panelLeft.Padding = new Padding(10);
            panelLeft.Size = new Size(450, 596);
            panelLeft.TabIndex = 2;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClear.Location = new Point(330, 540);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(102, 40);
            btnClear.TabIndex = 3;
            btnClear.Text = "Limpiar";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnParse
            // 
            btnParse.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnParse.Location = new Point(20, 540);
            btnParse.Name = "btnParse";
            btnParse.Size = new Size(304, 40);
            btnParse.TabIndex = 2;
            btnParse.Text = "PROCESAR CÓDIGOS";
            btnParse.UseVisualStyleBackColor = true;
            // 
            // txtInput
            // 
            txtInput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtInput.Location = new Point(20, 50);
            txtInput.Multiline = true;
            txtInput.Name = "txtInput";
            txtInput.ScrollBars = ScrollBars.Vertical;
            txtInput.Size = new Size(412, 480);
            txtInput.TabIndex = 1;
            txtInput.Font = new Font("Consolas", 10F);
            // 
            // lblInput
            // 
            lblInput.AutoSize = true;
            lblInput.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblInput.Location = new Point(20, 15);
            lblInput.Name = "lblInput";
            lblInput.Size = new Size(240, 20);
            lblInput.TabIndex = 0;
            lblInput.Text = "PEGAR CÓDIGOS DTC AQUÍ:";
            // 
            // panelRight
            // 
            panelRight.Controls.Add(dgvCodes);
            panelRight.Controls.Add(panelButtons);
            panelRight.Controls.Add(lblResults);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(450, 104);
            panelRight.Name = "panelRight";
            panelRight.Padding = new Padding(10);
            panelRight.Size = new Size(950, 596);
            panelRight.TabIndex = 3;
            // 
            // dgvCodes
            // 
            dgvCodes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCodes.Dock = DockStyle.Fill;
            dgvCodes.Location = new Point(10, 40);
            dgvCodes.Name = "dgvCodes";
            dgvCodes.RowTemplate.Height = 35;
            dgvCodes.Size = new Size(930, 520);
            dgvCodes.TabIndex = 2;
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(btnEdit);
            panelButtons.Controls.Add(btnAdd);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(10, 560);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(930, 60);
            panelButtons.TabIndex = 1;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(120, 10);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(100, 40);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "Editar";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Enabled = false;
            // 
            // btnAdd
            // 
            btnEdit.Location = new Point(120, 10);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(100, 40);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "Editar";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Enabled = false;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(10, 10);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(100, 40);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "Añadir";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // lblResults
            // 
            lblResults.AutoSize = true;
            lblResults.Dock = DockStyle.Top;
            lblResults.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblResults.Location = new Point(10, 10);
            lblResults.Name = "lblResults";
            lblResults.Padding = new Padding(0, 0, 0, 10);
            lblResults.Size = new Size(110, 30);
            lblResults.TabIndex = 0;
            lblResults.Text = "RESULTADOS:";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1400, 700);
            Controls.Add(panelRight);
            Controls.Add(panelLeft);
            Controls.Add(panelTop);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(1200, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DtcDesk - Diccionario de Códigos DTC";
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCodes).EndInit();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelTop;
        private Label lblTitle;
        private Label lblStats;
        private PictureBox picLogo;
        private Panel panelLeft;
        private Label lblInput;
        private TextBox txtInput;
        private Button btnParse;
        private Button btnClear;
        private Panel panelRight;
        private Label lblResults;
        private CumulativeSelectionDataGridView dgvCodes;
        private Panel panelButtons;
        private Button btnAdd;
        private Button btnEdit;
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
    }
}
