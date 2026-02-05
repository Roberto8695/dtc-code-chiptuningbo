namespace DtcDesk.WinForms
{
    partial class ImportForm
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
            lblInfo = new Label();
            lblFile = new Label();
            txtFilePath = new TextBox();
            btnSelectFile = new Button();
            btnImport = new Button();
            btnCancel = new Button();
            lblInstructions = new Label();
            SuspendLayout();
            // 
            // lblInfo
            // 
            lblInfo.AutoSize = true;
            lblInfo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblInfo.Location = new Point(20, 20);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(220, 21);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Importar Códigos DTC desde CSV";
            // 
            // lblInstructions
            // 
            lblInstructions.Location = new Point(20, 55);
            lblInstructions.Name = "lblInstructions";
            lblInstructions.Size = new Size(560, 60);
            lblInstructions.ForeColor = Color.White;
            lblInstructions.TabIndex = 1;
            lblInstructions.Text = "El archivo CSV debe tener las siguientes columnas:\n" +
                "• Code (obligatorio): código DTC\n" +
                "• Description (obligatorio): descripción del código\n" +
                "• Category, Source, Notes (opcionales)";
            // 
            // lblFile
            // 
            lblFile.AutoSize = true;
            lblFile.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFile.Location = new Point(20, 130);
            lblFile.Name = "lblFile";
            lblFile.Size = new Size(130, 19);
            lblFile.TabIndex = 2;
            lblFile.Text = "Archivo seleccionado:";
            // 
            // txtFilePath
            // 
            txtFilePath.Location = new Point(20, 155);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.ReadOnly = true;
            txtFilePath.Size = new Size(450, 23);
            txtFilePath.TabIndex = 3;
            txtFilePath.PlaceholderText = "Selecciona un archivo CSV...";
            // 
            // btnSelectFile
            // 
            btnSelectFile.Location = new Point(480, 153);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new Size(100, 27);
            btnSelectFile.TabIndex = 4;
            btnSelectFile.Text = "Examinar...";
            btnSelectFile.UseVisualStyleBackColor = true;
            // 
            // btnImport
            // 
            btnImport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnImport.Location = new Point(380, 210);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(200, 45);
            btnImport.TabIndex = 5;
            btnImport.Text = "IMPORTAR";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 9F);
            btnCancel.Location = new Point(20, 210);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 45);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // ImportForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 280);
            Controls.Add(btnCancel);
            Controls.Add(btnImport);
            Controls.Add(btnSelectFile);
            Controls.Add(txtFilePath);
            Controls.Add(lblFile);
            Controls.Add(lblInstructions);
            Controls.Add(lblInfo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ImportForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Importar Códigos DTC";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblInfo;
        private Label lblInstructions;
        private Label lblFile;
        private TextBox txtFilePath;
        private Button btnSelectFile;
        private Button btnImport;
        private Button btnCancel;
    }
}
