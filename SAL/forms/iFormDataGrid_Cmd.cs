using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAL.forms
{

    public class FormDataGrid_Cmd_Importxls : FormDataGrid_Cmd
    {
        public FormDataGrid_Cmd_Importxls(iFormGrid idg, Hashtable adict, BindingListOptions bloption)
            : base(idg, adict, bloption)
        {
            bindingNavigator1.Items.Add("importxls", null, tslImportXls_Click);
        }

        private void tslImportXls_Click(object sender, EventArgs e)
        {

            OpenFileDialog of = new OpenFileDialog();
            of.FileName = "*.XLS";
            if (of.ShowDialog() == DialogResult.OK)
            {
                idg.Importxls(of.FileName, dict);
            }
        }
    }
    [Flags]
    public enum BindingListOptions
    {
        AllowNewNo = 0x01,
        AllowModifyNo = 0x02,
        AllPri = 0x03
    }
    public interface iFormGrid
    {
        void submitAddRow(DataRow aRow);
        void submitDeleteRow(DataRow aRow);
        void submitModifyRow(DataRow aRow);
        int submitUpdate();
        void Exportxls(String filename, Hashtable dict);
        void Importxls(String filename, Hashtable dict);
        DataTable GetDT();
        void FillDT();
        bool GetRowChanging();
        void FillDT_Next_int();
        void AddNumOfRows();
        void Calc();
        void CalcTotal();
        void PritnSALForm();
    }

   
    public class iSqlCmdFormGrid : iFormGrid
    {
        public DbDataAdapter adapter;
        public DataSet ds;
        public DataTable dt;
        public string submitUpdateError = null;
        protected string c_sql;
        protected string c_keyfield;
        protected string tablename;
        protected int preDBRecCNT = 0;
        private bool rowchanging = false;
        public iSqlCmdFormGrid(DbDataAdapter pAdapter, DbCommand selcmd, DbCommand updcmd, DbCommand insertcmd, DbCommand delcmd)
        {
            this.adapter = pAdapter;
            if (selcmd != null) adapter.SelectCommand = selcmd;
            if (updcmd != null) adapter.UpdateCommand = updcmd;
            if (insertcmd != null) adapter.InsertCommand = insertcmd;
            if (delcmd != null) adapter.DeleteCommand = delcmd;
            adapter.TableMappings.Add("Table", "tablename");
            dt = new DataTable();
            adapter.Fill(dt);
            preDBRecCNT = dt.Rows.Count;
            this.dt.RowChanging += new DataRowChangeEventHandler(dt_RowChanging);
        }

        void dt_RowChanging(object sender, DataRowChangeEventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
            rowchanging = true;
        }
        public bool GetRowChanging()
        {
            return rowchanging;
        }
        public void FillDT()
        {
            adapter.Fill(dt);
        }
        public void FillDT_Next_int()
        {
            string keyfield=dt.Columns[0].ColumnName;
            int key = 0;
            foreach( DataRow r  in this.dt.Rows)
            {
                if (key < (int)r[keyfield]) key = (int)r[keyfield];
            }
            if (adapter.SelectCommand.CommandText.Contains("where")) {
                adapter.SelectCommand.CommandText = adapter.SelectCommand.CommandText + " and " + keyfield + ">" + key;
            }
            else
            {
                adapter.SelectCommand.CommandText = adapter.SelectCommand.CommandText + " where " + keyfield + ">" + key;
            }
            adapter.Fill(dt);
        }
        public iSqlCmdFormGrid(DbDataAdapter pAdapter) : this(pAdapter, null, null, null, null)
        {
        }
        public DataTable GetDT() { return dt; }
        public void submitAddRow(DataRow aRow)
        {
            DataRow[] drA = { aRow };
            try
            {
                adapter.Update(drA);
                aRow.AcceptChanges();
            }
            catch (Exception e1)
            {
                submitUpdateError += e1.Message;
            }
        }
        public void submitModifyRow(DataRow aRow)
        {
            DataRow[] drA = { aRow };
            try
            {
                adapter.Update(drA);
                aRow.AcceptChanges();
            }
            catch (Exception e1)
            {
                submitUpdateError += e1.Message;
            }
        }
        public void submitDeleteRow(DataRow aRow)
        {
            DataRow[] drA = { aRow };
            try
            {
                adapter.Update(drA);
                aRow.AcceptChanges();
            }
            catch (Exception e1)
            {
                if (e1.Message.Equals("Cannot perform this operation on a row not in the table.") ||
                    e1.Message.Equals("無法對不存在於資料表中的資料列上執行這項作業。"))
                {
                }
                else
                {
                    submitUpdateError += e1.Message;
                }
            }
        }
        public int submitUpdate()
        {
            submitUpdateError = null;
            int cnt = 0;
            int updatecnt = 0;

            //dt.AcceptChanges();
            //IEnumerator rowEnum = dt.Rows.GetEnumerator();

            List<DataRow> delRows = new List<DataRow>();
            foreach (DataRow currRow in dt.Rows)            //while (rowEnum.MoveNext())
            {
                //  DataRow currRow = (DataRow)rowEnum.Current;
                switch (currRow.RowState)
                {
                    case DataRowState.Unchanged: break;
                    case DataRowState.Added: submitAddRow(currRow); cnt++; updatecnt++; break;
                    case DataRowState.Modified: submitModifyRow(currRow); cnt++; break;
                    case DataRowState.Deleted: delRows.Add(currRow); cnt++; break;
                    default:
                        try
                        {
                            DataRow[] drA = { currRow };
                            adapter.Update(drA);
                            cnt++;
                            currRow.AcceptChanges();
                        }
                        catch (Exception e1)
                        {
                            MessageBox.Show(e1.Message);
                        }
                        cnt++;
                        break;
                }
            }
            foreach (DataRow currRow in delRows)
            {
                submitDeleteRow(currRow); cnt++;
            }
            if (submitUpdateError != null)
            {
                PubInfoBox info = new PubInfoBox();
                info.memo.Text = submitUpdateError;
                info.ShowDialog();
            }
            /*
           else
           {
               DialogResult reply = MessageBox.Show("更新完成, 是否從伺服重新載入資料?",
                           "Yes or No ", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
               if (reply == DialogResult.Yes && updatecnt > 0)
               {
                   //preDBRecCNT
                   //      adapter.Fill(ds, preDBRecCNT, cnt, tablename);
               }
               else
               {
                   //MessageBox.Show("No");
               }
            
           }*/
            //dt.AcceptChanges();
            return cnt;
        }
        public void Exportxls(String filename, Hashtable dict)
        {
            if (dt.Columns.Count > 127)
            {
                MessageBox.Show("欄位過長,超過127,現在欄位為" + dt.Columns.Count);
                return;
            }
            bool datetimefield_Str_flag = true;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (dt.Columns[i].DataType.Equals(System.Type.GetType("System.DateTime")))
                {
                    if (MessageBox.Show("日期欄位匯出時文字表示,是; 否則用數字表示", "日期欄位", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        datetimefield_Str_flag = false;
                    }
                }
            }
            PubExcel el = new PubExcel(datetimefield_Str_flag);
            el.Visible = true;
            el.ExportXls(dt, dict);
            el.saveTo(filename, true);
            el.closeExcel();
        }
        public virtual void Importxls(string filename, Hashtable dict)
        {
            PubExcel el = new PubExcel(filename, "sheet1");
            el.Visible = true;
            el.ImportXls(dt);
            el.closeExcel();
        }
        private Type _eh_addnumofrows_type = null;
        private String _eh_addnumofrows_function = null;
        private Object  _eh_addnumofrows_object = null;
        public void AddNumOfRows_EventHandler(Object o,Type t,String f)
        {
            _eh_addnumofrows_type = t;
            _eh_addnumofrows_function = f;
            _eh_addnumofrows_object = o;
        }
        public virtual void AddNumOfRows()
        {
            if(_eh_addnumofrows_function != null)
            {
                System.Reflection.MethodInfo mi = _eh_addnumofrows_type.GetMethod(_eh_addnumofrows_function);
                if (mi != null)
                {
                    object[] args = new object[2];
                    args[0] = Pub.cfg.curr_userinfo.MainForm;
                    args[1] = new EventArgs();
                    mi.Invoke(_eh_addnumofrows_object, args);
                }
                else { MessageBox.Show("未登記!"); }
            }
        }
        public virtual void Calc() {  }
        public virtual void CalcTotal() { }
        public virtual void PritnSALForm() { }
    }
    public class PubInfoBox : Form
    {
        public RichTextBox memo;
        private Button btn;
        private Button prtBTN;
        private void btnClick(Object sender, EventArgs e)
        {
            Close();
        }
        private void prtbtnClick(Object sender, EventArgs e)
        {
            //  PubPrintMemo pt = new PubPrintMemo(memo);
            //  pt.ShowPageSetup();
            //  pt.ShowPrintDialog();
        }
        public PubInfoBox()
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
        public PubInfoBox(Form parentForm)
            : this()
        {
            this.MdiParent = parentForm;
        }
    }
    public class ShellExec
    {
        [DllImport("shell32.dll")]
        public static extern int ShellExecute(IntPtr hwnd, StringBuilder lpszOp, StringBuilder lpszFile, StringBuilder lpszParams, StringBuilder lpszDir, int FsShowCmd);
    }
    public interface idgvEditCP
    {
        void copyGrid_Click(DataGridView dataGridView1);
        void PasteClipboard(DataGridView dataGridView1);
    }
    public class dgvEditCP : idgvEditCP
    {
        private static dgvEditCP _instance = null;
        public static dgvEditCP GetInst
        {
            get
            {
                if (_instance == null) _instance = new dgvEditCP();
                return _instance;
            }
        }
        public void copyGrid_Click(DataGridView dataGridView1)
        {
            //throw new NotImplementedException();
            if (dataGridView1.GetCellCount(System.Windows.Forms.DataGridViewElementStates.Selected) > 0)
            {
                try
                {
                    // Add the selection to the clipboard.
                    System.Windows.Forms.Clipboard.SetDataObject(
                        dataGridView1.GetClipboardContent());
                    // Replace the text box contents with the clipboard text.
                    //this.TextBox1.Text = Clipboard.GetText();
                }
                catch (System.Runtime.InteropServices.ExternalException ee)
                {
                    MessageBox.Show("The Clipboard could not be accessed. Please try again."+ee.Message);
                }
            }
        }
        public void PasteClipboard(DataGridView dataGridView1)
        {
            try
            {
                string s = Clipboard.GetText();
                MessageBox.Show(s);
                string[] lines = s.Split('\n');
                int iFail = 0, iRow = dataGridView1.CurrentCell.RowIndex;
                int iCol = dataGridView1.CurrentCell.ColumnIndex;
                DataGridViewCell oCell;
                foreach (string line in lines)
                {
                    if (iRow < dataGridView1.RowCount && line.Length > 0)
                    {
                        string[] sCells = line.Split('\t');
                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            if (iCol + i < dataGridView1.ColumnCount)
                            {
                                oCell = dataGridView1[iCol + i, iRow];
                                if (!oCell.ReadOnly)
                                {
                                    //if (oCell.Value.ToString() != sCells[i])
                                    //{
                                    
                                    try{
                                        Decimal od;
                                        if (oCell.ValueType.ToString().ToLower().Contains("decimal"))
                                        {
                                            if (Decimal.TryParse(sCells[i].ToString(), out od)) {
                                                oCell.Value = Convert.ChangeType(od, oCell.ValueType);
                                                oCell.Style.BackColor = Color.Tomato;
                                            }
                                            else { oCell.Value = DBNull.Value; ; }
                                        } else {
                                            oCell.Value = Convert.ChangeType(sCells[i], oCell.ValueType);
                                            oCell.Style.BackColor = Color.Tomato;
                                        }
                                    }
                                    catch{
                                        oCell.Value =DBNull.Value; 
                                    }
                                    //}
                                    //else
                                    //  iFail++;
                                    //only traps a fail if the data has changed 
                                    //and you are pasting into a read only cell
                                }
                            }
                            else
                            { break; }
                        }
                        iRow++;
                    }
                    else
                    { break; }
                    if (iFail > 0)
                        MessageBox.Show(string.Format("{0} updates failed due" +
                                        " to read only column setting", iFail));
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("The data you pasted is in the wrong format for the cell");
                return;
            }
        }
    }
}
