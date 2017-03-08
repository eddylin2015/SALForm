using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAL.forms
{
    public partial class Form_Print_Html : Form
    {

        public Form_Print_Html(String url, string[] margins)
          : base()
        {
            InitializeComponent();
            this.webBrowser1.Navigate(url);
            try
            {
                bkPG(margins);
            }
            catch (Exception excep)
            {
                MessageBox.Show(excep.Message);
            }
            InitComp();
        }

        public Form_Print_Html(String filename, String datafilename, string[] margins)
            : base()
        {
            InitializeComponent();
            this.webBrowser1.Navigate(filename);
            temp_url = datafilename;
            temp_html = filename;
            try
            {
                bkPG(margins);
            }
            catch (Exception excep)
            {
                MessageBox.Show(excep.Message);
            }
            InitComp();
        }
        private void InitComp()
        {
            this.tlsPrt.Click += Prt;
            this.tlsZoomIn.Click += ZoomIn;
            this.tlsZoomOut.Click += ZoomOut;
        }
        private void bkPG(string[] margins)
        {
            string keyName = @"Software\Microsoft\Internet Explorer\PageSetup";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName, true))
            {
                if (key != null)
                {
                    old_footer = key.GetValue("footer").ToString();
                    old_header = key.GetValue("header").ToString();
                    margin_right = key.GetValue("margin_right").ToString();
                    margin_left = key.GetValue("margin_left").ToString();
                    margin_bottom = key.GetValue("margin_bottom").ToString();
                    margin_top = key.GetValue("margin_top").ToString();
                    Print_Background = key.GetValue("Print_Background").ToString();
                    key.SetValue("Print_Background", "yes");
                    key.SetValue("footer", "");
                    key.SetValue("header", "");
                    key.SetValue("margin_top", margins[0]);
                    key.SetValue("margin_bottom", margins[1]);
                    key.SetValue("margin_left", margins[2]);
                    key.SetValue("margin_right", margins[3]);
                }
            }
        }
        private String temp_url = null;
        private String temp_html = null;
        private String old_footer;
        private String old_header;
        private String Print_Background;
        private String Shrink_To_Fit;
        private String margin_bottom;//0.39370
        private String margin_left;//0.59055
        private String margin_right;//0.59055
        private String margin_top;//0.29016
        private void Prt(object sender, EventArgs e)
        {
            this.webBrowser1.Document.Body.Style = "ZOOM:1";

            webBrowser1.ShowPrintPreviewDialog();
        }

        private void Form_Print_Html_FormClosing(object sender, FormClosingEventArgs e)
        {
            string keyName = @"Software\Microsoft\Internet Explorer\PageSetup";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName, true))
            {
                if (key != null && old_footer != null)
                {
                    try
                    {
                        key.SetValue("footer", old_footer);
                        key.SetValue("header", old_header);
                        key.SetValue("Print_Background", Print_Background);
                        key.SetValue("margin_bottom", margin_bottom);
                        key.SetValue("margin_top", margin_top);
                        key.SetValue("margin_left", margin_left);
                        key.SetValue("margin_right", margin_right);
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show(exp.Message);
                    }

                }
            }
            if (temp_url != null)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(temp_url);
                if (fi.Exists)
                {
                    fi.Delete();
                }
            }
            if (temp_html != null)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(temp_html);
                if (fi.Exists)
                {
                    fi.Delete();
                }
            }
        }

        private void ZoomIn(object sender, EventArgs e)
        {
            zoom_v += 0.5M;
            this.webBrowser1.Document.Body.Style = "ZOOM:" + zoom_v;
        }
        private decimal zoom_v = 1;

        private void ZoomOut(object sender, EventArgs e)
        {
            zoom_v -= 0.5M;
            this.webBrowser1.Document.Body.Style = "ZOOM:" + zoom_v;
        }
    }
}
