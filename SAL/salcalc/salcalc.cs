using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Dict = SAL.salcalc.salcalc_pub;
namespace SAL.salcalc
{
    public partial class PrtSalaryCal
    {
        private DataSet Sal_Rec_DS = null;
        private SQLiteDataAdapter Adapter = null;
        private String YM = Pub.cfg.YM;
        private Hashtable AllocItem_Dict = new Hashtable();
        private SQLiteDataAdapter SST_Adapter = null;
        private String salary_period = null; //糧單由某一月起始結束x月x日~x月x日
        private String note1 = null;         //糧單備註一
        private String note2 = null;         //糧單備註二
        private SQLiteConnection conn = (SQLiteConnection)ESData.GetInst.conn;

        /// <summary>
        /// 初始化DataSet(SalaryRec) DataAdapter;作為資料中間媒介, 取得和更新資料.
        /// <code>
        /// 1 to n tables relation link
        /// 1: Salary_rec 
        ///    childern_relations
        /// n: {stafinfo,baseinfo,historySeniority,Fallowance,VAllowance,SubstitutedTeach,Leave,ADJUST};
        /// 
        /// Initalize Fill Data;
        /// </code>
        /// </summary>
        /// <param name="stafref">若 任一職員編號 則 處理一個當月(YM)職員資料, 
        ///                        否則為 NULL 為處理所有在職當月糧資料 workstatus=1 and YM
        /// </param>
        private void initDA(String stafref)
        {
            //AllocItem_Dict{(房屋津貼->Housing_allowance)
            //             ..(夏令金->Summer_Class_allowance)}
            AllocItem_Dict.Clear();

            //Dict.FixAlloItem.Sort();

            for (int i = 0; i < Dict.FixAlloItem.Count; i++)
                AllocItem_Dict.Add(Dict.FixAlloItem[i].FieldCName, Dict.FixAlloItem[i].FieldName);
            /////////////////////////////////////////
            Sal_Rec_DS = new DataSet();
            /////////////////////////////////////////
            String Sal_SelSQL = "Select Sa_id";
            for (int i = 1; i < Dict.SalaryRecFields.Count; i++)
            {
                Sal_SelSQL += "," + Dict.SalaryRecFields[i].FieldName;
            }
            if (stafref == null)
            {
                Sal_SelSQL += " FROM `salary_rec` WHERE WorkStatus=1 and YM ='" + YM + "'";
            }
            else
            {
                Sal_SelSQL += " FROM `salary_rec` WHERE staf_ref='" + stafref + "' and YM ='" + YM + "'";
            }
            Adapter = new SQLiteDataAdapter(Sal_SelSQL, conn);
            /////////////////////////////////////////////////////////
            String Sal_UpdSQL = @"UPDATE salary_rec SET ";
            for (int i = 5; i < Dict.SalaryRecFields.Count - 3; i++)
            {
                if (i > 5) Sal_UpdSQL += ",";
                Sal_UpdSQL += Dict.SalaryRecFields[i].FieldName + "=?";
            }
            Sal_UpdSQL += " where Sa_id=? ";
            Adapter.UpdateCommand = new SQLiteCommand(Sal_UpdSQL,(SQLiteConnection)ESData.GetInst.conn);
            for (int i = 5; i < Dict.SalaryRecFields.Count - 3; i++)
            {
                FieldDefSt fdd = Dict.SalaryRecFields[i];
                Adapter.UpdateCommand.Parameters.Add("@" + fdd.FieldName, fdd.FieldType, fdd.Size, fdd.FieldName);
            }
            Adapter.UpdateCommand.Parameters.Add(
                new System.Data.SQLite.SQLiteParameter(
                "Original_Sa_id",
                DbType.Int32,
                0,
                System.Data.ParameterDirection.Input,
                false,
                ((byte)(0)),
                ((byte)(0)),
                "Sa_id",
                System.Data.DataRowVersion.Original,
                null));
            Adapter.Fill(Sal_Rec_DS, "Salary_Rec");
            Adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            ///////////////////////////////////////////////////////////
            String[,] subsql = new string[7, 3];
            subsql[0, 0] = "stafinfo";
            subsql[0, 1] = "select `Staf_ref`, C_name ,e_name, workType ,workPosi,workDept,schoolsect,FSS_NO,Tax_no from sa_stafinfo ";
            subsql[1, 0] = "baseinfo";
            subsql[1, 1] = "select `Staf_ref`, C_name ,`Seniority_base`, `Seniority_years`, `Seniority_YM`, `Week_Section`,  `Week_hrs`, `PensionFund_curr_YM`,PensionFund_Accept from salarybaseinfo ";
            subsql[2, 0] = "historySeniority";
            subsql[2, 1] = "SELECT `Hs_id`, `Staf_ref`, `Seniority_base`, `Seniority_years`, `Hs_note` FROM `sl_historyseniority` ";
            subsql[3, 0] = "Allowance";
            subsql[3, 1] = "SELECT `al_id`, `Staf_ref`, `YM`, `Allowance`, `Amount`, `note`,fv FROM `sl_allo` ";
            /*
            subsql[3, 0] = "FAllowance";
            subsql[3, 1] = "SELECT `Fa_id`, `Staf_ref`, `YM`, `F_Allowance`, `F_Amount`, `note` FROM `sl_fixed_allowance` ";
            subsql[4, 0] = "VAllowance";
            subsql[4, 1] = "SELECT `va_id`, `Staf_ref`, `YM`, `V_Allowance`, `V_Amount`, `note` FROM `sl_variable_allowance` ";
            */
            subsql[4, 0] = "SubstituteTeach";
            subsql[4, 1] = "SELECT `St_id`, `St_date`, `St_classno`, `St_sectno`, `St_Staf_ref`, `St_c_name`, `From_staf_ref`, `From_c_name`, `Note`, `Cal_YM`,St_Type,pay FROM `sl_substituteteachrec` ";
            subsql[5, 0] = "Leave";
            subsql[5, 1] = "SELECT `sl_id`, `Staf_ref`, `c_name`, `Start_Date`, `Sl_days`, `Sl_sections`, `Cal_YM`, `Sl_note` FROM `sl_leave` ";
            subsql[6, 0] = "ADJUST";
            subsql[6, 1] = "SELECT `aj_id` , `Staf_ref` , `YM` , `Adjustment` , `AJ_Amount` , `note` , `UnTaxable` FROM `sl_adjustment_amount` ";
            subsql[0, 2] = "Staf_ref";
            subsql[1, 2] = "Staf_ref";
            subsql[2, 2] = "Staf_ref";
            subsql[3, 2] = "Staf_ref";
            //subsql[4, 2] = "Staf_ref";
            subsql[4, 2] = "St_Staf_ref";
            subsql[5, 2] = "Staf_ref";
            subsql[6, 2] = "Staf_ref";
            if (stafref == null)
            {
                subsql[0, 1] += " where staf_ref in(select staf_ref from salary_rec where workstatus=1 and YM='" + YM + "')";
                subsql[1, 1] += " where staf_ref in(select staf_ref from salary_rec where workstatus=1 and YM='" + YM + "')";
                subsql[2, 1] += " where staf_ref in(select staf_ref from salary_rec where workstatus=1 and YM='" + YM + "')";
                subsql[3, 1] += " WHERE YM='" + YM + "' and staf_ref in(select staf_ref from salary_rec where workstatus=1 and YM='" + YM + "')";
                /*
                subsql[3, 1] += " WHERE YM='" + YM + "' and staf_ref in(select staf_ref from salary_rec where workstatus=1 and YM='" + YM + "')";
                subsql[4, 1] += " WHERE YM='" + YM + "' and staf_ref in(select staf_ref from salary_rec where workstatus=1 and YM='" + YM + "')";
                */
                subsql[4, 1] += " WHERE Cal_YM='" + YM + "' and St_Staf_ref in(select staf_ref St_Staf_ref from salary_rec where workstatus=1 and YM='" + YM + "')";
                subsql[5, 1] += " WHERE Cal_YM='" + YM + "' and    staf_ref in(select staf_ref             from salary_rec where workstatus=1 and YM='" + YM + "')";
                subsql[6, 1] += " WHERE YM='" + YM + "' and staf_ref in(select staf_ref from salary_rec where workstatus=1 and YM='" + YM + "')";
            }
            else
            {
                String StafRef = stafref;
                subsql[0, 1] += " where  staf_ref='" + StafRef + "'";
                subsql[1, 1] += " where  staf_ref='" + StafRef + "'";
                subsql[2, 1] += " where  staf_ref='" + StafRef + "'";
                subsql[3, 1] += " WHERE YM='" + YM + "' and staf_ref ='" + StafRef + "'";
                /*
                subsql[3, 1] += " WHERE YM='" + YM + "' and staf_ref ='" + StafRef + "'";
                subsql[4, 1] += " WHERE YM='" + YM + "' and staf_ref ='" + StafRef + "'";
                */
                subsql[4, 1] += " WHERE Cal_YM='" + YM + "' and St_Staf_ref ='" + StafRef + "'";
                subsql[5, 1] += " WHERE Cal_YM='" + YM + "' and staf_ref ='" + StafRef + "'";
                subsql[6, 1] += " WHERE YM='" + YM + "' and staf_ref ='" + StafRef + "'";
            }

            SQLiteDataAdapter[] subdataadapter = new SQLiteDataAdapter[8];
            DataColumn pcol = Sal_Rec_DS.Tables["Salary_Rec"].Columns["Staf_ref"];
            for (int i = 0; i < subsql.GetLength(0); i++)
            {
                SQLiteCommand cmd = new SQLiteCommand(subsql[i, 1], conn);
                subdataadapter[i] = new SQLiteDataAdapter(cmd);
                subdataadapter[i].MissingSchemaAction = MissingSchemaAction.AddWithKey;
                subdataadapter[i].Fill(Sal_Rec_DS, subsql[i, 0]);
                DataColumn ccol = Sal_Rec_DS.Tables[subsql[i, 0]].Columns[subsql[i, 2]];
                DataRelation dr = new DataRelation("sr_" + subsql[i, 0], pcol, ccol);
                dr.Nested = true;
                Sal_Rec_DS.Relations.Add(dr);
            }
            String SST_UpdSQL = "update sl_substituteteachrec set pay=? where St_id=?;";
            SQLiteCommand SST_UpdCmt = new SQLiteCommand(SST_UpdSQL,(SQLiteConnection) ESData.GetInst.conn);
            SST_UpdCmt.Parameters.Add("@play", DbType.Decimal, 0, "pay");
            SST_UpdCmt.Parameters.Add(
                new System.Data.SQLite.SQLiteParameter(
                "Original_St_id",
                DbType.Int32,
                0,
                System.Data.ParameterDirection.Input,
                false,
                ((byte)(0)),
                ((byte)(0)),
                "St_id",
                System.Data.DataRowVersion.Original,
                null));
            subdataadapter[5].UpdateCommand = SST_UpdCmt;
            SST_Adapter = subdataadapter[5];

        }
        /// <summary>
        /// Fill Data
        /// </summary>
        /// <param name="pStafRef"></param>
        public virtual void FillData(String pStafRef)
        {
            initDA(pStafRef);
        }
        /// <summary>
        /// Fill Data
        /// </summary>
        /// <param name="pYM"></param>
        /// <param name="pStafRef"></param>
        public virtual void FillData(String pYM, String pStafRef)
        {
            YM = pYM;
            initDA(pStafRef);
        }

