﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SAL.forms;
using System.Data.SQLite;
using System.Data;
using SAL.salcalc;

namespace SAL
{
    class SystemPrivilegeMngDef
    {
        public static SystemPrivilegeMng GetInstance(int index, Form Sender, string subSysPrivilege)
        {
            switch (index)
            {
                case 2: return new SalPriMng(Sender, subSysPrivilege);
                default: return null;
            }
        }
    }
    class SalPriMng : SystemPrivilegeMng
    {
        public string title = "薪資管理";
        public SalPriMng(Form Sender, string subSysPrivilege)
            : base(Sender, subSysPrivilege)
        {
            sidebaritem = new SidebarItem();
            sidebaritem.title = "管理";
            sidebaritem.subitems = new List<SidebarItem_Struct>();
            sidebaritem.subitems.Add(new SidebarItem_Struct("連接/創建DB", "createOrLinkSqlite", 2));
            sidebaritem.subitems.Add(new SidebarItem_Struct("編輯津貼分類", "editAlloType", 13));
            sidebaritem.subitems.Add(new SidebarItem_Struct("增加津貼欄位", "addrowAlloType", 13));
            sidebaritem.subitems.Add(new SidebarItem_Struct("編輯基本記錄", "editSalRec", 13));
            sidebaritem.subitems.Add(new SidebarItem_Struct("增加記錄", "addrowSalRec", 13));
            sidebaritem.t = RD_SalPriMng.GetInstance(this.ParentForm);
            sidebaritem.atype = Type.GetType("SAL.RD_SalPriMng");
            sidebaritem.next = new List<SidebarItem>();
            SidebarItem subsi = new SidebarItem();
            subsi.title = "檢視";
            subsi.subitems = new List<SidebarItem_Struct>();
            subsi.subitems.Add(new SidebarItem_Struct("檢視總表", "editSalAlloRec", 0));
 subsi.subitems.Add(new SidebarItem_Struct("月結總表", "editSalAlloRec_Title", 0));
	    subsi.subitems.Add(new SidebarItem_Struct("計算公式", "EditCalcRule_Click", 5));
            subsi.subitems.Add(new SidebarItem_Struct("報表", "Test_Click", 6));
            subsi.t = RD_SalPriMng.GetInstance(this.ParentForm);
            subsi.atype = Type.GetType("SAL.RD_SalPriMng");
            sidebaritem.next.Add(subsi);
        }
        public override void AddMenu(MainMenu mm)
        {
            
	    ss_mi = mm.MenuItems.Add("3.薪資管理");
            int lenPrivilege = this.subSysPrivilege.Length;
            if (lenPrivilege == 0) return;
            RD_SalPriMng rD_SalPriMng = RD_SalPriMng.GetInstance(this.ParentForm);
            ss_mi.MenuItems.Add("3.1.創建DB(SQL_YM.Sqlite)", rD_SalPriMng.createOrLinkSqlite);
            ss_mi.MenuItems.Add("-");
            ss_mi.MenuItems.Add("3.2.編輯津貼分類", rD_SalPriMng.editAlloType);
            ss_mi.MenuItems.Add("3.3.增加記錄(津貼分類)", rD_SalPriMng.addrowAlloType);
            ss_mi.MenuItems.Add("-");
            ss_mi.MenuItems.Add("3.4.檢視基本表格", rD_SalPriMng.editSalRec);
            ss_mi.MenuItems.Add("3.5.增加記錄(基本表格)", rD_SalPriMng.addrowSalRec);
            ss_mi.MenuItems.Add("-");
            ss_mi.MenuItems.Add("3.6.檢視總表", rD_SalPriMng.editSalAlloRec);
            ss_mi.MenuItems.Add("3.7.每月數據", rD_SalPriMng.editSalAlloRec_Title);
            ss_mi.MenuItems.Add("-");
            ss_mi.MenuItems.Add("3.8.編輯津貼列表", rD_SalPriMng.editAllo);
            ss_mi.MenuItems.Add("-");
            ss_mi.MenuItems.Add("3.9.CalcRule", rD_SalPriMng.EditCalcRule_Click);
            
        }
    }
    public class RD_SalPriMng
    {
        public Form MDIparent;
        internal static RD_SalPriMng instance=null;
        Hashtable sl_baseinfohs = new Hashtable();
        public static RD_SalPriMng GetInstance(Form parentRef)
        {
            if (instance == null)
            {
                instance = new RD_SalPriMng(parentRef);
            }
            instance.MDIparent = parentRef;
            return instance;
        }

