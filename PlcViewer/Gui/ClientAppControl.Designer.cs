namespace PlcViewer.Gui
{
    partial class ClientAppControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpApp = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblId = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblDurum = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblStart = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblSure = new System.Windows.Forms.Label();
            this.grpApp.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpApp
            // 
            this.grpApp.Controls.Add(this.groupBox1);
            this.grpApp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpApp.Location = new System.Drawing.Point(0, 0);
            this.grpApp.Name = "grpApp";
            this.grpApp.Size = new System.Drawing.Size(1693, 864);
            this.grpApp.TabIndex = 0;
            this.grpApp.TabStop = false;
            this.grpApp.Text = "groupBox1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblSure);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.lblStart);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.lblDurum);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lblId);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(6, 21);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(355, 199);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 37);
            this.label1.TabIndex = 0;
            this.label1.Text = "Id";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblId
            // 
            this.lblId.BackColor = System.Drawing.Color.White;
            this.lblId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblId.Location = new System.Drawing.Point(99, 18);
            this.lblId.Name = "lblId";
            this.lblId.Size = new System.Drawing.Size(225, 37);
            this.lblId.TabIndex = 1;
            this.lblId.Text = "label2";
            this.lblId.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Location = new System.Drawing.Point(6, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 37);
            this.label3.TabIndex = 0;
            this.label3.Text = "Durum";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDurum
            // 
            this.lblDurum.BackColor = System.Drawing.Color.White;
            this.lblDurum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDurum.Location = new System.Drawing.Point(99, 55);
            this.lblDurum.Name = "lblDurum";
            this.lblDurum.Size = new System.Drawing.Size(225, 37);
            this.lblDurum.TabIndex = 1;
            this.lblDurum.Text = "label2";
            this.lblDurum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label5.Location = new System.Drawing.Point(6, 92);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 37);
            this.label5.TabIndex = 0;
            this.label5.Text = "Başlangıç";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStart
            // 
            this.lblStart.BackColor = System.Drawing.Color.White;
            this.lblStart.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStart.Location = new System.Drawing.Point(99, 92);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(225, 37);
            this.lblStart.TabIndex = 1;
            this.lblStart.Text = "label2";
            this.lblStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label7.Location = new System.Drawing.Point(6, 129);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 37);
            this.label7.TabIndex = 0;
            this.label7.Text = "Süre";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSure
            // 
            this.lblSure.BackColor = System.Drawing.Color.White;
            this.lblSure.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSure.Location = new System.Drawing.Point(99, 129);
            this.lblSure.Name = "lblSure";
            this.lblSure.Size = new System.Drawing.Size(225, 37);
            this.lblSure.TabIndex = 1;
            this.lblSure.Text = "label2";
            this.lblSure.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ClientAppControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpApp);
            this.Name = "ClientAppControl";
            this.Size = new System.Drawing.Size(1693, 864);
            this.Load += new System.EventHandler(this.ClientAppControl_Load);
            this.grpApp.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpApp;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSure;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblDurum;
        private System.Windows.Forms.Label label3;
    }
}