        /// <summary>
        /// Fill Data Set;
        /// </summary>
        public virtual void FillData()
        {
            initDA(null);
        }

        /// <summary>
        /// 計算公積金計算器
        /// <code>
        /// if FullTimeWork and Accept PensionFund
        ///       withhold=(baseSalary+fixExtraWorkPlay)* -0.05M
        ///       if curr_YM != preYM
        ///         Amount =preAmount+   (baseSalary+FixExtraWorkPay) * 0.1M * PrtSalaryCal.YMtoCurrMonths(preYM, curr_YM);
        ///      else if  curr_YM == preYM  and  preAmount == 0.0M 
        ///         Amount = (baseSalary +FixExtraWorkPay) * 0.1M;
        ///      else
        ///         Amount = preAmount;
        /// else
        ///       withhold = 0.0M;
        ///       Amount = preAmount;
        /// </code>
        /// </summary>
        /// <param name="PensionFund_Accept"></param>
        /// <param name="baseSalary"></param>
        /// <param name="FixExtraWorkPay"></param>
        /// <param name="SptPay"></param>
        /// <param name="curr_YM"></param>
        /// <param name="preYM"></param>
        /// <param name="preAmount"></param>
        /// <param name="withhold">out </param>
        /// <param name="Amount">out </param>
        public static void Cal_PensionFund(bool PensionFund_Accept, decimal baseSalary, decimal FixExtraWorkPay, decimal SptPay, string curr_YM, string preYM, decimal preAmount, out decimal withhold, out decimal Amount)
        {
            if (PensionFund_Accept)
            {

                withhold = (baseSalary + FixExtraWorkPay + SptPay) * -0.05M;
                int month_stamp = PrtSalaryCal.YMtoCurrMonths(preYM, curr_YM) + 1;
                if (month_stamp > 0)
                {
                    Amount = preAmount + (baseSalary + FixExtraWorkPay + SptPay) * 0.1M * month_stamp;
                }
                else
                {
                    Amount = preAmount;
                    withhold = 0.0M;
                    MessageBox.Show("error Cal_PensionFund,DateError!");
                }
            }
            else
            {
                withhold = 0.0M;
                Amount = preAmount;
            }
        }

