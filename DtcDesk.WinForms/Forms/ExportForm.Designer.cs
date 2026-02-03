namespace DtcDesk.WinForms
{
    partial class ExportForm
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
            lblFormat = new Label();
            chkIncludeDescription = new CheckBox();
            chkIncludeCategory = new CheckBox();
            chkOnlyNotFound = new CheckBox();
            btnExportTxt = new Button();
            btnExportCsv = new Button();
            btnCancel = new Button();
            groupOptions = new GroupBox();
            groupOptions.SuspendLayout();
            SuspendLayout();
            // 
            // lblInfo
            // 
            lblInfo.AutoSize = true;
            lblInfo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblInfo.Location = new Point(20, 20);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(200, 21);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Exportar códigos DTC";
            // 
            // lblFormat
            // 
            lblFormat.AutoSize = true;
            lblFormat.Font = new Font("Segoe UI", 9F);
            lblFormat.Location = new Point(20, 55);
            lblFormat.Name = "lblFormat";
            lblFormat.Size = new Size(280, 15);
            lblFormat.TabIndex = 1;
            lblFormat.Text = "Selecciona el formato y las opciones de exportación:";
            // 
            // groupOptions
            // 
            groupOptions.Controls.Add(chkOnlyNotFound);
            groupOptions.Controls.Add(chkIncludeCategory);
            groupOptions.Controls.Add(chkIncludeDescription);
            groupOptions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupOptions.Location = new Point(20, 85);
            groupOptions.Name = "groupOptions";
            groupOptions.Size = new Size(460, 130);
            groupOptions.TabIndex = 2;
            groupOptions.TabStop = false;
            groupOptions.Text = "Opciones";
            // 
            // chkIncludeDescription
            // 
            chkIncludeDescription.AutoSize = true;
            chkIncludeDescription.Checked = true;
            chkIncludeDescription.CheckState = CheckState.Checked;
            chkIncludeDescription.Font = new Font("Segoe UI", 9F);
            chkIncludeDescription.Location = new Point(20, 30);
            chkIncludeDescription.Name = "chkIncludeDescription";
            chkIncludeDescription.Size = new Size(141, 19);
            chkIncludeDescription.TabIndex = 0;
            chkIncludeDescription.Text = "Incluir descripción";
            chkIncludeDescription.UseVisualStyleBackColor = true;
            // 
            // chkIncludeCategory
            // 
            chkIncludeCategory.AutoSize = true;
            chkIncludeCategory.Checked = true;
            chkIncludeCategory.CheckState = CheckState.Checked;
            chkIncludeCategory.Font = new Font("Segoe UI", 9F);
            chkIncludeCategory.Location = new Point(20, 60);
            chkIncludeCategory.Name = "chkIncludeCategory";
            chkIncludeCategory.Size = new Size(180, 19);
            chkIncludeCategory.TabIndex = 1;
            chkIncludeCategory.Text = "Incluir categoría y fuente";
            chkIncludeCategory.UseVisualStyleBackColor = true;
            // 
            // chkOnlyNotFound
            // 
            chkOnlyNotFound.AutoSize = true;
            chkOnlyNotFound.Font = new Font("Segoe UI", 9F);
            chkOnlyNotFound.Location = new Point(20, 90);
            chkOnlyNotFound.Name = "chkOnlyNotFound";
            chkOnlyNotFound.Size = new Size(210, 19);
            chkOnlyNotFound.TabIndex = 2;
            chkOnlyNotFound.Text = "Solo códigos no encontrados";
            chkOnlyNotFound.UseVisualStyleBackColor = true;
            // 
            // btnExportTxt
            // 
            btnExportTxt.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExportTxt.Location = new Point(20, 235);
            btnExportTxt.Name = "btnExportTxt";
            btnExportTxt.Size = new Size(210, 50);
            btnExportTxt.TabIndex = 3;
            btnExportTxt.Text = "📄 Exportar como TXT";
            btnExportTxt.UseVisualStyleBackColor = true;
            // 
            // btnExportCsv
            // 
            btnExportCsv.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExportCsv.Location = new Point(270, 235);
            btnExportCsv.Name = "btnExportCsv";
            btnExportCsv.Size = new Size(210, 50);
            btnExportCsv.TabIndex = 4;
            btnExportCsv.Text = "📊 Exportar como CSV";
            btnExportCsv.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 9F);
            btnCancel.Location = new Point(20, 300);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 35);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // ExportForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 360);
            Controls.Add(btnCancel);
            Controls.Add(btnExportCsv);
            Controls.Add(btnExportTxt);
            Controls.Add(groupOptions);
            Controls.Add(lblFormat);
            Controls.Add(lblInfo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ExportForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Exportar Códigos DTC";
            groupOptions.ResumeLayout(false);
            groupOptions.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblInfo;
        private Label lblFormat;
        private GroupBox groupOptions;
        private CheckBox chkIncludeDescription;
        private CheckBox chkIncludeCategory;
        private CheckBox chkOnlyNotFound;
        private Button btnExportTxt;
        private Button btnExportCsv;
        private Button btnCancel;
    }
}
