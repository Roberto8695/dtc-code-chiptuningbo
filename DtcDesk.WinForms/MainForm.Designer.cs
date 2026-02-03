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
            panelTop = new Panel();
            lblStats = new Label();
            lblTitle = new Label();
            panelLeft = new Panel();
            btnClear = new Button();
            btnParse = new Button();
            txtInput = new TextBox();
            lblInput = new Label();
            panelRight = new Panel();
            dgvCodes = new DataGridView();
            panelButtons = new Panel();
            btnImport = new Button();
            btnExport = new Button();
            btnDelete = new Button();
            btnEdit = new Button();
            btnAdd = new Button();
            lblResults = new Label();
            panelTop.SuspendLayout();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCodes).BeginInit();
            panelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.Controls.Add(lblStats);
            panelTop.Controls.Add(lblTitle);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1400, 70);
            panelTop.TabIndex = 0;
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
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(15, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(390, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "DTC DESK - Diccionario de Códigos";
            // 
            // panelLeft
            // 
            panelLeft.Controls.Add(btnClear);
            panelLeft.Controls.Add(btnParse);
            panelLeft.Controls.Add(txtInput);
            panelLeft.Controls.Add(lblInput);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 70);
            panelLeft.Name = "panelLeft";
            panelLeft.Padding = new Padding(10);
            panelLeft.Size = new Size(450, 630);
            panelLeft.TabIndex = 1;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClear.Location = new Point(332, 575);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(100, 40);
            btnClear.TabIndex = 3;
            btnClear.Text = "Limpiar";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnParse
            // 
            btnParse.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnParse.Location = new Point(20, 575);
            btnParse.Name = "btnParse";
            btnParse.Size = new Size(300, 40);
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
            txtInput.Size = new Size(412, 510);
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
            panelRight.Location = new Point(450, 70);
            panelRight.Name = "panelRight";
            panelRight.Padding = new Padding(10);
            panelRight.Size = new Size(950, 630);
            panelRight.TabIndex = 2;
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
            panelButtons.Controls.Add(btnImport);
            panelButtons.Controls.Add(btnExport);
            panelButtons.Controls.Add(btnDelete);
            panelButtons.Controls.Add(btnEdit);
            panelButtons.Controls.Add(btnAdd);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(10, 560);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(930, 60);
            panelButtons.TabIndex = 1;
            // 
            // btnImport
            // 
            btnImport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnImport.Location = new Point(820, 10);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(100, 40);
            btnImport.TabIndex = 4;
            btnImport.Text = "Importar";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnExport
            // 
            btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExport.Location = new Point(700, 10);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(110, 40);
            btnExport.TabIndex = 3;
            btnExport.Text = "Exportar";
            btnExport.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(230, 10);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(100, 40);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Eliminar";
            btnDelete.UseVisualStyleBackColor = true;
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
            MinimumSize = new Size(1200, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DtcDesk - Diccionario de Códigos DTC";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCodes).EndInit();
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelTop;
        private Label lblTitle;
        private Label lblStats;
        private Panel panelLeft;
        private Label lblInput;
        private TextBox txtInput;
        private Button btnParse;
        private Button btnClear;
        private Panel panelRight;
        private Label lblResults;
        private DataGridView dgvCodes;
        private Panel panelButtons;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnExport;
        private Button btnImport;
    }
}