        /// <summary>
        /// 計算公式 代課:正規代課(2014/02)
        /// 
        /// <code>
        /// bool TeachingSubsidy = PrtSalaryCal.Cal_SubsidyForTeacherReplacement(
        ///                         childRow, 
        ///                         FieldToInt(SRRow, "SchoolSect"), 
        ///                         FieldToDecimal(SRRow, "baseSalary"));
        /// </code>
        /// <code>
        /// 中(高,初)課堂管理基數 80元 每週工作18日數;小 60元   20日; 幼 60元   23日
        /// SST["pay"] = bSal * 12 / 52 / days * FieldToInt(SST, "St_sectno");//正規代課
        /// SST["pay"] = not_teach_subidy_base * FieldToInt(SST, "St_sectno");//課堂管理
        /// </code>
        /// </summary>
        /// <param name="SST">DATAROW</param>
        /// <param name="SchSect">中小幼代號</param>
        /// <param name="bSal">收入</param>
        /// <param name="sptpay">特別職務報酬</param>
        /// <returns>SST[代課金額]; true正規代課,false課堂管理 </returns>
        public static bool Cal_SubsidyForTeacherReplacement(DataRow SST, int SchSect, Decimal bSal, Decimal sptpay)
        {
            int not_teach_subidy_base = 80;
            int zxy = 0;
            if (SST["St_classno"].ToString().Trim().Length > 0)
            {
                String zx = SST["St_classno"].ToString().Trim().Substring(0, 1);
                if (zx == "高" || zx == "初") { zxy = 1; }
                else if (zx == "小") { zxy = 2; }
                else if (zx == "幼") { zxy = 3; }
            }
            else
            {
                if ((SchSect & 1) > 0) { zxy = 1; } else if ((SchSect & 2) > 0) { zxy = 2; } else if ((SchSect & 8) > 0) { zxy = 3; }
            }
            int days = 18;
            if (zxy == 2) { days = 20; not_teach_subidy_base = 60; } else if (zxy == 3) { days = 23; not_teach_subidy_base = 60; }
            if (SST["St_type"].ToString().Trim() == "正規代課")
            {
                SST["pay"] = (bSal + sptpay) * 12 / 52 / days * putils.FieldToInt(SST, "St_sectno");
                return true;
            }
            else
            {
                SST["pay"] = not_teach_subidy_base * putils.FieldToInt(SST, "St_sectno");
                return false;
            }
        }



