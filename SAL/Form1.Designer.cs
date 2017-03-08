namespace SAL
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tssl_msg = new System.Windows.Forms.ToolStripStatusLabel();
            this.sidebar = new System.Windows.Forms.Panel();
            this.listView1 = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.tsldbname = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.sidebar.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsldbname,
            this.tssl_msg});
            this.statusStrip1.Location = new System.Drawing.Point(0, 486);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(821, 24);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tssl_msg
            // 
            this.tssl_msg.Name = "tssl_msg";
            this.tssl_msg.Size = new System.Drawing.Size(158, 19);
            this.tssl_msg.Text = "toolStripStatusLabel1";
            // 
            // sidebar
            // 
            this.sidebar.Controls.Add(this.listView1);
            this.sidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidebar.Location = new System.Drawing.Point(0, 0);
            this.sidebar.Name = "sidebar";
            this.sidebar.Size = new System.Drawing.Size(200, 486);
            this.sidebar.TabIndex = 3;
            // 
            // listView1
            // 
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.GridLines = true;
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(200, 486);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "1Document.ico");
            this.imageList1.Images.SetKeyName(1, "2Programs.ico");
            this.imageList1.Images.SetKeyName(2, "3Screw.ico");
            this.imageList1.Images.SetKeyName(3, "4find_ico.png");
            this.imageList1.Images.SetKeyName(4, "5xlsx_32.png");
            this.imageList1.Images.SetKeyName(5, "6calc.png");
            this.imageList1.Images.SetKeyName(6, "7Print3.ico");
            this.imageList1.Images.SetKeyName(7, "8Report.ico");
            this.imageList1.Images.SetKeyName(8, "9xlsx_imp.PNG");
            this.imageList1.Images.SetKeyName(9, "browse.png");
            this.imageList1.Images.SetKeyName(10, "C1FlexPrintable.bmp");
            this.imageList1.Images.SetKeyName(11, "dashboard.png");
            this.imageList1.Images.SetKeyName(12, "DrawerFull.ico");
            this.imageList1.Images.SetKeyName(13, "edit.png");
            this.imageList1.Images.SetKeyName(14, "logs.png");
            this.imageList1.Images.SetKeyName(15, "Networkuploads.ico");
            this.imageList1.Images.SetKeyName(16, "TrashEmpty.ico");
            this.imageList1.Images.SetKeyName(17, "World.ico");
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(200, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 486);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // tsldbname
            // 
            this.tsldbname.Name = "tsldbname";
            this.tsldbname.Size = new System.Drawing.Size(158, 19);
            this.tsldbname.Text = "toolStripStatusLabel1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(821, 510);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.sidebar);
            this.Controls.Add(this.statusStrip1);
            this.IsMdiContainer = true;
            this.Name = "Form1";
            this.Text = "SAL system";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.sidebar.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        public System.Windows.Forms.ToolStripStatusLabel tssl_msg;
        private System.Windows.Forms.Panel sidebar;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ListView listView1;
        public System.Windows.Forms.ToolStripStatusLabel tsldbname;
    }
}

