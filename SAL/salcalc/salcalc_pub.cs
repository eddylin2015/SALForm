using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAL.salcalc
{
    class sl_pub
    {
        private static List<FieldDefSt> _SlRecFields;
        public static List<FieldDefSt> SlRecFields
        {
            get
            {
                if (_SlRecFields == null)
                {
                    _SlRecFields = new List<FieldDefSt>();
                    System.Data.Common.DbDataReader dr = ESData.GetInst.Reader("pragma table_info(sl_rec)");
                    while (dr.Read())
                    {
                        String strColumnName = dr.GetString(1);
                        String strColumnType = dr.GetString(2);
                        int strSize = 0;
                        string[] digits = Regex.Split(strColumnType, @"\D+");
                        if(digits.Length>0) int.TryParse(digits[0], out strSize);
                        DbType dbtype = DbType.String;
                        if (strColumnType.Contains("int")) dbtype = DbType.Int32;
                        if (strColumnType.Contains("decimal")) dbtype = DbType.Decimal;
                        _SlRecFields.Add(new FieldDefSt(strColumnName, dbtype, strSize));
                    }
                    dr.Close();
                    dr.Dispose();
                }
                return _SlRecFields;
            }
        }
        private static List<FieldDefSt> _FixAlloItem = null;
        public static List<FieldDefSt> FixAlloItem
        {
            get
            {
                if (_FixAlloItem == null)
                {
                    _FixAlloItem = new List<FieldDefSt>();
                    ESData.GetInst.Open_Conn();
                    System.Data.Common.DbDataReader dr = ESData.GetInst.Reader("select allotype,allotypec,allotypee,tab,fv from sl_allotype order by fv,tab");
                    while (dr.Read())
                        _FixAlloItem.Add(new FieldDefSt(dr.GetString(0), DbType.Decimal, 0, dr.GetString(1), dr.GetString(2), dr.GetInt16(3), dr.GetInt16(4)));
                    dr.Close();
                    dr.Dispose();
                };
                return _FixAlloItem;
            }
        }
    }
    /// <summary>
    /// 不用
    /// </summary>
    class salcalc_pub
    {
        public static List<FieldDefSt> FixAlloItem
        {
            get
            {
                if (_FixAlloItem == null)
                {
                    _FixAlloItem = new List<FieldDefSt>();
                    ESData.GetInst.Open_Conn();
                    System.Data.Common.DbDataReader dr = ESData.GetInst.Reader("select allotype,allotypec,allotypee,tab,fv from sl_allotype order by fv,tab");
                    while (dr.Read())
                        _FixAlloItem.Add(new FieldDefSt(dr.GetString(0), DbType.Decimal, 0, dr.GetString(1), dr.GetString(2), dr.GetInt16(3), dr.GetInt16(4)));
                    dr.Close();
                    dr.Dispose();
                };
                return _FixAlloItem;
            }
        }
        public static List<FieldDefSt> _FixAlloItem = null;
        public static List<FieldDefSt> _IncItem = null;
        public static List<FieldDefSt> IncomeItem
        {
            get
            {
                if (_IncItem == null)
                {
                    _IncItem = new List<FieldDefSt>();
                    foreach (FieldDefSt s in FixAlloItem)
                        _IncItem.Add(s);
                }
                return _IncItem;
            }
        }
        public static List<FieldDefSt> SalaryRecFields
        {
            get
            {
                if (_SalaryRecFields == null)
                {
                    _SalaryRecFields = new List<FieldDefSt>();
                    _SalaryRecFields.Add(new FieldDefSt("Sa_id", DbType.Int32, 0));
                    _SalaryRecFields.Add(new FieldDefSt("Staf_ref", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("YM", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("c_name", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("e_name", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("WorkPosi", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("School_No", DbType.String, 16));
                    _SalaryRecFields.Add(new FieldDefSt("SchoolSect", DbType.Int32, 0));
                    _SalaryRecFields.Add(new FieldDefSt("WorkDept", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("FSS_NO", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("TAX_NO", DbType.String, 64));
                    _SalaryRecFields.Add(new FieldDefSt("baseSalary", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("FSS_Fee", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("Seniority", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("F_allowance", DbType.Decimal, 0));
                    //sqlite: pragma table_info(foo)  //mysql: SHOW COLUMNS IN sl_rec
                    System.Data.Common.DbDataReader dr = ESData.GetInst.Reader("pragma table_info(sl_rec)");
                    bool start_flag = false;
                    while (dr.Read())
                    {
                        String strColumnName = dr.GetString(0);
                        String strColumnType = dr.GetString(1);
                        String strColumnNull = dr.GetString(2);
                        String strColumnPKey = dr.GetString(3);
                        //strColumnDflt = odrColumnReader.GetString(4);
                        String strColumnExtr = dr.GetString(5);
                        if (strColumnName.ToUpper().Equals("V_ALLOWANCE")) { break; }
                        if (start_flag)
                        {
                            _SalaryRecFields.Add(new FieldDefSt(dr.GetString(0), DbType.Decimal, 0));
                        }
                        if (strColumnName.ToUpper().Equals("F_ALLOWANCE")) { start_flag = true; }

                    }
                    dr.Close();
                    dr.Dispose();
                    _SalaryRecFields.Add(new FieldDefSt("V_allowance", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("Leave_withhold", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("Not_teaching_subsidy_for_teacher_replacement", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("Teaching_subsidy_for_teacher_replacement", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("Taxable_Income", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("Tax", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("Adjust_tax", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("AdjustAmount", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("FixExtraWorkPay", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("SptPay", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("PensionFund_preAmount", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("PensionFund_withhold", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("PensionFund_Amount", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("Net_income", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("Note", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("Salary_period", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("note1", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("note2", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("PensionFund_curr_YM", DbType.String, 0));
                    _SalaryRecFields.Add(new FieldDefSt("LeaveJob_Comp_PF", DbType.Decimal, 0));
                    _SalaryRecFields.Add(new FieldDefSt("LeaveJob_Pers_PF", DbType.Decimal, 0));
                }
                else
                {
                }
                return _SalaryRecFields;
            }
        }
        private static List<FieldDefSt> _SalaryRecFields;
    }
    public class Number_Check
    {
        public static bool isDecimal(String input)
        {
            System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex(@"^([+-]?\d+|\d+.\d+)$");
            return rg.IsMatch(input);
        }
        public static bool isNum(String input)
        {
            System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex(@"^(\d+|\d+[.]?\d+)$");
            return rg.IsMatch(input);
        }
    }
    public struct FieldDefSt
    {
        public String FieldName;
        public DbType FieldType;
        public int Size;
        public String FieldCName;
        public String FieldEName;
        public int Tab;
        public decimal Amount;
        public int AlloType;//1=fix 2=var
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldname"></param>
        /// <param name="fieldtype"></param>
        /// <param name="size"></param>
        /// <param name="fieldcname"></param>
        /// <param name="fieldename"></param>
        /// <param name="tab"></param>
        /// <param name="allotype"></param>
        public FieldDefSt(string fieldname, DbType fieldtype, int size, String fieldcname, String fieldename, int tab, int allotype)
        {
            FieldName = fieldname;
            FieldType = fieldtype;
            Size = size;
            FieldCName = fieldcname;
            FieldEName = fieldename;
            Tab = tab;
            AlloType = allotype;
            Amount = 0M;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldname"></param>
        /// <param name="fieldtype"></param>
        /// <param name="size"></param>
        /// <param name="fieldcname"></param>
        public FieldDefSt(string fieldname, DbType fieldtype, int size, String fieldcname)
        {
            FieldName = fieldname;
            FieldType = fieldtype;
            Size = size;
            FieldCName = fieldcname;
            FieldEName = "";
            Tab = 0;
            AlloType = 0;
            Amount = 0M;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldname"></param>
        /// <param name="fieldtype"></param>
        /// <param name="size"></param>
        public FieldDefSt(string fieldname, DbType fieldtype, int size)
        {
            FieldName = fieldname;
            FieldType = fieldtype;
            Size = size;
            FieldCName = "";
            FieldEName = "";
            Tab = 0;
            AlloType = 0;
            Amount = 0M;
        }
    }
}