        private decimal rounddown(decimal n, int n_digits)
        {
            int b = 1;
            for (int i = 0; i < n_digits; i++)
            {
                b *= 10;
            }
            return Math.Floor(n * b) / b;

        }
        private decimal roundup(decimal n, int n_digits)
        {
            int b = 1;
            for (int i = 0; i < n_digits; i++)
            {
                b *= 10;
            }
            return Math.Ceiling(n * b) / b;
        }
        /// <summary>
        /// 計職業稅以月為計算公式
        /// </summary>
        /// <param name="Taxable_income">decimal (可科稅)總金額</param>
        /// <param name="Year">某年eg: 2012,2013,之后</param>
        /// <returns>該年總稅額</returns>
        public static decimal TaxCalPreMonth(decimal Taxable_income, String Year)
        {
            decimal taxfee = 0.0M;
            if (Year == "2012")
            {
                decimal[] taxgrade = { 0, 116.66M, 250, 550, 1216.66M, 2316.66M };
                decimal[] taxrange = { 12000.00M, 13666.67M, 15333.33M, 18666.67M, 25333.33M, 35333.33M };
                Taxable_income = Math.Ceiling(Taxable_income) * 0.75M;
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
                taxfee = Math.Floor(taxfee * 100) / 100;
                taxfee = Math.Ceiling(taxfee - Math.Floor(taxfee * 0.25M * 100) / 100);
            }
            else if (Year == "2013")
            {
                decimal[] taxgrade = { 0, 116.66M, 250, 550, 1216.66M, 2316.66M };
                decimal[] taxrange = { 12000.00M, 13666.67M, 15333.33M, 18666.67M, 25333.33M, 35333.33M };
                Taxable_income = Math.Ceiling(Taxable_income) * 0.75M;
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
                taxfee = Math.Floor(taxfee * 100) / 100;
                taxfee = Math.Ceiling(taxfee - Math.Floor(taxfee * 0.30M * 100) / 100);
            }
            else
            {
                decimal[] taxgrade = { 0, 116.66M, 250, 550, 1216.66M, 2316.66M };
                decimal[] taxrange = { 12000.00M, 13666.67M, 15333.33M, 18666.67M, 25333.33M, 35333.33M };

                Taxable_income = Math.Floor(Taxable_income * 0.75M * 100) / 100;
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
                taxfee = Math.Floor(taxfee * 100) / 100;
                taxfee = Math.Ceiling(taxfee - Math.Floor(taxfee * 0.30M * 100) / 100);
            }
            return -taxfee;
        }
        /// <summary>
        /// 計職業稅以年為計算公式
        /// </summary>
        /// <param name="Taxable_income">decimal (可科稅)總金額</param>
        /// <param name="year">某年eg: 2012,2013,之后</param>
        /// <returns>該年總稅額</returns>
        public static decimal TaxAjustPreYear(decimal Taxable_income, String year)
        {
            decimal taxfee = 0.0M;

            if (year == "2012")
            {
                decimal[] taxgrade ={ 0M, 1400M,
                    3000M, 6600M,
                    14600M, 27800M };
                decimal[] taxrange ={144000.00M,164000.00M,
                                184000.00M, 224000.00M,
                                304000.00M, 424000.00M};
                Taxable_income = Math.Ceiling(Taxable_income) * 0.75M;
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

                taxfee = Math.Floor(taxfee * 100) / 100;
                taxfee = Math.Ceiling(taxfee - Math.Floor(taxfee * 0.25M * 100) / 100);
            }
            else if (year == "2013")
            {
                decimal[] taxgrade = { 0.00M, 1400.00M, 3000.00M, 6600.00M, 14600.00M, 27800.00M };

                decimal[] taxrange = { 144000.00M, 164000.00M, 184000.00M, 224000.00M, 304000.00M, 424000.00M };

                Taxable_income = Math.Ceiling(Taxable_income) * 0.75M;
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
                taxfee = Math.Floor(taxfee * 100) / 100;
                taxfee = Math.Ceiling(taxfee - Math.Floor(taxfee * 0.30M * 100) / 100);
            }
            else
            {
                decimal[] taxgrade = { 0.00M, 1400.00M, 3000.00M, 6600.00M, 14600.00M, 27800.00M };

                decimal[] taxrange = { 144000.00M, 164000.00M, 184000.00M, 224000.00M, 304000.00M, 424000.00M };

                Taxable_income = Math.Ceiling(Taxable_income) * 0.75M;
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
                taxfee = Math.Floor(taxfee * 100) / 100;
                taxfee = Math.Ceiling(taxfee - Math.Floor(taxfee * 0.30M * 100) / 100);
            }
            return -taxfee;
        }
        private static void NoteDocAddChild(XmlDocument NoteDoc, XmlNode DDnode, String EleName, String textstr)
        {
            XmlNode Fallo = NoteDoc.CreateElement(EleName);
            DDnode.AppendChild(Fallo);
            Fallo.AppendChild(NoteDoc.CreateTextNode(textstr));
        }
        private static void NoteDocAddChild(XmlDocument NoteDoc, XmlNode DDnode, String EleName, String T, String A)
        {
            XmlNode Fallo = NoteDoc.CreateElement(EleName);
            DDnode.AppendChild(Fallo);
            XmlNode TNode = NoteDoc.CreateElement("T");
            Fallo.AppendChild(TNode);
            TNode.InnerText = T;
            TNode = NoteDoc.CreateElement("A");
            Fallo.AppendChild(TNode);
            TNode.InnerText = A;
        }
        private static void NoteDocAddChild(XmlDocument NoteDoc, XmlNode DDnode, String EleName, String T, String A, String N)
        {
            XmlNode Fallo = NoteDoc.CreateElement(EleName);
            DDnode.AppendChild(Fallo);
            XmlNode TNode = NoteDoc.CreateElement("T");
            Fallo.AppendChild(TNode);
            TNode.InnerText = T;
            TNode = NoteDoc.CreateElement("A");
            Fallo.AppendChild(TNode);
            TNode.InnerText = A;
            TNode = NoteDoc.CreateElement("N");
            Fallo.AppendChild(TNode);
            TNode.InnerText = N;
        }