        public RD_SalPriMng(Form parentRef)
        {
            this.MDIparent = parentRef;
            StreamReader sr = new StreamReader(Pub.cfg.getAppPath() + @"\SA_baseinfo.dat", Encoding.Default);
            string line = null;
            string[] astr = null;
            while ((line = sr.ReadLine()) != null)
            {
                try
                {
                    astr = line.Split('=');
                    if (!sl_baseinfohs.ContainsKey(astr[0].ToUpper()))
                        sl_baseinfohs.Add(astr[0].ToUpper(), astr[1]);
                }
                catch
                {
                    MessageBox.Show("RD_SalPriMng:" + astr[0]);
                }
            }
            sr.Close();
        }
        public void createOrLinkSqlite(Object sender, EventArgs e)
        {
            //MessageBox.Show("createSqlite");
            string olddbname = Pub.cfg.dbname;
            string newdbname = forms.utls.prompt("create or link db", "dbname", Pub.cfg.dbname);
            if(newdbname!=null)
            {
                Pub.cfg.dbname = newdbname;
                AppSettingsConfigure.SetVal("dbname",newdbname);
                message_center.tssl_dbname(newdbname);
                using (DbConnection dbconn = Pub.cfg.getDBConn())
                {
                    dbconn.Open();//testing
                    dbconn.Close();
                }
            }
        }
        public void editAlloType(Object sender, EventArgs e)
        {
            string sql="select * from sl_allotype";
            System.Data.SQLite.SQLiteParameter[] spars = { };
            string u_sql = "update sl_allotype set AlloType=?,AlloTypeC=?,AlloTypeE=?,Tab=?,fv=?,base=?,flag=? where allotypeid=@allotypeid";           
            System.Data.SQLite.SQLiteParameter[] u_pars = {
                 new SQLiteParameter("@AlloType", DbType.String,"AlloType"),
                 new SQLiteParameter("@AlloTypeC", DbType.String,"AlloTypeC"),
                 new SQLiteParameter("@AlloTypeE", DbType.String,"AlloTypeE"),
                 new SQLiteParameter("@Tab", DbType.Int32,4,"Tab"),
                 new SQLiteParameter("@fv", DbType.Int32,4,"fv"),
                 new SQLiteParameter("@base", DbType.Decimal,10,"base"),
                 new SQLiteParameter("@flag", DbType.Int32,4,"flag"),
                 new SQLiteParameter("@allotypeid", DbType.Int32,4, "allotypeid")
            };
            string d_sql = "delete  from sl_allotype where allotypeid=@oallotypeid";
            System.Data.SQLite.SQLiteParameter[] d_pars = {
                 new SQLiteParameter("@oallotypeid", DbType.Int32,4, "allotypeid", DataRowVersion.Original)
            };
            iSqlCmdFormGrid idg = new iSqlCmdFormGrid(ESData.GetInst.GetAdapter(),
               ESData.GetInst.GetSQLiteCommand(sql, spars),
               ESData.GetInst.GetSQLiteCommand(u_sql, u_pars),
                null,
                ESData.GetInst.GetSQLiteCommand(d_sql, d_pars));
            idg.AddNumOfRows_EventHandler(this,Type.GetType("SAL.RD_SalPriMng"), "addrowAlloType");
            FormDataGrid_Cmd fg = new FormDataGrid_Cmd(idg,
                ESData.GetInst.dict,
                forms.BindingListOptions.AllowNewNo);
            fg.FrozenLeftColumns(1, true);
            
            fg.MdiParent = Pub.cfg.curr_userinfo.MainForm;
            fg.Text = "(modify)" + sql;
            fg.Show();
        }
        public void editAllo(Object sender, EventArgs e)
        {
            ESData.GetInst.Open_Conn();
            string sql = "select al_id,staf_ref,YM,allotype,amount,taxa,note from sl_allo";
            System.Data.SQLite.SQLiteParameter[] spars = { };
            string u_sql = "update sl_allo set staf_ref=?,alloType=?,amount=?,taxa=?,note=? where al_id=@al_id";
            System.Data.SQLite.SQLiteParameter[] u_pars = {
                 new SQLiteParameter("@staf_ref", DbType.String,"staf_ref"),
                 new SQLiteParameter("@alloType", DbType.String,"alloType"),
                 new SQLiteParameter("@amount", DbType.Decimal,10,"amount"),
                 new SQLiteParameter("@taxa", DbType.Int32,4,"taxa"),
                 new SQLiteParameter("@note", DbType.String,"note"),
                 new SQLiteParameter("@al_id", DbType.Int32,4, "al_id")
            };
            string d_sql = "delete  from sl_allo where al_id=@oal_id";
            System.Data.SQLite.SQLiteParameter[] d_pars = {
                 new SQLiteParameter("@oal_id", DbType.Int32,4, "al_id", DataRowVersion.Original)
            };
            FormDataGrid_Cmd fg = new FormDataGrid_Cmd(new iSqlCmdFormGrid(ESData.GetInst.GetAdapter(),
               ESData.GetInst.GetSQLiteCommand(sql, spars),
               ESData.GetInst.GetSQLiteCommand(u_sql, u_pars),
                null,
                ESData.GetInst.GetSQLiteCommand(d_sql, d_pars)),
                ESData.GetInst.dict,
                forms.BindingListOptions.AllowNewNo);
            fg.FrozenLeftColumns(1, true);
            fg.MdiParent = Pub.cfg.curr_userinfo.MainForm;
            fg.Text = "(modify)" + sql;
            fg.Show();
        }
        public void addrowAlloType(Object sender, EventArgs e)
        {
           string str =forms.utls.prompt("sl_allotype","Add Num Of Rows:","0");
           string pat = @"^\d+$";
            if (str != null && Regex.IsMatch(str, pat)) {
                int cnt = 0;
                int efcnt = 0;
                ESData.GetInst.Open_Conn();
                using (SQLiteDataReader dr = new SQLiteCommand("select max(allotypeid) from sl_allotype;", (SQLiteConnection)ESData.GetInst.conn).ExecuteReader())
                {
                    if (dr.Read())
                        if (!dr.IsDBNull(0))
                            cnt = dr.GetInt32(0);
                }
                for(int i=0;i<int.Parse(str);i++)
                using (SQLiteCommand cmd = new SQLiteCommand("insert into sl_allotype(allotypeid,allotype,allotypec,allotypee,tab,fv,base)values(?,?,?,?,?,?,?);", (SQLiteConnection)ESData.GetInst.conn))
                {
                    cnt++;
                    cmd.Parameters.AddWithValue("@allotypeid", cnt);
                    cmd.Parameters.AddWithValue("@allotype", "type_" + cnt);
                    cmd.Parameters.AddWithValue("@allotypec", "type_" + cnt);
                    cmd.Parameters.AddWithValue("@allotypee", "type_" + cnt);
                    cmd.Parameters.AddWithValue("@tab", 99);
                    cmd.Parameters.AddWithValue("@fv", 99);
                    cmd.Parameters.AddWithValue("@base", 0.0m);
                    efcnt += cmd.ExecuteNonQuery();
                }
                forms.utls.alert(string.Format("insert {0} maxid{1}rec,do it!" ,efcnt, cnt ) );
            }
        }

