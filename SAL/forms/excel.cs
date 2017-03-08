using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SAL.forms
{
    class excel
    {
    }
    public class PubExcel
    {
        protected bool oVisible = false;
        public const int MaxColumnNumber = 230;
        Boolean datetimefield_Str_flag = true;
        protected Microsoft.Office.Interop.Excel.Application oexcel;
        protected Microsoft.Office.Interop.Excel.Workbook obook;
        protected Microsoft.Office.Interop.Excel.Worksheet osheet;

        [DllImport("User32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        private static string convertIndex2ABC(int x, int y)
        {
            if (x > MaxColumnNumber)
                throw new Exception("Excel worksheet 長度超出" + MaxColumnNumber + ",即是column 編號超出IV");
            char c;

            c = (char)(65 + ((x - 1) % 26));
            string res = c.ToString() + y.ToString();
            int preFixNo = (x - 1) / 26;
            if (preFixNo > 0)
            {
                res = ((char)(65 + preFixNo - 1)).ToString() + res;
            }

            return res;
        }
        public string sheetname;
        public string readCell(int x, int y)
        {
            string r = osheet.Cells[x, y].Value.ToString();
            //.Value2.ToString();
            return r;


        }
        /*
         * Excel.Range oRng = (Excel.Range)oWS.get_Range("A1", "B4");
         * object[,] data = (object[,])oRng.Value2;
         * for(int i=1; i<=data.GetUpperBound(0); i++)
            {
              for(int j = 1; j <= data.GetUpperBound(1); j++)
              {
                    Console.WriteLine("(" + i + "," + j + ") = " + data[i,j]);
                    data[i,j] = "NEW" + data[i,j];
               }
            }
        */
        public Object[,] readCell(int x, int y, int x1)
        {
            Microsoft.Office.Interop.Excel.Range oRng = (Microsoft.Office.Interop.Excel.Range)osheet.get_Range(convertIndex2ABC(x, y), convertIndex2ABC(x1, y));

            Object[,] data = (Object[,])oRng.Value2;
            return data;

        }
        public Object[,] readCell(int x, int y, int x1, int y1)
        {
            Microsoft.Office.Interop.Excel.Range oRng = (Microsoft.Office.Interop.Excel.Range)osheet.get_Range(convertIndex2ABC(x, y), convertIndex2ABC(x1, y1));
            Object[,] data = (Object[,])oRng.Value2;
            return data;

        }

        /* 1990/01/01 1990/01
         * 在匯出時前加 '
         * 
         */
        public bool VerifyDateStrFormat(string context)
        {
            if (context.Length <= 10 && System.Text.RegularExpressions.Regex.Match(context, @"\d+/\d+/\d+").Success)
            {
                return true;
            }
            else if (context.Length <= 7 && System.Text.RegularExpressions.Regex.Match(context, @"\d+/\d+").Success)
            {
                return true;
            }
            /*            if (context.Length <= 20 && Regex.Match(context, @"\d+/\d+/\d+ \d+:\d+:\d+").Success)
                        {
                            return true;
                        }
            */
            else
            {
                return false;

            }
        }

        public void writeCell(int x, int y, string context)
        {
            if (VerifyDateStrFormat(context))
            { osheet.get_Range(convertIndex2ABC(x, y), Type.Missing).Value2 = "," + context; }
            else
            { osheet.get_Range(convertIndex2ABC(x, y), Type.Missing).Value2 = context; }

        }
        /*Excel.Range range = wks.get_Range("A1", "C10");
            Object[,] arr = new Object[10,3];
            for (int i = 0; i < 10; i++)
            {
                arr[i,0] = "test";
                arr[i,1] = i;
                arr[i,2] = 5.653;
            }
            range.set_Value(Missing.Value,arr);
         */
        public void writeCell(int x, int y, int x1, int y1, Object[] context)
        {
            // MessageBox.Show(x1+" "+y1+context.ToString());

            osheet.get_Range(convertIndex2ABC(x, y), convertIndex2ABC(x1, y1)).Value2 = context;
        }
        public bool saveTo(string filename, bool alert)
        {
            bool r = false;
            try
            {
                oexcel.DisplayAlerts = alert;
                osheet.SaveAs(filename);
                /*
                    Microsoft.Office.Interop.Excel.XlFileFormat.xlExcel9795, Type.Missing, Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                 */
                oexcel.DisplayAlerts = true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            r = true;
            return r;

        }
        public bool Visible
        {
            get
            {

                return oVisible;
            }

            set
            {
                oexcel.Visible = value;
                oVisible = value;
            }
        }
        public PubExcel()
        {
            newFile();
        }
        public PubExcel(bool DateTimeFlag)
        {
            datetimefield_Str_flag = DateTimeFlag;
            newFile();

        }
        /*
         *同時打開某檔案 
         */
        public PubExcel(string filename, string sheetname)
        {

            this.sheetname = sheetname;

            oexcel = new Microsoft.Office.Interop.Excel.Application();
            obook = oexcel.Workbooks.Open(filename);
            foreach (Microsoft.Office.Interop.Excel.Worksheet displayWorksheet in oexcel.Worksheets)
            {
                if (sheetname.ToUpper().Equals("SHEET1")) { }
                if (displayWorksheet.Name.Equals("工作表1")) { sheetname = "工作表1"; }
            }
            osheet = (Microsoft.Office.Interop.Excel.Worksheet)obook.Sheets[sheetname];
            //osheet = (Microsoft.Office.Interop.Excel.Worksheet)obook.Worksheets[sheetname];
        }

        public PubExcel(string filename, string sheetname, bool DateTimeFlag)
        {
            datetimefield_Str_flag = DateTimeFlag;
            this.sheetname = sheetname;
            oexcel = new Microsoft.Office.Interop.Excel.Application();
            obook = oexcel.Workbooks.Open(filename);
            osheet = (Microsoft.Office.Interop.Excel.Worksheet)obook.Worksheets[sheetname];
        }

        ~PubExcel()
        {
        }
        //-------
        //新增XLS檔案
        //-------
        public void newFile()
        {
            sheetname = "sheet1";
            Microsoft.Office.Interop.Excel.XlWBATemplate wba = Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet;
            oexcel = new Microsoft.Office.Interop.Excel.Application();
            obook = oexcel.Workbooks.Add(wba);
            osheet = (Microsoft.Office.Interop.Excel.Worksheet)obook.Worksheets.get_Item(1);
            sheetname = osheet.Name;

        }

        void initailExcel()
        {
            //檢查PC有無Excel在執行
            bool flag = false;
            foreach (var item in System.Diagnostics.Process.GetProcesses())
            {
                if (item.ProcessName == "EXCEL")
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                this.oexcel = new Microsoft.Office.Interop.Excel.Application();
            }
            else
            {
                object obj = System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");//引用已在執行的Excel
                oexcel = obj as Microsoft.Office.Interop.Excel.Application;
            }
            this.oexcel.Visible = true;//設false效能會比較好
        }
        public void closeExcel()
        {
            oexcel.DisplayAlerts = false;
            oexcel.Quit();//main
            oexcel.DisplayAlerts = true;
            IntPtr t = new IntPtr(oexcel.Hwnd);
            int k = 0;
            GetWindowThreadProcessId(t, out k);
            System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(k);
            p.Kill();
            oexcel = null;//main

        }
        public void ExportXls(System.Data.DataTable dt, System.Collections.Hashtable dict)
        {
            String[] a = new String[dt.Columns.Count];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (dict != null)
                {
                    if (dict.Contains(dt.Columns[i].ColumnName.ToLower()))
                    {
                        a[i] = dict[dt.Columns[i].ColumnName.ToLower()].ToString();
                    }
                    else
                    {
                        a[i] = dt.Columns[i].ColumnName.ToString();
                    }
                }
                else { a[i] = dt.Columns[i].ColumnName; }
            }
            writeCell(1, 1, dt.Columns.Count, 1, a);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (dt.Columns[j].DataType.Equals(System.Type.GetType("System.DateTime")) && datetimefield_Str_flag)
                    {
                        a[j] = "'" + dt.Rows[i][j].ToString();
                    }
                    else if (VerifyDateStrFormat(dt.Rows[i][j].ToString()))
                    {
                        a[j] = "'" + dt.Rows[i][j].ToString();
                    }
                    else
                    {
                        a[j] = dt.Rows[i][j].ToString();
                    }
                }
                writeCell(1, i + 2, dt.Columns.Count, i + 2, a);
            }
        }


        public virtual void ImportXls(System.Data.DataTable dt)
        {
            int rowcnt = 1;
            string keyfield = dt.Columns[0].Caption;
            Object[,] data = readCell(1, ++rowcnt, dt.Columns.Count);
            string tmp = "";
            while (data[1, 1] != null)
            {
                tmp += data[1, 1].ToString() + "{}";

                System.Data.DataRow[] dr = dt.Select(keyfield + "='" + data[1, 1].ToString() + "'");
                if (dr != null && dr.Length > 0)
                {
                    dr[0].BeginEdit();
                    for (int i = 0; i < data.GetUpperBound(1); i++)
                    {
                        if (data[1, i + 1] != null)
                        {
                            dr[0][i] = data[1, i + 1];
                        }
                    }

                    dr[0].EndEdit();

                }
                else
                {
                    System.Data.DataRow r = dt.NewRow();
                    r.BeginEdit();
                    for (int i = 0; i < data.GetUpperBound(1); i++)
                    {

                        if (data[1, i + 1] != null)
                        {
                            r[i] = data[1, i + 1];
                        }
                    }
                    r.EndEdit();
                    dt.Rows.Add(r);

                }
                data = readCell(1, ++rowcnt, dt.Columns.Count);
            }
        }
    }
}