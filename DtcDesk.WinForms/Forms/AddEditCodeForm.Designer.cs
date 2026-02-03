namespace DtcDesk.WinForms
{
    partial class AddEditCodeForm
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
            lblCode = new Label();
            txtCode = new TextBox();
            lblDescription = new Label();
            txtDescription = new TextBox();
            lblCategory = new Label();
            cmbCategory = new ComboBox();
            lblSource = new Label();
            txtSource = new TextBox();
            lblNotes = new Label();
            txtNotes = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblCode
            // 
            lblCode.AutoSize = true;
            lblCode.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCode.Location = new Point(20, 20);
            lblCode.Name = "lblCode";
            lblCode.Size = new Size(107, 19);
            lblCode.TabIndex = 0;
            lblCode.Text = "Código DTC: *";
            // 
            // txtCode
            // 
            txtCode.Font = new Font("Consolas", 12F, FontStyle.Bold);
            txtCode.Location = new Point(20, 45);
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(150, 26);
            txtCode.TabIndex = 1;
            txtCode.PlaceholderText = "P0420";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDescription.Location = new Point(20, 85);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(112, 19);
            lblDescription.TabIndex = 2;
            lblDescription.Text = "Descripción: *";
            // 
            // txtDescription
            // 
            txtDescription.Font = new Font("Segoe UI", 10F);
            txtDescription.Location = new Point(20, 110);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Size = new Size(560, 80);
            txtDescription.TabIndex = 3;
            txtDescription.PlaceholderText = "Ej: Eficiencia del catalizador por debajo del umbral";
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCategory.Location = new Point(20, 205);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(82, 19);
            lblCategory.TabIndex = 4;
            lblCategory.Text = "Categoría:";
            // 
            // cmbCategory
            // 
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.Font = new Font("Segoe UI", 10F);
            cmbCategory.FormattingEnabled = true;
            cmbCategory.Location = new Point(20, 230);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Size = new Size(250, 25);
            cmbCategory.TabIndex = 5;
            // 
            // lblSource
            // 
            lblSource.AutoSize = true;
            lblSource.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSource.Location = new Point(20, 270);
            lblSource.Name = "lblSource";
            lblSource.Size = new Size(63, 19);
            lblSource.TabIndex = 6;
            lblSource.Text = "Fuente:";
            // 
            // txtSource
            // 
            txtSource.Font = new Font("Segoe UI", 10F);
            txtSource.Location = new Point(20, 295);
            txtSource.Name = "txtSource";
            txtSource.Size = new Size(560, 25);
            txtSource.TabIndex = 7;
            txtSource.PlaceholderText = "Ej: OBD-II Standard, VAG Group, BMW, etc.";
            // 
            // lblNotes
            // 
            lblNotes.AutoSize = true;
            lblNotes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblNotes.Location = new Point(20, 335);
            lblNotes.Name = "lblNotes";
            lblNotes.Size = new Size(55, 19);
            lblNotes.TabIndex = 8;
            lblNotes.Text = "Notas:";
            // 
            // txtNotes
            // 
            txtNotes.Font = new Font("Segoe UI", 9F);
            txtNotes.Location = new Point(20, 360);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Size = new Size(560, 80);
            txtNotes.TabIndex = 9;
            txtNotes.PlaceholderText = "Información adicional, soluciones, etc.";
            // 
            // btnSave
            // 
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.Location = new Point(380, 460);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(200, 45);
            btnSave.TabIndex = 10;
            btnSave.Text = "GUARDAR";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 10F);
            btnCancel.Location = new Point(20, 460);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 45);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // AddEditCodeForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 525);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(txtNotes);
            Controls.Add(lblNotes);
            Controls.Add(txtSource);
            Controls.Add(lblSource);
            Controls.Add(cmbCategory);
            Controls.Add(lblCategory);
            Controls.Add(txtDescription);
            Controls.Add(lblDescription);
            Controls.Add(txtCode);
            Controls.Add(lblCode);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AddEditCodeForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Añadir/Editar Código DTC";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblCode;
        private TextBox txtCode;
        private Label lblDescription;
        private TextBox txtDescription;
        private Label lblCategory;
        private ComboBox cmbCategory;
        private Label lblSource;
        private TextBox txtSource;
        private Label lblNotes;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;
    }
}
