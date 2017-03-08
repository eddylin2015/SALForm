using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SAL.salcalc
{
    public partial class PrtSalaryCal
    {
        /// <summary>
        /// 請假計算
        /// <code>
        /// SRRow["Leave_withhold"]=Calc_Leave(SRRow,
        ///                                     fulltimework, 
        ///                                     summer_pay, 
        ///                                     leave_days_total, 
        ///                                     weekworkSections, 
        ///                                     leave_sections_total);
        ///    //全職告假 (7月份)有夏令金  if (summer_pay > 100)
        ///            //7月份處理=夏令金 / 30 * sum(sl_days where =july)
        ///            return -summer_pay / 30 * 告假天數leave_days_total;
        ///    //全職 代扣無薪假=(基本薪金+年資+固定津貼)/30*sum(sl_days where=curr month)
        ///            return  -(FieldToDecimal(SRRow, "baseSalary")
        ///                + FieldToDecimal(SRRow, "FixExtraWorkPay")
        ///                + FieldToDecimal(SRRow, "Seniority")
        ///                + FieldToDecimal(SRRow, "F_allowance")
        ///                ) / 30 * leave_days_total;
        ///    //非全職若每周總節數>0, 代扣無薪假=(基本薪金/周節/4)*sum(sl_sections where=curr month)
        ///        if (weekworkSections > 0)
        ///            return -FieldToDecimal(SRRow, "baseSalary") / 4 / weekworkSections * leave_sections_total;
        ///    //非全職若每周總節數為0, Excepton ("非全職請設定每周總節數. 不能設為0;否不作無薪假扣薪計算!\n" + SRRow["Staf_ref"].ToString() + SRRow["c_name"].ToString() + "非全職代扣無薪假=(基本薪金/周節" + weekworkSections + "/4)*sum(sl_sections where=curr month)\n", "嚴重錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        ///            return 0M;
        /// </code>
        /// </summary>
        /// <param name="SRRow">datarow</param>
        /// <param name="fulltimework">true fulltime work,false parttime work</param>
        /// <param name="summer_pay">夏令金</param>
        /// <param name="leave_days_total">請假天數</param>
        /// <param name="weekworkSections">每週節數</param>
        /// <param name="leave_sections_total">請假節數</param>
        /// <returns>Decimal</returns>
        public static decimal Calc_Leave(DataRow SRRow, bool fulltimework, decimal summer_pay, decimal leave_days_total, int weekworkSections, int leave_sections_total)
        {
            if (fulltimework)
            {   //全職告假
                if (summer_pay > 100)
                {
                    //7月份處理=夏令金/30 * sum(sl_days where =july)
                    return -summer_pay / 30 * leave_days_total;
                }
                else
                {
                    //全職 代扣無薪假=(基本薪金+年資+固定津貼)/30*sum(sl_days where=curr month)
                    return -(putils.FieldToDecimal(SRRow, "baseSalary")
                        + putils.FieldToDecimal(SRRow, "FixExtraWorkPay")
                        + putils.FieldToDecimal(SRRow, "SptPay")
                        + putils.FieldToDecimal(SRRow, "Seniority")
                        + putils.FieldToDecimal(SRRow, "F_allowance")
                        ) / 30 * leave_days_total;
                }
            }
            else
            {
                //非全職代扣無薪假=(基本薪金/周節/4)*sum(sl_sections where=curr month)
                if (weekworkSections > 0)
                {
                    return -(putils.FieldToDecimal(SRRow, "baseSalary") + putils.FieldToDecimal(SRRow, "SptPay")) / 4 / weekworkSections * leave_sections_total;
                }
                else
                {
                    MessageBox.Show("非全職請設定每周總節數. 不能設為0;否不作無薪假扣薪計算!\n" + SRRow["Staf_ref"].ToString() + SRRow["c_name"].ToString() + "非全職代扣無薪假=(基本薪金/周節" + weekworkSections + "/4)*sum(sl_sections where=curr month)\n", "嚴重錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0M;
                }
            }
        }
        private void Append_Allo_XML(XmlDocument NoteDoc, XmlNode DDnode, String Tag, DataRow childRow, String FV_)
        {
            String tag_name = "";
            String amt_name = "";
            if (FV_ == "F_" || FV_ == "V_")
            {
                tag_name = "Allowance"; amt_name = "Amount";
            }
            else if (FV_ == "AJ") { tag_name = "Adjustment"; amt_name = "AJ_Amount"; }
            NoteDocAddChild(NoteDoc, DDnode, Tag, childRow[tag_name].ToString(), childRow[amt_name].ToString(), childRow["note"].ToString());
        }
        private String Add_Allo_Amt(DataRow SRRow, DataRow childRow, String FV_, StringBuilder warning_msg)
        {
            if (AllocItem_Dict.ContainsKey(childRow["Allowance"].ToString()))
            {
                if (putils.FieldToDecimal(SRRow, AllocItem_Dict[childRow["Allowance"]].ToString()) > 0.0m)
                {
                    MessageBox.Show(SRRow["c_name"].ToString() + childRow["Allowance"].ToString()
                        + AllocItem_Dict[childRow["Allowance"]].ToString() + "重複登記!");
                    //累加 津貼金額 F_Amount
                    SRRow[AllocItem_Dict[childRow["Allowance"]].ToString()] = putils.FieldToDecimal(SRRow, AllocItem_Dict[childRow["Allowance"]].ToString())
                        + putils.FieldToDecimal(childRow, "Amount");
                }
                else
                {
                    SRRow[AllocItem_Dict[childRow["Allowance"]].ToString()] = putils.FieldToDecimal(childRow, "Amount");
                }
            }
            else
            {
                warning_msg.Append(String.Format("WARNING:{0}{1}津貼{2}{3},未登記外文名!\n", SRRow["staf_ref"], FV_, childRow["Allowance"], childRow["Amount"]));
            }
            return "";
        }

        /// <summary>
        /// 薪金計算
        /// <code>
        /// 初始基本基料
        /// Stafinfo子表 WORKPOSI WORKDEPT FSS_NO TAX_NO SCHOOL_NO WORKTYPE->Salary_Rec
        /// baseinfo子表 Seniority_YM Seniority_years Seniority Week_Section Pensionfund_Accept->Salary_Rec
        /// 計年資  Seniority+Seniority_base*Seniority_years
        /// 固定津貼 totalFAllowance=SUM(F_Amount)
        /// 非固定津貼totalVAllowance=SUM(V_Amount)
        /// 調整 totalAdjustAmount
        /// 告假 leave_days_total
        ///        if (fulltimework)
        ///            if (summer_pay > 100)
        ///                //7月份處理=夏令金/30* sum(sl_days where =july)       SRRow["Leave_withhold"] = -summer_pay / 30 * leave_days_total;
        ///            else
        ///                //全職 代扣無薪假=(基本薪金+年資+固定津貼)/30*sum(sl_days where=curr month)
        ///                SRRow["Leave_withhold"] = -(FieldToDecimal(SRRow, "baseSalary") 
        ///                    + FieldToDecimal(SRRow,"FixExtraWorkPay")  + FieldToDecimal(SRRow,"Seniority")
        ///                    + FieldToDecimal(SRRow,"F_allowance")  ) / 30 * leave_days_total;
        ///        else
        ///            //非全職代扣無薪假=(基本薪金/周節/4)*sum(sl_sections where=curr month)
        /// </code>
        /// </summary>
        public void Cal()
        {
            StringBuilder warning_msg = new StringBuilder();
            StringBuilder info_msg = new StringBuilder();
            ///////////////////////////////////////////////////////////////
            //                     Main Table Salary_Rec (YM)
            foreach (DataRow SRRow in Sal_Rec_DS.Tables["Salary_Rec"].Rows)
            {
                ///////////////////////////////////////////////////////////
                //            初始化變量 清零 和 設FALSE
                //////////////////////////////////////////////////////////
                decimal summer_pay = 0;   //夏令金  : 若夏令金>0,用夏令金計算請假扣薪;
                                          //否則由基本薪金+固定津貼計算請假扣薪
                int weekworkSections = 0; //每周工作節數:非全職每周節數,計請假扣薪計算每周節數.
                bool fulltimework = false;//全職工作:T 全職 , F 非全職(不需要供公積金)
                bool PensionFund_Accept = false;//公積金供款標誌:T 有供公積金  F 不需要供公積金
                ////////////////////////////////////////////////////////////
                //                     津貼欄位清零
                //  SRRow[AlloType_Name] <-0   
                //  except : baseSalary && FixExtraWorkPay                    
                ////////////////////////////////////////////////////////////
                foreach (FieldDefSt s in salcalc_pub.FixAlloItem)
                {
                    if (s.AlloType > 0 || s.FieldName.Equals("Seniority"))
                    {
                        if (!(s.FieldName == "baseSalary" || s.FieldName == "FixExtraWorkPay" || s.FieldName == "SptPay"))
                        {
                            SRRow[s.FieldName] = 0;
                            System.Diagnostics.Debug.WriteLine(s.FieldName);
                        }
                    }
                    System.Diagnostics.Debug.WriteLine(s.FieldName);
                }
                //////////////////////////////////////////////////////////////////////////////////////////
                //                     XML格式記錄津貼表.1:N Sub Items {allo, adj, leave}
                //                     SRRow[Note] <- ( NoteDoc ->Add ChildNode{allo|adj|leave}).Text();
                //////////////////////////////////////////////////////////////////////////////////////////                     
                XmlDocument NoteDoc = new XmlDocument();
                NoteDoc.AppendChild(NoteDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                XmlNode DDnode = NoteDoc.CreateElement("dd"); NoteDoc.AppendChild(DDnode);
                //////////////////////////////////////////////////////////////////
                //             工作職位,部門,校部,全職否,Fss no,Tax No
                ///////////////////////////////////////////////////////////////////             
                foreach (DataRow childRow in SRRow.GetChildRows("sr_stafinfo"))
                {
                    SRRow["FSS_NO"] = childRow["FSS_No"].ToString();
                    SRRow["TAX_NO"] = childRow["Tax_no"].ToString();
                    SRRow["School_No"] = SchoolNo(int.Parse(childRow["schoolsect"].ToString()));
                    SRRow["SchoolSect"] = childRow["schoolsect"].ToString();
                    //全職工作
                    if (childRow["WorkType"].ToString().Equals("全職"))
                    { fulltimework = true; }
                    else { fulltimework = false; }
                }
                foreach (DataRow childRow in SRRow.GetChildRows("sr_baseinfo"))
                {
                    ////////////////////////////////////////////////////////
                    //文字描述
                    ///////////////////////////////////////////////////////////
                    if (salary_period != null) SRRow["Salary_period"] = salary_period;
                    if (note1 != null) SRRow["note1"] = note1; if (note2 != null) SRRow["note2"] = note2;
                    /////////////////////////////////////////////////////////
                    //每周工作節數
                    weekworkSections = putils.FieldToInt(childRow, "Week_Section");
                    /////////////////////////////////////////////////////////
                    //                 公積金供款標誌
                    /////////////////////////////////////////////////////////
                    if (childRow["PensionFund_Accept"].Equals(DBNull.Value) || !childRow["PensionFund_Accept"].ToString().Equals("1"))
                    { PensionFund_Accept = false; }
                    else { PensionFund_Accept = true; }
                    /////////////////////////////////////////////////////////
                    //                         年資計算
                    ///////////////////////////////////////////////////////
                    int Seniority_years = YMtoCurrYears_Seniority(childRow["Seniority_YM"].ToString(), YM);
                    info_msg.Append("S_Years:" + Seniority_years.ToString());
                    childRow["Seniority_years"] = Seniority_years;
                    SRRow["Seniority"] = putils.FieldToDecimal(childRow, "Seniority_base") * Seniority_years;
                }
                //////////////////////////////////////////////////////////////
                //                         
                //                 不使用 sr_historySeniority    (暫時保留code)
                //                         
                foreach (DataRow childRow in SRRow.GetChildRows("sr_historySeniority"))
                {
                    SRRow["Seniority"] = putils.FieldToDecimal(SRRow, "Seniority")
                        + putils.FieldToDecimal(childRow, "Seniority_base") * putils.FieldToInt(childRow, "Seniority_years");
                }
                #region prompt
                info_msg.Append(String.Format("姓名:{0}{1}({2}) 職位:{3,-12} 部門:{4,-12} 校部:{5,-4} 全職:{6,-6} 社保編號:{7,-6} 納稅編號:{8,-8}\n ",
                    SRRow["c_name"], SRRow["e_name"], SRRow["staf_ref"], SRRow["workposi"], SRRow["workdept"], SRRow["School_No"], fulltimework,
                    SRRow["FSS_NO"], SRRow["TAX_NO"]));
                info_msg.Append(String.Format("年月:{0,-8} 基本薪金:{1,9:0.00} 加班費:{2,8:0,0.00} 年資:{3,8:0,0.00}\n",
                    SRRow["YM"], SRRow["baseSalary"], SRRow["FixExtraWorkPay"], SRRow["Seniority"]));
                #endregion
                //////////////////////////////////////////////////////////////
                //                             固定津貼 -非固定 
                decimal totalFAllowance = 0M;
                decimal totalVAllowance = 0M;
                foreach (DataRow childRow in SRRow.GetChildRows("sr_FAllowance"))
                {
                    if (childRow["fx"].ToString().Equals("1"))
                    {
                        info_msg.Append(String.Format("固津:{0}{1}\t{2}\n", childRow["Allowance"], childRow["note"], childRow["Amount"]));
                        if (!childRow["Amount"].Equals(DBNull.Value))
                        {
                            decimal famount = putils.FieldToDecimal(childRow, "Amount");
                            ///////////////////////////////////////////////////////
                            //  SRRow[F_Allowance]<-F_Amount;
                            Add_Allo_Amt(SRRow, childRow, "F_", warning_msg);
                            totalFAllowance += famount;
                            ////////////////////////////////////////////////////////
                            //  NoteDoc->AddNode(Fallo,F_Allowance,F_Amount);
                            Append_Allo_XML(NoteDoc, DDnode, "FAllo", childRow, "F_");
                            ////////////////////////////////////////////////////////
                            //  if(F_Allowance==夏令金) summer_pay <- famount;
                            if (childRow["Allowance"].ToString().Equals("夏令金")) summer_pay = famount;
                        }
                    }
                    else if (childRow["fx"].ToString().Equals("2"))
                    {
                        info_msg.Append(String.Format("非固津:{0}{1}\t{2}\n", childRow["Allowance"], childRow["note"], childRow["Amount"]));
                        if (!childRow["Amount"].Equals(DBNull.Value))
                        {
                            decimal vamount = putils.FieldToDecimal(childRow, "Amount");
                            Add_Allo_Amt(SRRow, childRow, "V_", warning_msg);
                            totalVAllowance += vamount;
                            Append_Allo_XML(NoteDoc, DDnode, "VAllo", childRow, "V_");
                            //summer_pay夏令金(非固定津貼)
                            if (childRow["Allowance"].ToString().Equals("夏令金")) summer_pay = vamount;
                        }
                    }
                }
                SRRow["F_allowance"] = totalFAllowance;
                if (totalFAllowance > 0M) info_msg.Append(String.Format("固津小計:{0,7:0,0.00}\n", totalFAllowance));
                SRRow["V_allowance"] = totalVAllowance;
                if (totalVAllowance > 0M) info_msg.Append(String.Format("非固津小計:{0,7:0,0:00}\n", totalVAllowance));
                ////////////////////////////////////////////////////////////////
                //                             非固定津貼
                /*
                foreach (DataRow childRow in SRRow.GetChildRows("sr_VAllowance"))
                {
                    info_msg.Append(String.Format("非固津:{0}{1}\t{2}\n", childRow["V_Allowance"], childRow["note"], childRow["V_Amount"]));
                    if (!childRow["V_Amount"].Equals(DBNull.Value))
                    {
                        decimal vamount = putils.FieldToDecimal(childRow, "V_Amount");
                        Add_Allo_Amt(SRRow, childRow, "V_", warning_msg);
                        totalVAllowance += vamount;
                        Append_Allo_XML(NoteDoc, DDnode, "VAllo", childRow, "V_");
                        //summer_pay夏令金(非固定津貼)
                        if (childRow["V_Allowance"].ToString().Equals("夏令金")) summer_pay = vamount;
                    }
                }
                */
                //////////////////////////////////////////////////////////////////////
                //                              調整
                decimal totalAdjustAmount = 0;
                decimal TaxableAdjust = 0;
                foreach (DataRow childRow in SRRow.GetChildRows("sr_ADJUST"))
                {
                    info_msg.Append(String.Format("調整:{0}{1}\t{2}\n", childRow["Adjustment"], childRow["note"], childRow["AJ_Amount"]));
                    decimal vamount = putils.FieldToDecimal(childRow, "AJ_Amount");
                    totalAdjustAmount += vamount;
                    Append_Allo_XML(NoteDoc, DDnode, "ADJU", childRow, "AJ");
                    if (!childRow["UnTaxable"].Equals(DBNull.Value) && int.Parse(childRow["UnTaxable"].ToString()) == 0)
                        TaxableAdjust += vamount;
                }
                SRRow["AdjustAmount"] = totalAdjustAmount;
                if (totalAdjustAmount != 0M)
                    info_msg.Append(String.Format("小計:調整{0,7:0,0:00} 調整(需交稅) {1,7:0,0:00}\n", totalAdjustAmount, TaxableAdjust));
                //////////////////////////////告假
                decimal leave_days_total = 0.0M;
                int leave_sections_total = 0;
                //////////////////////////////////
                foreach (DataRow childRow in SRRow.GetChildRows("sr_Leave"))
                {
                    if (!childRow["Sl_days"].Equals(DBNull.Value)) leave_days_total += decimal.Parse(childRow["Sl_days"].ToString());
                    if (!childRow["Sl_sections"].Equals(DBNull.Value)) leave_sections_total += int.Parse(childRow["Sl_sections"].ToString());
                    string textstr = String.Format("{0:d} {1}天數/{2}節數 備註{3}", childRow["Start_Date"], childRow["Sl_days"].ToString(), childRow["Sl_sections"].ToString(),
                                                                          childRow["Sl_note"].ToString());
                    info_msg.Append("告假:" + textstr + "\n");
                    NoteDocAddChild(NoteDoc, DDnode, "LEAVE", textstr);
                }
                info_msg.Append("告假天數:" + leave_days_total + "\n");
                info_msg.Append("告假節數:" + leave_sections_total + "\n");
                SRRow["Leave_withhold"] = Calc_Leave(SRRow, fulltimework, summer_pay, leave_days_total, weekworkSections, leave_sections_total);
                if (putils.FieldToDecimal(SRRow, "Leave_withhold") != 0M) info_msg.Append(String.Format("告假小計:{0,7:0,0.00}\n", SRRow["Leave_withhold"]));

                //////////////////////////////////////////////////////////////
                //                                                  代課
                decimal Teaching_subsidy_for_teacher_replacement = 0.0M;
                decimal Not_teaching_subsidy_for_teacher_replacement = 0.0M;
                ////////////////////////////////////////////////////////////
                foreach (DataRow childRow in SRRow.GetChildRows("sr_SubstituteTeach"))
                {
                    bool TeachingSubsidy = Cal_SubsidyForTeacherReplacement(childRow, putils.FieldToInt(SRRow, "SchoolSect"), putils.FieldToDecimal(SRRow, "baseSalary"), putils.FieldToDecimal(SRRow, "SptPay"));
                    ////////////////////////////////////////////////
                    //      update Sub_table(sl_substituteTeach.pay) 
                    DataRow[] drs = { childRow };
                    SST_Adapter.Update(drs);
                    childRow.AcceptChanges();
                    /////////////////////////////////////////////////
                    if (TeachingSubsidy)
                    {
                        Teaching_subsidy_for_teacher_replacement += putils.FieldToDecimal(childRow, "pay");
                    }
                    else
                    {
                        Not_teaching_subsidy_for_teacher_replacement += putils.FieldToDecimal(childRow, "pay");
                    }
                    String textstr = string.Format("{0}\t{1}\t{2}\t{3}\n",
                         childRow["St_date"].ToString(),
                         childRow["St_classno"].ToString(),
                         childRow["St_sectno"].ToString(),
                         childRow["pay"].ToString()
                         );
                    info_msg.Append("代課" + textstr);
                    NoteDocAddChild(NoteDoc, DDnode, "SubstituteTeach", textstr);
                }
                SRRow["Teaching_subsidy_for_teacher_replacement"] = Teaching_subsidy_for_teacher_replacement;
                SRRow["Not_teaching_subsidy_for_teacher_replacement"] = Not_teaching_subsidy_for_teacher_replacement;
                if ((Teaching_subsidy_for_teacher_replacement + Not_teaching_subsidy_for_teacher_replacement) != 0M)
                    info_msg.Append(String.Format("代課小計:{0,7:0,0.00}\n",
                        (Teaching_subsidy_for_teacher_replacement + Not_teaching_subsidy_for_teacher_replacement)));

                /////////////////////////////////////////////////////////////////////
                //                            社保,稅務,公積金
                ////////////////////////////////////////////////////////////////////
                decimal Taxable_income = 0.0M;    //可科稅_收入總金額
                Taxable_income =
                    putils.FieldToDecimal(SRRow, "baseSalary") +   //基本收入
                    putils.FieldToDecimal(SRRow, "FixExtraWorkPay") +//固定加班費
                    putils.FieldToDecimal(SRRow, "SptPay") +//固定加班費
                    putils.FieldToDecimal(SRRow, "Seniority") +//年資
                    putils.FieldToDecimal(SRRow, "F_allowance") +//固定津貼
                   putils.FieldToDecimal(SRRow, "V_allowance") +//非固定津貼
                    putils.FieldToDecimal(SRRow, "Leave_withhold") +//請假,為負數
                    putils.FieldToDecimal(SRRow, "Teaching_subsidy_for_teacher_replacement") +//代課
                    putils.FieldToDecimal(SRRow, "Not_teaching_subsidy_for_teacher_replacement") +//代課
                    TaxableAdjust;//職業稅調整(調整分需可稅和不需可稅);
                //FieldToDecimal(SRRow, "FSS_Fee") +//15元社保
                SRRow["Taxable_Income"] = Taxable_income;
                SRRow["Tax"] = TaxCalPreMonth(Taxable_income, YM.Substring(0, 4));//按月公式計算

                ///////////////////////////////////////////////////////////////////////////////////
                ////                                扣減公積金
                ////fulltimework && PensionFund_Accept
                ///////////////////////////////////////////////////////////////////////////////////
                decimal PFwithhold = 0.0M;
                decimal PFamount = 0.0M;
                //flltimework &&  // 20140328
                if (PensionFund_Accept && putils.FieldToDecimal(SRRow, "PensionFund_withhold") == 0.0M)
                {
                    Cal_PensionFund(
                        PensionFund_Accept,
                        putils.FieldToDecimal(SRRow, "baseSalary"),
                        putils.FieldToDecimal(SRRow, "FixExtraWorkPay"),
                        putils.FieldToDecimal(SRRow, "SptPay"),
                        YM,
                        SRRow["PensionFund_curr_YM"].ToString(),
                        putils.FieldToDecimal(SRRow, "PensionFund_preAmount"),
                        out PFwithhold,
                        out PFamount);
                    SRRow["PensionFund_withhold"] = PFwithhold;
                    SRRow["PensionFund_Amount"] = PFamount;
                }
                else
                {
                    Cal_PensionFund(
                        PensionFund_Accept,
                        putils.FieldToDecimal(SRRow, "baseSalary"),
                        putils.FieldToDecimal(SRRow, "FixExtraWorkPay"),
                        putils.FieldToDecimal(SRRow, "SptPay"),
                        YM,
                        SRRow["PensionFund_curr_YM"].ToString(),
                        putils.FieldToDecimal(SRRow, "PensionFund_preAmount"),
                        out PFwithhold,
                        out PFamount);
                    SRRow["PensionFund_withhold"] = PFwithhold;
                    SRRow["PensionFund_Amount"] = PFamount;
                }

                ///////////////////////////////////////////////
                String tempStr = NoteDoc.InnerXml;
                SRRow["Note"] = tempStr;

                ///////////////////////////////////////////////
                //              Net Income 出糧金額
                //////////////////////////////////////////////
                SRRow["Net_income"] = putils.FieldToDecimal(SRRow, "baseSalary") +
                    putils.FieldToDecimal(SRRow, "FSS_Fee") +
                   putils.FieldToDecimal(SRRow, "Seniority") +
                    putils.FieldToDecimal(SRRow, "F_allowance") +
                    putils.FieldToDecimal(SRRow, "V_allowance") +
                    putils.FieldToDecimal(SRRow, "Leave_withhold") +
                    putils.FieldToDecimal(SRRow, "Teaching_subsidy_for_teacher_replacement") +
                    putils.FieldToDecimal(SRRow, "Not_teaching_subsidy_for_teacher_replacement") +
                    putils.FieldToDecimal(SRRow, "Tax") +
                    putils.FieldToDecimal(SRRow, "Adjust_tax") +
                    putils.FieldToDecimal(SRRow, "AdjustAmount") +
                    putils.FieldToDecimal(SRRow, "FixExtraWorkPay") +
                    putils.FieldToDecimal(SRRow, "SptPay") +
                    putils.FieldToDecimal(SRRow, "PensionFund_withhold") +
                    putils.FieldToDecimal(SRRow, "LeaveJob_Comp_PF") +
                    putils.FieldToDecimal(SRRow, "LeaveJob_Pers_PF");
                ///////////////////////////////////////////////
                //          Update Salary_Rec
                ///////////////////////////////////////////////
                DataRow[] arrDR = new DataRow[1];
                arrDR[0] = SRRow;
                Adapter.Update(arrDR);
                int tempYMToCurrMonths = 0;
                if (fulltimework && PensionFund_Accept)
                    tempYMToCurrMonths = (YMtoCurrMonths(SRRow["PensionFund_curr_YM"].ToString(), YM) + 1);
                info_msg.Append(
                    String.Format("社保:{0,6:0,0.00} 可課稅收入:{1,10:0,0.00} 職業稅扣款{2,9:0,0.00} 期初供款公積金額:{3,10:0,0.00} 當前由{5}計{6}個月\n"
                    + "公積金供款(學校供款加上個人供款累積總額){7,10:0,0.00} 公積金個人供款{8,8:0,0.00} 出糧金額{9,8:0,0.00}\n",
                    SRRow["FSS_Fee"], SRRow["Taxable_income"], SRRow["Tax"], SRRow["PensionFund_preAmount"], 0, SRRow["PensionFund_curr_YM"], tempYMToCurrMonths,
                    SRRow["PensionFund_Amount"], SRRow["PensionFund_withhold"], SRRow["Net_income"]
                    ));
                info_msg.Append("-----------------------------\n");
            }
            if (warning_msg.ToString() != "")
            {
                MessageBox.Show(warning_msg.ToString());
            }
            forms.PubInfoBox pib = new forms.PubInfoBox();
            pib.memo.AppendText(info_msg.ToString());
            pib.ShowDialog();
        } 
    }
    public class putils
    {
        /// <summary>
        /// 好用工具.  fieldToDecimal
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="i"></param>
        /// <param name="incName"></param>
        /// <returns></returns>
        public static decimal FieldToDecimal(DataTable dt, int i, String incName)
        {
            if (dt.Columns.Contains(incName))
            {
                if (dt.Rows[i][incName].Equals(DBNull.Value))
                {
                    return 0M;
                }
                else
                {
                    return decimal.Parse(dt.Rows[i][incName].ToString());
                }
            }
            else
            {
                return 0M;
            }
        }
        /// <summary>
        /// 如果null 為 0
        /// </summary>
        /// <param name="dr">DATAROW</param>
        /// <param name="fieldname">FIELDNAME</param>
        /// <returns>DECIMAL</returns>
        public static decimal FieldToDecimal(DataRow dr, string fieldname)
        {
            if (dr[fieldname].Equals(DBNull.Value))
            {
                return 0M;
            }
            else
            {
                return decimal.Parse(dr[fieldname].ToString());
            }
        }
        /// <summary>
        /// 如果null 為 0
        /// </summary>
        /// <param name="dr">DATAROW</param>
        /// <param name="fieldname">FIELDNAME</param>
        /// <returns>整數</returns>
        public static int FieldToInt(DataRow dr, string fieldname)
        {
            if (dr[fieldname].Equals(DBNull.Value))
            {
                return 0;
            }
            else
            {
                return int.Parse(dr[fieldname].ToString());
            }
        }
    }
}
