using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAL.forms
{

    class utls
    {
        public static string prompt(string title, string prompt, string defval)
        {
            input_txt_frm ifm = new input_txt_frm(title, prompt, defval);
            if (ifm.ShowDialog() == DialogResult.OK)
            {
                return ifm.getText();
            }
            return null;
        }
        public static void alert(string x)
        {
            MessageBox.Show(x);
        }

        public static void ShowView(String sql)
        {
            FormDataGrid_Cmd fg = new FormDataGrid_Cmd(new iSqlCmdFormGrid(ESData.GetInst.GetAdapter(),
                ESData.GetInst.GetCommand(sql),
             null,
             null,
             null),
             ESData.GetInst.dict,
             forms.BindingListOptions.AllowModifyNo);
            fg.MdiParent = Pub.cfg.curr_userinfo.MainForm;
            fg.Text = "(ReadOnly)" + sql;
            fg.Show();
        }
        public static void ShowTable(String sql)
        {
            FormDataGrid_Cmd fg = new FormDataGrid_Cmd(new iSqlCmdFormGrid(ESData.GetInst.GetAdapter(),
                ESData.GetInst.GetCommand(sql),
             null,
             null,
             null),
             ESData.GetInst.dict,
             forms.BindingListOptions.AllPri);
            fg.MdiParent = Pub.cfg.curr_userinfo.MainForm;
            fg.Text = "(ReadOnly)" + sql;
            fg.Show();
        }
        public static void ShowTableWithUpdate(String sql)
        {
            DbDataAdapter da = ESData.GetInst.GetAdapter();
            da.SelectCommand = ESData.GetInst.GetCommand(sql);
            ESData.GetInst.GetDbCommandBuilder(da);
            FormDataGrid_Cmd fg = new FormDataGrid_Cmd(new forms.iSqlCmdFormGrid(da),
             ESData.GetInst.dict,
             forms.BindingListOptions.AllPri);
            fg.MdiParent = Pub.cfg.curr_userinfo.MainForm;
            fg.Text = "(+RW)" + sql;
            fg.Show();
        }
        public static void ShowTableWithImportxls(String sql)
        {
            DbDataAdapter da = ESData.GetInst.GetAdapter();
            da.SelectCommand = ESData.GetInst.GetCommand(sql);
            ESData.GetInst.GetDbCommandBuilder(da);
            FormDataGrid_Cmd_Importxls fg = new FormDataGrid_Cmd_Importxls(new forms.iSqlCmdFormGrid(da),
             ESData.GetInst.dict,
             forms.BindingListOptions.AllPri);
            fg.MdiParent = Pub.cfg.curr_userinfo.MainForm;
            fg.Text = "(+RW)" + sql;
            fg.Show();
        }
        public static void ShowTableWithUpdateAndInsert(String SQL, String insSQL)
        {
            DbDataAdapter da = ESData.GetInst.GetAdapter();
            da.SelectCommand = ESData.GetInst.GetCommand(SQL);
            ESData.GetInst.GetDbCommandBuilder(da);

            da.InsertCommand = ESData.GetInst.GetCommand(insSQL);
            FormDataGrid_Cmd fg = new FormDataGrid_Cmd(new forms.iSqlCmdFormGrid(da),
             ESData.GetInst.dict,
             forms.BindingListOptions.AllPri);
            fg.MdiParent = Pub.cfg.curr_userinfo.MainForm;
            fg.Text = "(+RW)" + SQL;
            fg.Show();
        }
    }
    interface iInput_Text_Frm
    {
        string getText();
    }
    class input_txt_frm : Form, iInput_Text_Frm
    {
        public string getText() { return inputtxt.Text; }
        private Label promptlbl = new Label();
        private TextBox inputtxt = new TextBox();
        private Button okbtn = new Button();
        private TableLayoutPanel tp = new TableLayoutPanel();

        public input_txt_frm(string _title, string _prompt, string _defval)
        {
            tp.RowCount = 2;
            tp.ColumnCount = 2;
            tp.Controls.Add(promptlbl, 0, 0);
            tp.Controls.Add(inputtxt, 1, 0);
            tp.Controls.Add(okbtn, 1, 1);
            tp.Dock = DockStyle.Fill;
            this.Text = _title;
            promptlbl.Text = _prompt;
            inputtxt.Text = _defval;
            okbtn.Text = "OK";
            this.Controls.Add(tp);
            inputtxt.KeyUp += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter && inputtxt.Text.Length > 0)
                    okbtn.Focus();
            };
            okbtn.Click += (sender, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
        }
    }
    public class PubInfoBox_MEMO : Form
    {
        public RichTextBox memo;
        public Button btn;
        private Button prtBTN;
        public PubInfoBox_MEMO()
        {
            this.Size = new System.Drawing.Size(600, 800);
            this.Text = "Info";
            memo = new RichTextBox();
            memo.Multiline = true;
            memo.Dock = DockStyle.Fill;
            memo.Font = new System.Drawing.Font("細明體_HKSCS", 10);
            prtBTN = new Button();
            prtBTN.Text = "打印";
            prtBTN.Dock = DockStyle.Bottom;
            
            btn = new Button();
            btn.Dock = DockStyle.Bottom;
            btn.Text = "OK";
            
            this.Controls.AddRange(new System.Windows.Forms.Control[] { memo, btn, prtBTN });
        }
        public PubInfoBox_MEMO(Form parentForm)
            : this()
        {

            this.MdiParent = parentForm;

        }
    }
        public class PubInfoBox_PRT : Form
    {
        public RichTextBox memo;
        public Button btn;
        private Button prtBTN;
        public PubInfoBox_PRT()
        {
            this.Size = new System.Drawing.Size(600, 800);
            this.Text = "Info";
            memo = new RichTextBox();
            memo.Multiline = true;
            memo.Dock = DockStyle.Fill;
            memo.Font = new System.Drawing.Font("細明體_HKSCS", 10);
            prtBTN = new Button();
            prtBTN.Text = "打印";
            prtBTN.Dock = DockStyle.Bottom;
            prtBTN.Click += prtbtnClick;
            btn = new Button();
            btn.Dock = DockStyle.Bottom;
            btn.Text = "退出";
            btn.Click += btnClick;
            this.Controls.AddRange(new System.Windows.Forms.Control[] { memo, btn, prtBTN });
        }
        public PubInfoBox_PRT(Form parentForm)
            : this()
        {

            this.MdiParent = parentForm;

        }
        private void btnClick(Object sender, EventArgs e)
        {
            Close();
        }
        private void prtbtnClick(Object sender, EventArgs e)
        {
            PubPrintMemo pt = new PubPrintMemo(this.memo);
            pt.ShowPageSetup();
            pt.ShowPrintDialog();
        }
    }
    class PubPrintTextMemoDoc : System.Drawing.Printing.PrintDocument
    {
        RichTextBox rtb;
        Font printFont;
        int lineIndex = 0;
        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            //  base.OnPrintPage(e);
            float linesPerPage = 0;
            float yPos = 0;
            float leftMargin = e.MarginBounds.Left;
            float topMargin = e.MarginBounds.Top;
            int count = 0;
            linesPerPage = e.MarginBounds.Height / printFont.GetHeight(e.Graphics);
            while (count < linesPerPage && lineIndex < rtb.Lines.Length)
            {
                yPos = topMargin + (count * printFont.GetHeight(e.Graphics));

                e.Graphics.DrawString(rtb.Lines[lineIndex], printFont, Brushes.Black,

                leftMargin, yPos, new StringFormat());

                count++;
                lineIndex++;
            }

            if (lineIndex < rtb.Lines.Length)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
            }

            if (lineIndex >= rtb.Lines.Length) lineIndex = 0;

        }
        public PubPrintTextMemoDoc(RichTextBox rtb) : base()
        {

            printFont = new System.Drawing.Font("細明體_HKSCS", 10);
            this.rtb = rtb;
        }
        ~PubPrintTextMemoDoc()
        {
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
    class PubPrintMemo
    {
        private int reptdefpageindex = 0;
        private PageSettings oPageSettings;
        public System.Drawing.Printing.PrintDocument printDoc;
        private System.Windows.Forms.PrintPreviewDialog ppDialog;
        private System.Windows.Forms.PrintDialog oPrintDialog;
        private System.Windows.Forms.PageSetupDialog oPageSetup;
        public PubPrintMemo(RichTextBox rtb)
        {
            this.printDoc = new PubPrintTextMemoDoc(rtb);
            this.ppDialog = new System.Windows.Forms.PrintPreviewDialog();
            this.oPrintDialog = new System.Windows.Forms.PrintDialog();
            this.oPageSetup = new System.Windows.Forms.PageSetupDialog();
            oPageSettings = new PageSettings();
            this.oPrintDialog.AllowSomePages = true;
        }
        public void ShowPrintDialog()
        {
            oPrintDialog.Document = printDoc;
            if (oPrintDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    oPageSettings.PrinterSettings = printDoc.PrinterSettings;
                    printDoc.DefaultPageSettings = oPageSettings;
                    ppDialog.Document = printDoc;
                    ppDialog.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        public void ShowPageSetup()
        {
            oPageSettings.Margins.Top = 30;
            oPageSetup.PageSettings = oPageSettings;
            if (oPageSetup.ShowDialog() == DialogResult.OK)
                oPageSettings = oPageSetup.PageSettings;
        }
        public void ShowPageSetup(int MarginTop, int MarginLeft)
        {
            oPageSettings.Margins.Top = MarginTop;
            oPageSettings.Margins.Left = MarginLeft;
            oPageSetup.PageSettings = oPageSettings;
            if (oPageSetup.ShowDialog() == DialogResult.OK)
                oPageSettings = oPageSetup.PageSettings;
        }
    }
}