        private static string SchoolNo(int schoolsect)
        {
            string schoolno = "";
            if (schoolsect == 4) { schoolno = "072"; } else { schoolno = "159"; if ((schoolsect & 0x4) > 0) schoolno = "159/072"; }
            return schoolno;
        }
        /// <summary>
        /// 年月 sub 年月 equ 年數
        /// </summary>
        /// <param name="Seinority_YM">年月 xxxx/xx(文字)</param>
        /// <param name="Curr_YM">年月 xxxx/xx(文字)</param>
        /// <returns>年數</returns>
        public static int YMtoCurrYears_Seniority(String Seinority_YM, string Curr_YM)
        {
            string[] YMstr = Seinority_YM.Split('/');
            int year = int.Parse(YMstr[0]);
            int month = int.Parse(YMstr[1]);
            if (year == 0 && month == 0) return 0;
            string[] CYMstr = Curr_YM.ToString().Split('/');
            int cyear = int.Parse(CYMstr[0]);
            int cmonth = int.Parse(CYMstr[1]);
            if (year * 12 + month < 2001 * 12 + 7)
            {
                return (cyear * 12 + cmonth - 2001 * 12 - 7) / 12;
            }
            else
            {
                return (cyear * 12 + cmonth - year * 12 - month) / 12;
            }
        }
        /// <summary>
        /// 年月 sub 年月 equ 月數
        /// </summary>
        /// <param name="YM">年月 xxxx/xx(文字)</param>
        /// <param name="Curr_YM">年月 xxxx/xx(文字)</param>
        /// <returns>月數</returns>
        public static int YMtoCurrMonths(String YM, String Curr_YM)
        {
            int months = 0;
            try
            {
                string[] YMstr = YM.ToString().Split('/');
                int year = int.Parse(YMstr[0]);
                int month = int.Parse(YMstr[1]);
                string[] CYMstr = Curr_YM.ToString().Split('/');
                int cyear = int.Parse(CYMstr[0]);
                int cmonth = int.Parse(CYMstr[1]);
                months = cyear * 12 + cmonth - year * 12 - month;
            }
            catch (Exception e1)
            {
                MessageBox.Show(String.Format("YMtoCurrMonths:YM{0},CYM{1}:{2}", YM, Curr_YM, e1.Message));
            }
            if (months < 0) throw new Exception(String.Format("公積金起期日期有誤{0}currYM{1}", YM, Curr_YM));
            return months;
        }
        /// <summary>
        /// 設置糧單文字描述
        /// 由某一月起始結束x月x日~x月x日
        /// 糧單備註一
        /// 糧單備註二
        /// </summary>
        /// <param name="txtNote1">糧單備註一</param>
        /// <param name="txtNote2">糧單備註二</param>
        /// <param name="txtSalary_period">糧單由某一月起始結束x月x日~x月x日</param>
        public void SetNoteAndSalary_Period(String txtNote1, String txtNote2, String txtSalary_period)
        {
            note1 = txtNote1;
            note2 = txtNote2;
            salary_period = txtSalary_period;
        }
    }
}