        public void editAlloCollum(Object sender, EventArgs e)
        {
            MessageBox.Show("editAlloCollum");
        }
        public void editSalRec(Object sender, EventArgs e)
        {
            string sql = "select * from sl_rec";
            SQLiteParameter[] spars = { };
            StringBuilder u_sql = new StringBuilder();
            u_sql.Append("update sl_rec set ");
            for(int i = 1; i < sl_pub.SlRecFields.Count; i++)
            {
                u_sql.Append(String.Format("{0}=?", sl_pub.SlRecFields[i].FieldName));
                if (i < (sl_pub.SlRecFields.Count - 1)) u_sql.Append(",");
            }
            u_sql.Append(" where sa_id=@sa_id");
            SQLiteParameter[] u_pars = new SQLiteParameter[sl_pub.SlRecFields.Count];
            for (int i = 1; i < sl_pub.SlRecFields.Count; i++)
                u_pars[i - 1] = new SQLiteParameter("@" + sl_pub.SlRecFields[i].FieldName, sl_pub.SlRecFields[i].FieldType, sl_pub.SlRecFields[i].Size, sl_pub.SlRecFields[i].FieldName);
            u_pars[sl_pub.SlRecFields.Count - 1] = new SQLiteParameter("@sa_id", DbType.Int32, 4, "sa_id");

            string d_sql = "delete  from sl_rec where sa_id=@sa_id";
            SQLiteParameter[] d_pars = {
                 new SQLiteParameter("@sa_id", DbType.Int32,4, "sa_id", DataRowVersion.Original)
            };
            FormDataGrid_Cmd fg = new FormDataGrid_Cmd(new iSqlCmdFormGrid(ESData.GetInst.GetAdapter(),
               ESData.GetInst.GetSQLiteCommand(sql, spars),
               ESData.GetInst.GetSQLiteCommand(u_sql.ToString(), u_pars),
                null,
                ESData.GetInst.GetSQLiteCommand(d_sql, d_pars)),
                ESData.GetInst.dict,
                forms.BindingListOptions.AllowNewNo);
            fg.FrozenLeftColumns(1, true);
            fg.MdiParent = Pub.cfg.curr_userinfo.MainForm;
            fg.Text = "(modify)" + sql;
            fg.Show();
        }
        public void addrowSalRec(Object sender, EventArgs e)
        {
            string str = forms.utls.prompt("sl_SalRec", "Add Num Of Rows:", "0");
            string pat = @"^\d+$";
            if (str != null && Regex.IsMatch(str, pat))
            {
                int cnt = 0;
                int efcnt = 0;
                // String dt = DateTime.Now.ToString("YMDhms");
                ESData.GetInst.Open_Conn();
                using (SQLiteDataReader dr = new SQLiteCommand("select max(sa_id) from sl_rec;", (SQLiteConnection)ESData.GetInst.conn).ExecuteReader())
                {
                    if (dr.Read())
                        if (!dr.IsDBNull(0))
                            cnt = dr.GetInt32(0);
                }
                for (int i = 0; i < int.Parse(str); i++)
                    using (SQLiteCommand cmd = new SQLiteCommand("insert into sl_rec(sa_id,Staf_ref,YM)values(?,?,?);", (SQLiteConnection)ESData.GetInst.conn))
                    {
                        cnt++;
                        cmd.Parameters.AddWithValue("@sa_id", cnt);
                        cmd.Parameters.AddWithValue("@Staf_ref", "STAF_" + cnt);
                        cmd.Parameters.AddWithValue("@YM", Pub.cfg.YM);
                        efcnt += cmd.ExecuteNonQuery();
                    }
                forms.utls.alert(string.Format("insert {0} maxid{1}rec,do it!", efcnt, cnt));
            }
        }
        public void editSalAlloRec(Object sender, EventArgs e)
        {
            FormDataGrid_Cmd fg = new FormDataGrid_Cmd(new iSqlCmdFormGrid_SalAlloRec(),
                ESData.GetInst.dict,
                forms.BindingListOptions.AllowNewNo);
            fg.FrozenLeftColumns(1, true);
            fg.MdiParent = Pub.cfg.curr_userinfo.MainForm;
            fg.Text = "(modify)" ;
            fg.Show();
        }
        public void editSalAlloRec_Title(Object sender, EventArgs e)
        {
            ESData.GetInst.Open_Conn();
            Hashtable _dict=new Hashtable();
            _dict.Add("acctItem_no","會計科目");
_dict.Add("acctItem","會計代號");
_dict.Add("bankacct","帳戶");
_dict.Add("transAcctno","過數帳戶");
_dict.Add("qu_no","QNO");
_dict.Add("salary_no","NO_");
_dict.Add("YM","YM");
_dict.Add("Staf_ref","職員編號");
_dict.Add("c_name","中文姓名");
_dict.Add("bankacctname","戶名");
_dict.Add("TGRADE","教師級別");
_dict.Add("Net_income","淨收入________");
_dict.Add("baseSalary","底薪________");
_dict.Add("SptPay","特別職務報酬_");
_dict.Add("FixExtraWorkPay","超時薪金");
_dict.Add("sumbaseSalary","總底薪________");
_dict.Add("Seniority","年資");
_dict.Add("F_Allowance","固定津貼總計");
_dict.Add("V_Allowance","非固定津貼總計");
_dict.Add("Teaching_subsidy_for_teacher_replacement","正規代課");
_dict.Add("Not_teaching_subsidy_for_teacher_replacement","自修管理");
_dict.Add("IncomeAmount","收入總數");
_dict.Add("PensionFund_withhold","學校0%基金");
_dict.Add("PensionFund_Amount","基金累積供款");
_dict.Add("FSS_Fee","扣保障基金");
_dict.Add("Leave_withhold","病事假扣薪");
_dict.Add("WithholdAmount","扣減總數");
_dict.Add("Adjust_tax","年度職業稅調整");
_dict.Add("AdjustAmount","調整總數");
_dict.Add("Taxable_Income","每月總收益(不含B0)");
_dict.Add("Tax","職業稅扣款");
_dict.Add("salary_period","描述");
_dict.Add("Note","Note");
_dict.Add("note1","note1");
_dict.Add("note2","note2");
_dict.Add("School_No","0學校編號");
_dict.Add("WorkStatus","工作狀態");
_dict.Add("WorkPosi","工作職位");
_dict.Add("WorkDept","工作部門");
_dict.Add("FSS_NO","社工局編號");
_dict.Add("TAX_NO","職業稅編號");
_dict.Add("schoolsect","學部");
_dict.Add("PensionFund_preAmount","PensionFund_preAmount");
_dict.Add("PensionFund_term_interest","PensionFund_term_interest");
_dict.Add("PensionFund_curr_YM","PensionFund_curr_YM");
_dict.Add("LeaveJob_Comp_PF","LeaveJob_Comp_PF");
_dict.Add("LeaveJob_Pers_PF","LeaveJob_Pers_PF");
   foreach(FieldDefSt fst     in sl_pub.FixAlloItem){
       if(!_dict.ContainsKey(fst.FieldName))
        _dict.Add(fst.FieldName,fst.FieldCName);
   }

            FormDataGrid_Cmd fg = new FormDataGrid_Cmd(new iSqlCmdFormGrid_SalAlloRec(),
                _dict,
                forms.BindingListOptions.AllowNewNo);
            fg.FrozenLeftColumns(1, true);
            fg.MdiParent = Pub.cfg.curr_userinfo.MainForm;
            fg.Text = "(modify)" ;
            fg.Show();
        }
        public static void ReOrderTableColumn(DataTable objDt, string[] strNewColumnsOrder)
        {
            objDt.PrimaryKey = null;
            for (int i = 0; i < strNewColumnsOrder.Length; i++)
                objDt.Columns[strNewColumnsOrder[i]].SetOrdinal(i);
            int intCount = objDt.Columns.Count;
            for (int i = strNewColumnsOrder.Length; i < intCount; i++)
                objDt.Columns.RemoveAt(strNewColumnsOrder.Length);
        }
     
    
        public void EditCalcRule_Click(Object sender,EventArgs e)
        {
            string str = forms.utls.prompt("rule id", "input id:", "1");
            string pat = @"^\d+$";
            if (str != null && Regex.IsMatch(str, pat))
            {
            PubInfoBox_MEMO frm = new PubInfoBox_MEMO(Pub.cfg.curr_userinfo.MainForm);
            ESData.GetInst.Open_Conn();
            DbDataReader dr = ESData.GetInst.Reader("select cr_i from sl_calcrule where cr_id="+str);
            if(dr.Read())
                frm.memo.Text = dr.GetString(0);
            frm.btn.Click += (_sender, _e) =>
            {
                StringBuilder sb = new StringBuilder();
                TextWriter _out = new StringWriter(sb);
                try
                {
                    string ins = "update sl_calcrule set cr_i='" + frm.memo.Text + "',cr_o='" + cc.CCMachine.run_cc(frm.memo.Text, _out) + "' where cr_id="+str;
                    _out.WriteLine(" update {0} rec",ESData.GetInst.GetCommand(ins).ExecuteNonQuery().ToString());
                    MessageBox.Show(sb.ToString());
                    frm.Close();
                }catch(Exception exp)
                {
                    string ins = "update sl_calcrule set cr_i='" + frm.memo.Text + "' where cr_id="+str;
                    _out.WriteLine(exp.Message);
                    MessageBox.Show(sb.ToString());
                }
            };
            frm.Show();
            }
        }
        public void ViewMonthDetail_Click(Object sender,EventArgs e)
        {
            /*
                List<string> parm = new List<string>();
                parm.Add(res.StartYM);
                parm.Add(res.EndYM);
                if (res.StafRefs != null) { parm.Add(res.StafRefs); }
                if (res.acttype != null) { parm.Add(res.acttype); }
                if (res.actno != null) { parm.Add(res.actno); }
                iDataGridWF df = new iMonthSalDataWF(parm, "a", 0, Pub.cfg.dbconn);
                DataGridWF dgwf = new DataGridWF(df, null);
                monthdetail_bingdingsource = dgwf.customersBindingSource;
                dgwf.filterToolStripMenuItem.Click += monthdetail_filter_click;
                dgwf.Text = res.StartYM+res.EndYM + "每月數據";
                dgwf.MdiParent = this.MdiParent;
                dgwf.Show();*/
        }
        public void Test_Click(Object sender, EventArgs e)
        {
            forms.utls.ShowView("pragma table_info(sl_rec)");
            forms.utls.ShowView("select * from sl_rec;");
        }
    }
    public class iSqlCmdFormGrid_SalAlloRec : iFormGrid
    {
        List<string> sl_allotype_def = null;
        List<string> sl_allo_key = null;
        public string submitUpdateError = "";
        public DbDataAdapter adapter;
        public DataSet ds;
        public DataTable dt;
        public void submitAddRow(DataRow aRow) { }
        public void submitDeleteRow(DataRow aRow) { }
        public void submitModifyRow(DataRow aRow) {
            if (aRow["sa_id"].ToString().Equals("9999")) { aRow.AcceptChanges(); return; }
            DataRow[] drA = { aRow };
            try
            {
                adapter.Update(drA);
                List<string> li_null_col=new List<string>();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (sl_allotype_def.Contains(dt.Columns[i].ColumnName))
                    {
                        string key = aRow["Staf_ref"].ToString() + dt.Columns[i].ColumnName;
                        string usql =String.Format( " where staf_ref='{0}' and YM='{1}' and allotype='{2}'",aRow["Staf_ref"],aRow["YM"], dt.Columns[i].ColumnName);
                        if (aRow.IsNull(dt.Columns[i].ColumnName)) 
                        { 
                            if(sl_allo_key.Contains(key)) li_null_col.Add(dt.Columns[i].ColumnName);
                            continue; 
                        }

                        if (sl_allo_key.Contains(key))
                        {
                            usql = "update sl_allo set amount=" + aRow[dt.Columns[i].ColumnName].ToString() + " " + usql;
                            using (SQLiteCommand cmd = new SQLiteCommand(usql, (SQLiteConnection)ESData.GetInst.conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            usql = String.Format("insert into sl_allo (staf_ref,YM,allotype,amount,base,mul) values('{0}','{1}','{2}',{3},0,0);",
                                aRow["Staf_ref"], Pub.cfg.YM, dt.Columns[i].ColumnName, aRow[dt.Columns[i].ColumnName]);
                            using (SQLiteCommand cmd = new SQLiteCommand(usql, (SQLiteConnection)ESData.GetInst.conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            sl_allo_key.Add(aRow["Staf_ref"].ToString() + dt.Columns[i].ColumnName);
                        }
                    }
                }
                if(li_null_col.Count>0){
                       string  usql = "update sl_allo set amount=null where staf_ref='{0}' and YM='{1}' and allotype in ({2})";
                       string insql=null;
                       foreach(string s in li_null_col){
                            if(insql==null) {insql="'"+s+"'"; }else{insql+=",'"+s+"'";}
                       }
                       usql=string.Format(usql,aRow["Staf_ref"],aRow["YM"],insql);
                       using (SQLiteCommand cmd = new SQLiteCommand(usql, (SQLiteConnection)ESData.GetInst.conn))
                       {
                                cmd.ExecuteNonQuery();
                        }
                }
                aRow.AcceptChanges();
            }
            catch (Exception e1)
            {
                submitUpdateError += e1.Message;
            }
        }
        public int submitUpdate()
        {
            message_center.tssl_msg("save .....! please wait.");
            ESData.GetInst.Open_Conn();
            DbTransaction _trans=ESData.GetInst.conn.BeginTransaction();
            System.Diagnostics.Stopwatch watch=new System.Diagnostics.Stopwatch();
            watch.Start();
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
            _trans.Commit();
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
            watch.Stop();
            message_center.tssl_msg("saved! completed!."+watch.Elapsed);
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
                if (dt.Columns[i].DataType.Equals(System.Type.GetType("System.DateTime")))
                    if (MessageBox.Show("日期欄位匯出時文字表示,是; 否則用數字表示", "日期欄位", MessageBoxButtons.YesNo) == DialogResult.No)
                        datetimefield_Str_flag = false;
            PubExcel el = new PubExcel(datetimefield_Str_flag);
            el.Visible = true;
            el.ExportXls(dt, dict);
            el.saveTo(filename, true);
            el.closeExcel();
        }
        public void Importxls(String filename, Hashtable dict) { }
        public DataTable GetDT()
        {
            if (dt == null)
            {
                List<FieldDefSt> rec_f = sl_pub.SlRecFields;
                int f_posi = 0;
                int v_posi = 0;
                int withh_posi = 0;
                int adj_posi = 0;
                for (int i = 1; i < rec_f.Count; i++)
                {
                    if (rec_f[i].FieldName.Equals("F_Allowance")) f_posi = i-1;
                    if (rec_f[i].FieldName.Equals("V_Allowance")) v_posi = i-1;
                    if (rec_f[i].FieldName.Equals("WithholdAmount")) withh_posi = i-1;
                    if (rec_f[i].FieldName.Equals("AdjustAmount")) adj_posi = i-1;
                }
                sl_allotype_def = new List<string>();
                sl_allo_key = new List<string>();
                dt = new DataTable();
                ESData.GetInst.Open_Conn();
                adapter = new SQLiteDataAdapter("SELECT * FROM sl_rec where YM='" + Pub.cfg.YM + "'", (SQLiteConnection)ESData.GetInst.conn);
                adapter.Fill(dt);
                int cnt = 0;
                ESData.GetInst.Open_Conn();
                using (SQLiteDataReader dr = new SQLiteCommand("select allotype,fv from sl_allotype where flag=1 order by fv,tab", (SQLiteConnection)ESData.GetInst.conn).ExecuteReader())
                {
                    while (dr.Read())   
                    {
                        sl_allotype_def.Add(dr.GetString(0));
                        cnt++;
                        DataColumn col = new DataColumn(dr.GetString(0), typeof(decimal));
                        dt.Columns.Add(col);
                        switch (dr.GetInt32(1))
                        {
                           case 1: dt.Columns[dr.GetString(0)].SetOrdinal(f_posi + cnt); break;
                            case 2: dt.Columns[dr.GetString(0)].SetOrdinal(v_posi + cnt); break;
                            case 3: dt.Columns[dr.GetString(0)].SetOrdinal(withh_posi + cnt); break;
                            case 4: dt.Columns[dr.GetString(0)].SetOrdinal(adj_posi + cnt); break;
                        }
                    }
                }
                ESData.GetInst.Open_Conn();
                String allosql = String.Format("select staf_ref,allotype,amount from sl_allo where YM='{0}' order by staf_ref,YM,allotype", Pub.cfg.YM);
                using (SQLiteDataReader dr = new SQLiteCommand(allosql, (SQLiteConnection)ESData.GetInst.conn).ExecuteReader())
                {
                    while (dr.Read())
                    {
                        sl_allo_key.Add(dr.GetString(0)+dr.GetString(1));
                        String stafref = dr.GetString(0);
                        DataRow[] d_row=dt.Select(String.Format("staf_ref='{0}'", stafref));
                        if (sl_allotype_def.Contains(dr.GetString(1)))
                        {
                            if (!dr.IsDBNull(2))  d_row[0][dr.GetString(1)] = dr.GetDecimal(2);
                        } else {
                            MessageBox.Show(stafref+"|"+dr.GetString(1));
                        }
                    }
                }
                StringBuilder u_sql = new StringBuilder();
                u_sql.Append("update sl_rec set ");
                for (int i = 1; i < rec_f.Count; i++)
                {
                    u_sql.Append(String.Format("{0}=?", rec_f[i].FieldName));
                    if (i < (rec_f.Count - 1)) u_sql.Append(",");
                }
                u_sql.Append(" where sa_id=@sa_id");
                SQLiteParameter[] u_pars = new SQLiteParameter[rec_f.Count];
                for (int i = 1; i < rec_f.Count; i++)
                   u_pars[i - 1] = new SQLiteParameter("@" + rec_f[i].FieldName, rec_f[i].FieldType, rec_f[i].FieldName);
                u_pars[rec_f.Count - 1] = new SQLiteParameter("@sa_id", DbType.Int32, 4, "sa_id");
                adapter.UpdateCommand = ESData.GetInst.GetSQLiteCommand(u_sql.ToString(), u_pars);
            }
            return dt;
        }
        public void FillDT() {
            GetDT();
        }
        public bool GetRowChanging() { return false; }
        public void FillDT_Next_int() { }
        public void AddNumOfRows()
        {
            string str = forms.utls.prompt("sl_SalRec", "Add Num Of Rows:", "0");
            string pat = @"^\d+$";
            if (str != null && Regex.IsMatch(str, pat))
            {
                int cnt = 0;
                int efcnt = 0;
                ESData.GetInst.Open_Conn();
                using (SQLiteDataReader dr = new SQLiteCommand("select max(sa_id) from sl_rec;", (SQLiteConnection)ESData.GetInst.conn).ExecuteReader())
                {
                    if (dr.Read())
                        if (!dr.IsDBNull(0))
                            cnt = dr.GetInt32(0);
                }
                for (int i = 0; i < int.Parse(str); i++)
                    using (SQLiteCommand cmd = new SQLiteCommand("insert into sl_rec(sa_id,Staf_ref,YM)values(?,?,?);", (SQLiteConnection)ESData.GetInst.conn))
                    {
                        cnt++;
                        cmd.Parameters.AddWithValue("@sa_id", cnt);
                        cmd.Parameters.AddWithValue("@Staf_ref", "STAF_" + cnt);
                        cmd.Parameters.AddWithValue("@YM", Pub.cfg.YM);
                        efcnt += cmd.ExecuteNonQuery();
                    }
                forms.utls.alert(string.Format("insert {0} maxid{1}rec,do it!", efcnt, cnt));
            }
        }
        private int use_cr_id=1;
        public static decimal TaxCalPreMonth(decimal Taxable_income, String Year)
        {
            decimal taxfee = 0.0M;
            {
                            decimal[] taxgrade = { 0, 116.66M, 250, 550, 1216.66M, 2316.66M };
        decimal[] taxrange = { 12000.00M, 13666.67M, 15333.33M, 18666.67M, 25333.33M, 35333.33M };
        Taxable_income = Math.Floor(Taxable_income* 0.75M * 100) / 100;
                //Taxable_income = Math.Ceiling(Taxable_income) * 0.75M;
                if (Taxable_income > taxrange[5])
                {
                    taxfee = Math.Ceiling(Taxable_income - taxrange[5]) * 0.12M + taxgrade[5];
                }
                else if (Taxable_income > taxrange[4])
                {
                    taxfee = (Taxable_income - taxrange[4]) * 0.11M + taxgrade[4];
                }
                else if (Taxable_income > taxrange[3])
                {
                    taxfee = (Taxable_income - taxrange[3]) * 0.10M + taxgrade[3];
                }
                else if (Taxable_income > taxrange[2])
                {
                    taxfee = (Taxable_income - taxrange[2]) * 0.09M + taxgrade[2];
                }
                else if (Taxable_income > taxrange[1])
                {
                    taxfee = (Taxable_income - taxrange[1]) * 0.08M + taxgrade[1];
                }
                else if (Taxable_income > taxrange[0])
                {
                    taxfee = (Taxable_income - taxrange[0]) * 0.07M + taxgrade[0];
                }
                else
                {
                    taxfee = 0.0M;
                }
                taxfee = Math.Floor(taxfee* 100) / 100;
                taxfee = Math.Ceiling(taxfee - Math.Floor(taxfee* 0.30M * 100) / 100);
            }
            return -taxfee;
        }
        public void CalcTotal()
        {
            message_center.tssl_msg("Calc starting.....");
            DataRow[] a = dt.Select("sa_id=9999");
            DataRow tt = null;
            if (a.Length > 0)
            {
                tt = a[0];
            }else
            {
                tt = dt.NewRow();
                tt["sa_id"] = 9999;
                tt["TGRADE"]="小計:";
                dt.Rows.Add(tt);
            }
            List<int> li = new List<int>();
            for (int i = 1; i < dt.Columns.Count; i++)
            {

                if (dt.Columns[i].DataType.ToString() == "System.Decimal"
                    /*||dt.Columns[i].DataType.ToString() == "System.Int32"
                    || dt.Columns[i].DataType.ToString() == "System.Int16"
                    || dt.Columns[i].DataType.ToString() == "System.Single"
                    || dt.Columns[i].DataType.ToString() == "System.Double"*/
                    )
                {
                    tt[i] = 0.0M;li.Add(i);
                }
            }
            foreach (DataRow r in dt.Rows)
            {
                foreach(int i in li)
                {
                    if (!r.IsNull(i)) tt[i]=(Decimal)tt[i] + (Decimal)r[i];
                }
            }
            message_center.tssl_msg("Calc Completed");
        }
        public void Calc()
        {
            message_center.tssl_msg("Calc starting.....");
           StringBuilder sb = new StringBuilder();
            TextWriter _out = new StringWriter(sb);
            ESData.GetInst.Open_Conn();
            string ruleout="";
            using( DbDataReader dr = ESData.GetInst.Reader("select cr_o from sl_calcrule where cr_id="+use_cr_id))
            if(dr.Read())
                ruleout = dr.GetString(0);
            foreach (DataRow r in dt.Rows)
            {
                SAL.cx.DataRowVarMgr tvm = new SAL.cx.DataRowVarMgr(r);
                SAL.cx.CXMachine.run_cx(tvm, ruleout, _out);
                _out.WriteLine("-------output--------");
                foreach (DictionaryEntry entry in tvm.getHashtable())
                    _out.WriteLine(entry.Key + " = " + entry.Value);
                _out.WriteLine("---------------");
                r["Note"] = "<?xml version='1.0' encoding='UTF-8'?><dd />";
            }
            PubInfoBox_MEMO frm = new PubInfoBox_MEMO(Pub.cfg.curr_userinfo.MainForm);
            frm.memo.Text=sb.ToString();
            frm.Show();
            message_center.tssl_msg("Calc Completetd");
        }
        public void PritnSALForm() 
        {
            String file_name = String.Format("{0}\\temp{1}.html", Pub.cfg.getAppPath(), DateTime.Now.ToString("HHmmss"));

            String datafile = HTMLRPT.out_html(file_name, dt, null);
            try
            {
                String[] margin = { "0.3", "0.3", "0.2", "0.2" };
                forms.Form_Print_Html out_r = new forms.Form_Print_Html(file_name, datafile, margin);
                out_r.MdiParent = Pub.cfg.curr_userinfo.MainForm;
                out_r.Show();
            }
            catch (Exception ep1)
            {
                MessageBox.Show(ep1.Message);
            }
        }
    }
    /*
     public class iMonthSalDataWF : iDataGridWF
    {
        //private string YearMonth;

        public iMonthSalDataWF(List<string> parm, string tablename, int flag,OdbcConnection conn)
            : base(parm, tablename, 0, conn)
        {
        }

        protected override void Initialize()
        {
            //base.Initialize();

            String startYM = param[0];
            String endYM = param[1];
            String cond = "";
            if (param.Count > 2) { cond = " and " + param[2]; };
            //YearMonth = c_sql;
            string actsumSQL = @"SELECT 
b.acctItem_no `會計科目`,
b.acctItem  `會計代號`, 
b.bankacct `帳戶`,
b.transAcctno `過數帳戶`,
a.qu_no `QNO`,
a.salary_no `NO_`,
a.YM ,
b.Staf_ref `職員編號`,
b.C_NAME `中文姓名`,
b.bankacctname `戶名`,
a.TGRADE `教師級別`,
a.Net_Income `淨收入________`,
a.`baseSalary`  `底薪________`,
a.`SPTPay`      `特別職務報酬_`,
a.`FixExtraWorkPay` `超時薪金`,
(a.`baseSalary` + a.`FixExtraWorkPay`+a.`SPTPay` )  `總底薪________`, 
(a.`F_allowance`) `固定津貼總計` , 
(a.`Seniority`) `年資` , 
(a.`V_allowance` ) `非固定津貼總計` , 
(a.Teaching_subsidy_for_teacher_replacement) `正規代課`,
(a.Not_teaching_subsidy_for_teacher_replacement) `自修管理`,
(a.`PensionFund_withhold`) `學校5%基金`, 
(a.`PensionFund_Amount`) `基金累積供款`,
(a.`FSS_Fee`) `扣保障基金` , 
(a.`Leave_withhold`) `病事假扣薪` , 
(a.`Adjust_tax`)  `年度職業稅調整`, 
(a.`AdjustAmount`)  `前期調整`, 
a.`Taxable_Income`  `每月總收益(不含B1)`, 
(a.`Tax`) `職業稅扣款` 
FROM  `salary_rec` a
INNER JOIN sa_stafinfo b ON a.staf_ref = b.staf_ref
WHERE  `YM` >=  '{0}' and `YM` <='{1}' {2} order by a.qu_no";
            //(a.`SubstituteTeachPay`)  `代課金(不用)`,  
            String sql = string.Format(actsumSQL, startYM, endYM, cond);
            System.Diagnostics.Debug.WriteLine(sql);

            OdbcCommand actsumCmd = new OdbcCommand(
sql, es_dblib.esdb.GetInst.GetConn());
            OdbcDataReader actsumDr = actsumCmd.ExecuteReader();

            ds = new DataSet();
            dt = ds.Tables.Add(tablename);
            List<string> allowance_name = new List<string>();
            for (int i = 0; i < actsumDr.FieldCount; i++)
            {
                if (i < 11)
                { dt.Columns.Add(actsumDr.GetName(i)); }
                else
                {
                    DataColumn dco = dt.Columns.Add(actsumDr.GetName(i), System.Type.GetType("System.Decimal"));
                }
                if (actsumDr.GetName(i) == "固定津貼總計")
                {
                    OdbcCommand FCmd = new OdbcCommand(
    @"SELECT  `F_Allowance` 
FROM  `sl_fixed_allowance` 
WHERE  `YM` >=  '" + startYM + "' and `YM` <= '" + endYM + "' GROUP BY  `F_Allowance`  ORDER BY  `F_Allowance` "
    , es_dblib.esdb.GetInst.GetConn());
                    OdbcDataReader dr = FCmd.ExecuteReader();
                    while (dr.Read())
                    {
                        if (!allowance_name.Contains(dr[0].ToString().Trim()))
                        {
                            allowance_name.Add(dr[0].ToString().Trim());
                            dt.Columns.Add(dr[0].ToString().Trim(), System.Type.GetType("System.Decimal"));
                        }
                    }
                    dr.Dispose();
                    FCmd.Dispose();
                }
                if (actsumDr.GetName(i) == "非固定津貼總計")
                {
                    OdbcCommand FCmd = new OdbcCommand(
    @"SELECT  `V_Allowance` 
FROM  `sl_variable_allowance` 
WHERE  `YM`  >=  '" + startYM + "' and `YM`  <= '" + endYM + "' GROUP BY  `V_Allowance`  ORDER BY  `V_Allowance` "
    , es_dblib.esdb.GetInst.GetConn());
                    OdbcDataReader dr = FCmd.ExecuteReader();
                    while (dr.Read())
                    {
                        if (!allowance_name.Contains(dr[0].ToString().Trim()))
                        {
                            allowance_name.Add(dr[0].ToString().Trim());
                            dt.Columns.Add(dr[0].ToString().Trim(), System.Type.GetType("System.Decimal"));
                        }
                    }
                    dr.Dispose();
                    FCmd.Dispose();
                }
            }
            DataRow total_dr = dt.NewRow();
            total_dr["會計科目"] = "TOTAL";
            for (int i = 9; i < dt.Columns.Count; i++) total_dr[i] = 0.0M;
            while (actsumDr.Read())
            {
                DataRow dr = dt.Rows.Add();
                for (int i = 0; i < actsumDr.FieldCount; i++)
                {
                    dr[actsumDr.GetName(i)] = actsumDr[i];
                    if (i >= 11 && !actsumDr.IsDBNull(i))
                    {
                        try
                        {
                            total_dr[actsumDr.GetName(i)] = decimal.Parse(total_dr[actsumDr.GetName(i)].ToString()) + actsumDr.GetDecimal(i);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show(exp.Message);
                        }
                    }
                }

            }
            actsumDr.Dispose();
            actsumCmd.Dispose();
            dt.Rows.Add(total_dr);

            {
                OdbcCommand FGcmd = new OdbcCommand(
    @"SELECT staf_ref `職員編號` , F_Allowance ITEM, SUM( F_Amount ) AMT,YM
FROM  `sl_fixed_allowance` 
WHERE  `YM` >=  '" + startYM + "'  and `YM`  <= '" + endYM + "' GROUP BY staf_ref, F_Allowance ORDER BY staf_ref, F_Allowance"
        , es_dblib.esdb.GetInst.GetConn());
                OdbcDataReader FGdr = FGcmd.ExecuteReader();
                while (FGdr.Read())
                {
                    DataRow[] dr = dt.Select("`職員編號`='" + FGdr["職員編號"].ToString() + "' and YM='" + FGdr["YM"].ToString() + "'");
                    if (dr.Length > 0)
                    {
                        dr[0][FGdr["ITEM"].ToString()] = FGdr["AMT"];
                        total_dr[FGdr["ITEM"].ToString()] = decimal.Parse(total_dr[FGdr["ITEM"].ToString()].ToString()) + decimal.Parse(FGdr["AMT"].ToString());
                    }

                }
                FGdr.Dispose();
                FGcmd.Dispose();
            }
            {
                OdbcCommand FGcmd = new OdbcCommand(
    @"SELECT staf_ref `職員編號` , V_Allowance ITEM, SUM( V_Amount ) AMT,YM
FROM  `sl_variable_allowance` 
WHERE  `YM`  >=  '" + startYM + "' and `YM`  <= '" + endYM + "' GROUP BY staf_ref, V_Allowance ORDER BY staf_ref, V_Allowance"
        , es_dblib.esdb.GetInst.GetConn());
                OdbcDataReader FGdr = FGcmd.ExecuteReader();
                while (FGdr.Read())
                {
                    DataRow[] dr = dt.Select("`職員編號`='" + FGdr["職員編號"].ToString() + "' and YM='" + FGdr["YM"].ToString() + "'");
                    if (dr.Length > 0)
                    {
                        dr[0][FGdr["ITEM"].ToString()] = FGdr["AMT"];
                        total_dr[FGdr["ITEM"].ToString()] = decimal.Parse(total_dr[FGdr["ITEM"].ToString()].ToString()) + decimal.Parse(FGdr["AMT"].ToString());
                    }
                }
                FGdr.Dispose();
                FGcmd.Dispose();

            }


        }
        public override void submitAddRow(DataRow aRow)
        {
            //base.submitAddedRow(aRow);
        }
        public override void submitDeleteRow(DataRow aRow)
        {
            //base.submitDeletdRow(aRow);
        }
        public override void submitModifyRow(DataRow aRow)
        {
            //base.submitModifiedRow(aRow);
        }
    }
    public class iDataGridWF
    {
        public OdbcDataAdapter adapter;
        public OdbcCommandBuilder cmdb;
        public DataSet ds;
        public DataTable dt;
        public string submitUpdateError = null;
        public List<String> readonly_fieldnames = null;
        public List<String> Columns = null;
        public List<DataGridViewColumn> ColumnsType = null;
        protected string c_sql;
        protected string tablename;
        protected int preDBRecCNT = 0;
        protected List<string> param = null;
        private OdbcCommand delete_cmd=null;
        protected OdbcConnection _conn = null;
        public iDataGridWF(string c_sql, string tablename,OdbcConnection conn)
        {
            this.c_sql = c_sql;
            this.tablename = tablename;
            _conn = conn;
            Initialize();
        }
        public iDataGridWF(string c_sql, string tablename,OdbcCommand delcmd,OdbcConnection conn)
        {
            this.c_sql = c_sql;
            this.tablename = tablename;
            _conn = conn;
            delete_cmd=delcmd;
            Initialize();
        }
        public iDataGridWF(List<string> c_sql, string tablename,int flag,OdbcConnection conn)
        {
            param = c_sql;
            this.tablename = tablename;
            _conn = conn;
            Initialize();
        }
        ~iDataGridWF()
        {
            if (param != null) { param.Clear(); }
        }
        public OdbcConnection conn
        {
            set
            {
                _conn = value;
            }
            get
            {
                return null;
            }
        }
        protected virtual void Initialize()
        {
            adapter = new OdbcDataAdapter(c_sql, _conn);
            cmdb = new OdbcCommandBuilder(adapter);
            ds = new DataSet();
            adapter.TableMappings.Add("Table", tablename);
            adapter.Fill(ds);
            if (delete_cmd != null) adapter.DeleteCommand = delete_cmd;
            dt = ds.Tables[tablename];
            preDBRecCNT = dt.Rows.Count;
        }
        public virtual void reload_DBAdapter()
        {
            //FillAdd();Initialize(); 131210 modify by cool;
            ds =new DataSet();
            adapter.Fill(ds);
            dt = ds.Tables[tablename];
            preDBRecCNT = dt.Rows.Count;
        }
        public virtual void Fill_AddLastRow(string addnewrowsql)
        {
            OdbcDataAdapter adp = new OdbcDataAdapter();
            adp.SelectCommand = new OdbcCommand(addnewrowsql, _conn);
            adp.TableMappings.Add("Table", tablename);
            adp.Fill(dt);
            preDBRecCNT = dt.Rows.Count;           
        }
        public void getDataWithSelColumn(List<string> liColumns){
            string[] delmetStrs={"From","from","FROM"};
            string[] arrStr = c_sql.Split(delmetStrs,2,StringSplitOptions.None);
            if(arrStr.Length<2 || liColumns.Count==0) return;
            string selectItem="";
            selectItem = liColumns[0].ToString();
            for(int i=1;i<liColumns.Count;i++)
            {
                selectItem +=  "," + liColumns[i].ToString() ;   
            }
            string select_sql=" select "+selectItem+ " from "+arrStr[1];
            MessageBox.Show(select_sql);
            adapter = new OdbcDataAdapter(select_sql,_conn);
            cmdb = new OdbcCommandBuilder(adapter);
            ds = new DataSet();
            adapter.TableMappings.Add("Table", tablename);
            adapter.Fill(ds);
            dt = ds.Tables[tablename];
            return;
        }
        public virtual void submitAddRow(DataRow aRow)
        {
            DataRow[] drA ={ aRow };
            adapter.Update(drA);
        }
        public virtual void submitModifyRow(DataRow aRow)
        {
            try
            {
                DataRow[] drA ={ aRow };
                adapter.Update(drA);
            }
            catch (Exception e1)
            {
               submitUpdateError+=string.Format("{0,10}{1}\n",aRow[0].ToString(),e1.Message);
            }
        }
        public virtual void submitDeleteRow(DataRow aRow)
        {
            DataRow[] drA ={ aRow };
            try
            {
                adapter.Update(drA);
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
        }

        public virtual int submitUpdate()
        {
            submitUpdateError = null;
            int cnt = 0;
            int updatecnt = 0;
            //dt.AcceptChanges();
            //IEnumerator rowEnum = dt.Rows.GetEnumerator();
            List<DataRow> delRows = new List<DataRow>();
            foreach(DataRow currRow in dt.Rows)
            {
                //while (rowEnum.MoveNext())
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
                            DataRow[] drA ={ currRow };
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
            else
            {
            }
            return cnt;
        }
        public virtual bool NeedUpdate()
        {
            foreach (DataRow currRow in dt.Rows)
            {
                switch (currRow.RowState)
                {
                    case DataRowState.Unchanged: break;
                    case DataRowState.Added: return true; 
                    case DataRowState.Modified: return true; 
                    case DataRowState.Deleted: return true; 
                    default:
                        break;
                }

            }
            return false;
        }
        public virtual void ImportXls(string filename,Hashtable dict)
        {
            PubExcel el = new PubExcel(filename, "sheet1");
            el.Visible = true;
            el.ImportXls(dt);
            el.closeExcel();
        }
        public virtual void Exportxls(string filename,Hashtable dict)
        {
            if (dt.Columns.Count > 127)
            {
                MessageBox.Show("欄位過長,超過127,現在欄位為"+dt.Columns.Count);
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
        public virtual void WriteXml(string filename)
        {
            dt.WriteXml(filename);
            MessageBox.Show(filename);
            dt.WriteXmlSchema(filename + ".schema", true);
            MessageBox.Show(filename);
        }
        public virtual void ReadXml(string filename)
        {
            dt.ReadXml(filename);
        }
    }
    public class iCMDDataGridWF:iDataGridWF
    {
        public iCMDDataGridWF(OdbcCommand selcmd, OdbcCommand updcmd)
            : base(null, null,null)
        {
            this.adapter=new OdbcDataAdapter();
            adapter.SelectCommand = selcmd;
            adapter.UpdateCommand = updcmd;
            ds = new DataSet();
            adapter.TableMappings.Add("Table", "tablename");
            adapter.Fill(ds);
            dt = ds.Tables["tablename"];
            preDBRecCNT = dt.Rows.Count;
        }
        public iCMDDataGridWF(OdbcCommand selcmd, OdbcCommand updcmd,OdbcCommand delcmd)
            : base(null, null,null)
        {
            this.adapter = new OdbcDataAdapter();
            adapter.SelectCommand = selcmd;
            adapter.UpdateCommand = updcmd;
            adapter.DeleteCommand = delcmd;
            ds = new DataSet();
            adapter.TableMappings.Add("Table", "tablename");
            adapter.Fill(ds);
            dt = ds.Tables["tablename"];
            preDBRecCNT = dt.Rows.Count;
        }


        protected override void Initialize()
        {
        }
        public override void submitAddRow(DataRow aRow)
        {
            MessageBox.Show("唯讀!");
        }
        public override void submitModifyRow(DataRow aRow)
        {
            //base.submitAddRow(aRow);
            base.submitModifyRow(aRow);

        }
        public override void submitDeleteRow(DataRow aRow)
        {
            if (adapter.DeleteCommand == null)
            { MessageBox.Show("唯讀!"); }
            else
            {
                base.submitDeleteRow(aRow);
            }
        }
    }*/
}
