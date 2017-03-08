using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
/*除法優先高於乘法
 *解釋運算式編譯成逆波蘭式
 */
namespace SAL.cx
{
    /*變數管理器Varibles Manager*/
    interface iVarMgr
    {
        decimal getVar(string varname);
        void setVar(string varname, decimal o);
        bool Contains(string varname);
        Hashtable getHashtable();
        void ADD(string varname, decimal f);
    }
    class DataRowVarMgr : iVarMgr
    {
        private DataRow datarow_dict = null;
        private Hashtable dict = new Hashtable();
        public void ADD(string varname, decimal f)
        {
            dict.Add(varname, f);
        }
        public decimal getVar(string varname)
        {
            message_center.tssl_msg(varname);
            if(datarow_dict.Table.Columns.Contains(varname))
            {
                if(datarow_dict.IsNull(varname)) return 0.0M;
               return (decimal) datarow_dict[varname];    
            }else{
               return (decimal) dict[varname];
            }
        }
        public void setVar(string varname, decimal o)
        {
            if(datarow_dict.Table.Columns.Contains(varname))
            {
                datarow_dict[varname]=o;
            }else{
                dict[varname] = o;
            }
        }
        public DataRowVarMgr(DataRow _dr)
        {
            datarow_dict=_dr;
        }
        public bool Contains(string varname)
        {
            return datarow_dict.Table.Columns.Contains(varname)|| dict.Contains(varname);
        }
        public Hashtable getHashtable()
        {
            return (Hashtable)dict.Clone();
        }
    }
    class SampleVarMgr : iVarMgr
    {
        private Hashtable dict = new Hashtable();
        public void ADD(string varname, decimal f)
        {
            dict.Add(varname, f);
        }
        public decimal getVar(string varname)
        {
            return (decimal)dict[varname];
        }
        public void setVar(string varname, decimal o)
        {
            dict[varname] = o;
        }
        public SampleVarMgr()
        {
        }
        public bool Contains(string varname)
        {
            return dict.Contains(varname);
        }
        public Hashtable getHashtable()
        {
            return (Hashtable)dict.Clone();
        }
    }
    /*運算式中間代碼（逆波蘭式）運算執行器
     */
    class CalcMachine
    {
        public iVarMgr dict = null;//變數管理器
        private List<string> rules = new List<string>(); //運算式子
        private Stack tempStack = new Stack();
        private Stack formuStack = new Stack();   //堆栈
        private Hashtable opSet = new Hashtable();//運算符集
        List<string> Def_Fun=new List<string>();
        public static decimal CalcTax(decimal Taxable_income)
        {
            decimal taxfee = 0.0M;
            decimal[] taxgrade = { 0, 116.66M, 250, 550, 1216.66M, 2316.66M };
            decimal[] taxrange = { 12000.00M, 13666.67M, 15333.33M, 18666.67M, 25333.33M, 35333.33M };
            Taxable_income = Math.Floor(Taxable_income* 0.75M * 100) / 100;
            if (Taxable_income > taxrange[5]) { taxfee = Math.Ceiling(Taxable_income - taxrange[5]) * 0.12M + taxgrade[5];}
            else if (Taxable_income > taxrange[4]){taxfee = (Taxable_income - taxrange[4]) * 0.11M + taxgrade[4];}
            else if (Taxable_income > taxrange[3]){taxfee = (Taxable_income - taxrange[3]) * 0.10M + taxgrade[3];}
            else if (Taxable_income > taxrange[2]){taxfee = (Taxable_income - taxrange[2]) * 0.09M + taxgrade[2];}
            else if (Taxable_income > taxrange[1]){taxfee = (Taxable_income - taxrange[1]) * 0.08M + taxgrade[1];}
            else if (Taxable_income > taxrange[0]){taxfee = (Taxable_income - taxrange[0]) * 0.07M + taxgrade[0];}
            else{taxfee = 0.0M;}
            taxfee = Math.Floor(taxfee* 100) / 100;
            taxfee = Math.Ceiling(taxfee - Math.Floor(taxfee* 0.30M * 100) / 100);
            return -taxfee;
        }
        private decimal calc(Stack f, Stack temp)   //堆栈運算逆波蘭式得出運算後結果
        {
            

            decimal tempres = 0;
            decimal d1 = 0;
            decimal d2 = 0;
            decimal d3 = 0;
            while (f.Count > 0)
            {
                Object o = f.Pop();
                Console.WriteLine(o);
                if(o is string && Def_Fun.Contains(o.ToString()))
                {
                    d1=(decimal)f.Pop();
                    d2=(decimal)f.Pop();
                    f.Pop();
                    f.Pop();
                    tempres=CalcTax(d1);
                    tempStack.Push(tempres);
                }
                else if (o is decimal)
                {
                    tempStack.Push(o);
                }
                else
                {
                    
                    switch (o.ToString()[0])
                    {
                        case '+':
                            d1 = (decimal)tempStack.Pop();
                            d2 = (decimal)tempStack.Pop();
                            tempres = d2 + d1;
                            tempStack.Push(tempres);
                            break;
                        case '-':
                            d1 = (decimal)tempStack.Pop();
                            d2 = (decimal)tempStack.Pop();
                            tempres = d2 - d1;
                            tempStack.Push(tempres);
                            break;
                        case '*':
                            d1 = (decimal)tempStack.Pop();
                            d2 = (decimal)tempStack.Pop();
                            tempres = d2 * d1;
                            tempStack.Push(tempres);
                            break;
                        case '/':
                            d1 = (decimal)tempStack.Pop();
                            d2 = (decimal)tempStack.Pop();
                            tempres = d2 / d1;
                            tempStack.Push(tempres);
                            break;
                        case ':':{
                            
                            Object _o = f.Pop();
                            if (_o is string)
                            {
                                switch(_o.ToString()[0])
                                {
                                    case '?' :
                                        d1 = (decimal)tempStack.Pop();
                                        d2 = (decimal)tempStack.Pop();
                                        d3=(decimal)tempStack.Pop();
                                        tempres = d3 > 0 ? d2 : d1;
                                        tempStack.Push(tempres);
                                        break;
                                   default:
                                        f.Push(_o);
                                        break;
                                }
                            }
                            else
                            {
                                f.Push(_o);
                            }
                            break;}
                       
                    }
                }
            }
            return (decimal)tempStack.Pop();
        }
        /*運算器構造，變數管理器iVarMgr　和　運算式文件名 filename
         */
         public CalcMachine(iVarMgr inst, string rule_o)
         {
            dict = inst;
            //操作符集合
            opSet.Add("+", 1);
            opSet.Add("-", 1);
            opSet.Add("*", 2);
            opSet.Add("/", 2);
            Def_Fun.Add("CalcTax");
            //計算公式集
            string[] lines = rule_o.Split('\n');
            for(int i=0;i<lines.Length;i++)
            {
                lines[i]=lines[i].TrimEnd();
                if(lines[i].Equals("")) continue;
                rules.Add(lines[i]);
            }
         }
        public CalcMachine(iVarMgr inst, StreamReader srRules)
        {
            dict = inst;
            //操作符集合
            opSet.Add("+", 1);
            opSet.Add("-", 1);
            opSet.Add("*", 2);
            opSet.Add("/", 2);
            Def_Fun.Add("CalcTax");
            //計算公式集
            string line = null;
            while ((line = srRules.ReadLine()) != null)
            {
                rules.Add(line);
            }
            srRules.Close();
        }
        /*進行運算
         */
        public void Cal()
        {
            foreach (string s in rules)
            {
                string[] arstr = s.Split('=');
                string varname = arstr[0];
                if (!dict.Contains(varname))
                {
                    dict.ADD(varname, 0.0M);
                }
                Console.WriteLine(varname);
                string[] formu = arstr[1].Split('|');
                for (int i = 0; i < formu.Length; i++)
                {
                    string forum1rule = formu[formu.Length - 1 - i];
                    if (opSet.Contains(forum1rule))
                    {
                        formuStack.Push(forum1rule);
                    }
                    else if (char.IsNumber(forum1rule, 0) || (forum1rule[0] == '-'))
                    {
                        formuStack.Push(decimal.Parse(forum1rule));
                    }
                    else if (dict.Contains(forum1rule))
                    {
                        formuStack.Push(dict.getVar(forum1rule));
                    }
                    else
                    {
                        formuStack.Push(forum1rule);
                    }
                }
                dict.setVar(varname, calc(formuStack, tempStack));
                formuStack.Clear();
                tempStack.Clear();
            }

        }

    }
    class CXMachine
    {
        public  static void run_cx(iVarMgr tvm ,string srRules,TextWriter _output){
            //SampleVarMgr tvm = new SampleVarMgr();
            CalcMachine cs = new CalcMachine(tvm, srRules);
            /*顯示運算式中變數值
             */
            _output.WriteLine("Initalize Variables：");
            foreach (DictionaryEntry entry in tvm.getHashtable())
            {
                _output.WriteLine(entry.Key + " = " + entry.Value);
            }
            _output.WriteLine("---------------");
            cs.Cal();
            /*顯示運算式中變數值
             */
            _output.WriteLine("Values After Calculation：");
            foreach (DictionaryEntry entry in tvm.getHashtable())
            {
                _output.WriteLine(entry.Key + " = " + entry.Value);
            }
            _output.WriteLine("---------------");
      }
      public  static void run_test_cx(string srRules,TextWriter _output){
            SampleVarMgr tvm = new SampleVarMgr();
            CalcMachine cs = new CalcMachine(tvm, srRules);
            /*顯示運算式中變數值
             */
            _output.WriteLine("Initalize Variables：");
            foreach (DictionaryEntry entry in tvm.getHashtable())
            {
                _output.WriteLine(entry.Key + " = " + entry.Value);
            }
            _output.WriteLine("---------------");
            cs.Cal();
            /*顯示運算式中變數值
             */
            _output.WriteLine("Values After Calculation：");
            foreach (DictionaryEntry entry in tvm.getHashtable())
            {
                _output.WriteLine(entry.Key + " = " + entry.Value);
            }
            _output.WriteLine("---------------");
        }
        static void run_console(string[] args)
        {
            SampleVarMgr tvm = new SampleVarMgr();
            CalcMachine cs = null;
            if (args.Length > 0)
            {
                cs = new CalcMachine(tvm, args[0]);
            }
            else
            {
                 System.IO.StreamReader srRules = new StreamReader("rule.out");
                cs = new CalcMachine(tvm, srRules);
            }
            /*顯示運算式中變數值
             */
            Console.WriteLine("Initalize Variables：");
            foreach (DictionaryEntry entry in tvm.getHashtable())
            {
                Console.WriteLine(entry.Key + " = " + entry.Value);
            }
            Console.WriteLine("---------------");
            cs.Cal();
            /*顯示運算式中變數值
             */
            Console.WriteLine("Values After Calculation：");
            foreach (DictionaryEntry entry in tvm.getHashtable())
            {
                Console.WriteLine(entry.Key + " = " + entry.Value);
            }
            Console.WriteLine("---------------");
            Console.ReadLine();
        }
    }
}
