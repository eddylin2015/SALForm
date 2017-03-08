namespace SAL.forms
{
    partial class Form_Print_Html
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tlsPrt = new System.Windows.Forms.ToolStripLabel();
            this.tlsZoomIn = new System.Windows.Forms.ToolStripLabel();
            this.tlsZoomOut = new System.Windows.Forms.ToolStripLabel();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlsPrt,
            this.tlsZoomIn,
            this.tlsZoomOut});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(282, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tlsPrt
            // 
            this.tlsPrt.Name = "tlsPrt";
            this.tlsPrt.Size = new System.Drawing.Size(42, 22);
            this.tlsPrt.Text = "print";
            // 
            // tlsZoomIn
            // 
            this.tlsZoomIn.Name = "tlsZoomIn";
            this.tlsZoomIn.Size = new System.Drawing.Size(61, 22);
            this.tlsZoomIn.Text = "Zoom+";
            // 
            // tlsZoomOut
            // 
            this.tlsZoomOut.Name = "tlsZoomOut";
            this.tlsZoomOut.Size = new System.Drawing.Size(56, 22);
            this.tlsZoomOut.Text = "Zoom-";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 25);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(282, 230);
            this.webBrowser1.TabIndex = 1;
            // 
            // Form_Print_Html
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 255);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "Form_Print_Html";
            this.Text = "Form_Print_Html";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel tlsPrt;
        private System.Windows.Forms.ToolStripLabel tlsZoomIn;
        private System.Windows.Forms.ToolStripLabel tlsZoomOut;
        private System.Windows.Forms.WebBrowser webBrowser1;
    }
}