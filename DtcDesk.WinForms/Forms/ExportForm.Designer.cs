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
            chkIncludeModules = new CheckBox();
            btnExportJson = new Button();
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
            lblInfo.Size = new Size(182, 21);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Exportar backup DTC";
            // 
            // lblFormat
            // 
            lblFormat.AutoSize = true;
            lblFormat.Font = new Font("Segoe UI", 9F);
            lblFormat.Location = new Point(20, 55);
            lblFormat.Name = "lblFormat";
            lblFormat.Size = new Size(232, 15);
            lblFormat.TabIndex = 1;
            lblFormat.Text = "Selecciona las opciones del backup:";
            // 
            // groupOptions
            // 
            groupOptions.Controls.Add(chkIncludeModules);
            groupOptions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupOptions.Location = new Point(20, 85);
            groupOptions.Name = "groupOptions";
            groupOptions.Size = new Size(460, 70);
            groupOptions.TabIndex = 2;
            groupOptions.TabStop = false;
            groupOptions.Text = "Opciones";
            groupOptions.ForeColor = Color.FromArgb(232, 232, 232);
            // 
            // chkIncludeModules
            // 
            chkIncludeModules.AutoSize = true;
            chkIncludeModules.Checked = true;
            chkIncludeModules.CheckState = CheckState.Checked;
            chkIncludeModules.Font = new Font("Segoe UI", 9F);
            chkIncludeModules.Location = new Point(20, 30);
            chkIncludeModules.Name = "chkIncludeModules";
            chkIncludeModules.Size = new Size(245, 19);
            chkIncludeModules.TabIndex = 0;
            chkIncludeModules.Text = "Incluir módulos y reglas";
            chkIncludeModules.UseVisualStyleBackColor = true;
            // 
            // btnExportJson
            // 
            btnExportJson.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExportJson.Location = new Point(20, 175);
            btnExportJson.Name = "btnExportJson";
            btnExportJson.Size = new Size(460, 45);
            btnExportJson.TabIndex = 3;
            btnExportJson.Text = "🧩 Exportar backup JSON";
            btnExportJson.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 9F);
            btnCancel.Location = new Point(20, 235);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 35);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // ExportForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 290);
            Controls.Add(btnCancel);
            Controls.Add(btnExportJson);
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
        private CheckBox chkIncludeModules;
        private Button btnExportJson;
        private Button btnCancel;
    }
}
