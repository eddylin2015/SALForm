using SAL.salcalc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SAL.forms
{
    public class HTMLRPT
    {
        public static void saL_Note_XML_tab(XmlNodeList fa_List,ref int i,List<string> incomeothers)
        {
            String fmt = "<td>({0}){1}<br />{2}</td><td class=r>{3}</td>";
             foreach (XmlNode node in fa_List)
                {
                    XmlNode a = node.SelectSingleNode("T");
                    XmlNode b = node.SelectSingleNode("A");
                    foreach (FieldDefSt s in sl_pub.FixAlloItem)
                    {
                        if (s.FieldCName.Equals(a.InnerText)) goto loop1;
                    }
                    i++;
                    incomeothers.Add(string.Format(fmt,i, a.InnerText,"", string.Format("{0,8}", b.InnerText) ));
                loop1:;
                }
        }
        public static string saL_tab(List<string> Inctab,int i,String tabname,StringBuilder incomeothers)
        {
                int modtwo= Inctab.Count % 2;
                for(int _r_i=0;_r_i<Inctab.Count / 2 + modtwo; _r_i++)
                {
                    incomeothers.Append("<tr>"+Inctab[_r_i]);
                    if(Inctab.Count / 2 + _r_i <Inctab.Count)
                    {incomeothers.Append(Inctab[Inctab.Count / 2 + _r_i]);}
                    else
                    {incomeothers.Append("<td>----</td><td class=r>---</td>");}
                }
                return String.Format("$('#{2}_{0}').find('tbody').append('{1}');", i,incomeothers.ToString(),tabname);
        }
        public static string out_html(String file_name, DataTable dt, String template)
        {
            String fileContents = System.IO.File.ReadAllText(Pub.cfg.getAppPath() + @"\salaryform.html", Encoding.Default);
            if (template != null)
                fileContents = System.IO.File.ReadAllText(Pub.cfg.getAppPath() + @"\" + template, Encoding.Default);
            String _datajsfilename = String.Format("salaryform.data{0}.js", DateTime.Now.ToString("HHmmss"));
            String datafile = Pub.cfg.getAppPath() + @"\" + _datajsfilename;
            StreamWriter salayformjs = new StreamWriter(datafile, false, Encoding.Default);
            salayformjs.WriteLine("$(document).ready(function(){");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if(dt.Rows[i]["Sa_id"].ToString().Equals("9999")) continue;
                string[] ouptfields={"c_name","WorkDept","Sa_id","Staf_ref","FSS_NO","TAX_NO",
                "IncomeAmount","WithholdAmount","AdjustAmount","Net_income","PensionFund_Amount",
                "FSS_Fee","Tax","PensionFund_withhold","Leave_withhold","Salary_Period","YM","Adjust_tax"};
                foreach(string o_fieldname in ouptfields)
                {
                salayformjs.WriteLine("$('#{0}_{1}').text('{2}');",o_fieldname, i, dt.Rows[i][o_fieldname].ToString().Trim().Replace('\r',' '));
                }
                salayformjs.WriteLine("$('#T_income0_{0}').text('{1}');", i, putils.FieldToDecimal(dt.Rows[i], "IncomeAmount"));
                salayformjs.WriteLine("$('#T_withhold_{0}').text('{1}');", i, putils.FieldToDecimal(dt.Rows[i], "WithholdAmount"));
                salayformjs.WriteLine("$('#T_Adj_total_{0}').text('{1}');", i,putils.FieldToDecimal(dt.Rows[i], "AdjustAmount"));
                List<string> Inctab=new List<string>();
                List<string> Adjtab=new List<string>();
                List<string> Whhtab=new List<string>();
                int inc_cnt = 0;
                int adj_cnt = 0;
                int whh_cnt = 0;
                StringBuilder incomeothers = new StringBuilder();
                StringBuilder adjothers =  new StringBuilder();
                StringBuilder withholdothers =  new StringBuilder();
                String fmt = "<td>({0}){1}<br />{2}</td><td class=r>{3}</td>";
                foreach (FieldDefSt s in sl_pub.FixAlloItem)
                {
                    decimal d = putils.FieldToDecimal(dt, i, s.FieldName);
                    if (d > 0M)
                        switch(s.AlloType)
                        {
                            case 0:
                            case 1:
                            case 2:inc_cnt++;Inctab.Add(String.Format(fmt,inc_cnt,s.FieldCName,s.FieldEName,d));break;
                            case 3:adj_cnt++;Adjtab.Add(String.Format(fmt,adj_cnt,s.FieldCName,s.FieldEName,d));break;
                            case 4:whh_cnt++;Whhtab.Add(String.Format(fmt,whh_cnt,s.FieldCName,s.FieldEName,d));break;
                        }
                }
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(dt.Rows[i]["Note"].ToString());
                XmlNodeList fa_List = xmlDoc.SelectNodes("dd/FAllo");
                XmlNodeList va_List = xmlDoc.SelectNodes("dd/VAllo");
                XmlNodeList adj_List = xmlDoc.SelectNodes("dd/ADJU");
                saL_Note_XML_tab(fa_List,ref inc_cnt,Inctab);
                saL_Note_XML_tab(va_List,ref inc_cnt,Inctab);
                saL_Note_XML_tab(adj_List,ref inc_cnt,Adjtab);

                salayformjs.WriteLine(saL_tab(Inctab,i,"IncTab",incomeothers));
                salayformjs.WriteLine(saL_tab(Adjtab,i,"AdjTab",adjothers));
                salayformjs.WriteLine(saL_tab(Whhtab,i,"WhhTab",withholdothers));
               /* for (int j = 0; j < dt.Columns.Count; j++)
                {
                    String feildname = dt.Columns[j].ColumnName;
                    if (feildname == "Note") continue;
                    salayformjs.WriteLine("$('#{0}_{1}').text('{2}');", feildname, i, dt.Rows[i][feildname].ToString().Trim().Replace('\r', ' '));
                }*/
            }
            salayformjs.WriteLine("});");
            salayformjs.Flush();
            salayformjs.Close();
            salayformjs.Dispose();
            StreamWriter sw = new StreamWriter(file_name, false, Encoding.Default);
            sw.WriteLine(@"<!DOCTYPE html>
<html>
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=big5"">
    <link type=""text/css"" href=""salaryform.css"" rel=""stylesheet"" />	
    <script src=""jquery-1.7.2.js""></script>
    <script src=""jquery-ui-1.8.18.custom.min.js""></script>");
            sw.WriteLine("<script src=\"{0}\"></script>", _datajsfilename);
            sw.WriteLine(@" </head>
<body>");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sw.WriteLine("<div style=\"page-break-before: always\">");
                sw.WriteLine(fileContents.Replace("keyid", i.ToString()));
            }
            sw.WriteLine("</body></html>");
            sw.Flush();
            sw.Close();
            sw.Dispose();
            return datafile;
        }
    }
}
