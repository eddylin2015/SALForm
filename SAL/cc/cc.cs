using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
 * 除法優先高於乘法
 *解釋運算式編譯成逆波蘭式
 */
namespace SAL.cc
{
    public enum itemtype { opt, data };
    class TreeItem
    {
        public TreeItem Perant = null;
        public TreeItem LChild = null;
        public TreeItem RChild = null;
        public string Data = null;
        public char Opt;
        public itemtype ittype;
        public TreeItem(char opt)
        {
            ittype = itemtype.opt;
            Opt = opt;
        }
        public TreeItem(string data)
        {
            ittype = itemtype.data;
            Data = data;
        }
    }
    class CCMachine
    {
         int lineDataCnt = 0;
         StreamWriter rw;
         TextWriter output;
        public  Hashtable KeySigns = new Hashtable();　//關鍵符號集如：+-*/()=;
        //處理 一目運算符如 負數
         void proc1opt(List<string> li)
        {
            List<int> indexs = new List<int>();
            int preindex = 0;
            for (int i = 0; i < li.Count; i++)
            {
                if (li[i].Length == 1 && KeySigns.Contains(li[i][0]))
                {
                    if (li[i][0] == '-' && preindex + 1 == i)
                    {
                        indexs.Add(i);
                    }
                    else
                    {
                        preindex = i;
                    }
                }
            }
            for (int i = 0; i < indexs.Count; i++)
            {
                li[indexs[i] + 1] = li[indexs[i]] + li[indexs[i] + 1];
            }
            for (int i = indexs.Count - 1; i >= 0; i--)
            {
                li.RemoveAt(indexs[i]);
            }
        }
        //處理 二目運算符如 = + - * / ( ) 生成樹狀型式
         TreeItem proc2opt(List<string> l1, TreeItem prenode)
        {
            TreeItem it;
            if (l1[0][0] == '(' && l1[l1.Count - 1][0] == ')')
            {
                l1.RemoveAt(l1.Count - 1);
                l1.RemoveAt(0);
            }
            if (l1.Count <= 1)
            {
                it = new TreeItem(l1[0]);
                it.Perant = prenode;
                it.LChild = null;
                it.RChild = null;
                return it;
            }
            else
            {
                int optCnt = 0;
                char preOpt = ';';
                int preOptIndex = 0;
                int dataCnt = 0;
                int preOptGrade = 0;
                int optGrade = 0;
                foreach (string s in l1)
                {
                    if (s.Length == 1 && KeySigns.Contains(s[0]))
                    {
                        if (s[0] == '(') { optGrade += 10; }
                        if (s[0] == ')') { optGrade -= 10; }

                        if ((optCnt) == 0) { preOptGrade = optGrade; preOpt = s[0]; preOptIndex = optCnt + dataCnt; }
                        //    Console.WriteLine("{0}__{1}__{2}__{3}__{4}", s[0], (int)KeySigns[s[0]]+optGrade, preOpt, (int)KeySigns[preOpt]+preOptGrade, preOptIndex);
                        if (preOptGrade < 10 && (int)KeySigns[s[0]] + optGrade >= (int)KeySigns[preOpt] + preOptGrade && preOptIndex != optCnt + dataCnt)
                        {
                            break;
                        }
                        else
                        {
                            preOpt = s[0]; preOptIndex = optCnt + dataCnt; preOptGrade = optGrade;
                        }
                        optCnt++;
                    }
                    else
                    {
                        dataCnt++;
                    }
                }
                it = new TreeItem(l1[preOptIndex][0]);
                it.Perant = prenode;
                it.LChild = proc2opt(l1.GetRange(0, preOptIndex), it);
                it.RChild = proc2opt(l1.GetRange(preOptIndex + 1, l1.Count - preOptIndex - 1), it);
                return it;
            }
        }
        //遍歷整棵樹并輸出文件rule.out
        void travTree(TreeItem ti)
        {
            if (ti.ittype == itemtype.opt && ti.Opt == '=')
            {
                output.Write(ti.LChild.Data);
                output.Write(ti.Opt);
                rw.Write(ti.LChild.Data);
                rw.Write(ti.Opt);
                travTree(ti.RChild);
            }
            else if (ti.ittype == itemtype.data)
            {
                lineDataCnt++;
                if (lineDataCnt == 1)
                {
                    rw.Write("{0}", ti.Data);
                    output.Write("{0}", ti.Data);
                }
                else
                {
                    rw.Write("|{0}", ti.Data);
                    output.Write("|{0}", ti.Data);
                }
            }
            else if (ti.ittype == itemtype.opt)
            {
                travTree(ti.LChild);
                travTree(ti.RChild);
                rw.Write("|{0}", ti.Opt.ToString());
                output.Write("|{0}", ti.Opt.ToString());
            }
        }
         void printList(List<string> l)
        {
            foreach (string s in l)
            {
                output.Write(" {0}", s);
            }
            output.WriteLine();
        }
       
        public static void run_stream(string s,StreamWriter _rrw, TextWriter _output){
            CCMachine cc=new CCMachine();
            cc.output=_output;
            cc.rw=_rrw;
            cc.KeySigns.Add('\t', 0);
            cc.KeySigns.Add('\n', 0);
            cc.KeySigns.Add('\r', 0);
            cc.KeySigns.Add(';', -1);
            cc.KeySigns.Add(' ', 0);
            cc.KeySigns.Add('=', 1);
            cc.KeySigns.Add('(', 6);
            cc.KeySigns.Add(')', 6);
            cc.KeySigns.Add('*', 4);
            cc.KeySigns.Add('/', 5);
            cc.KeySigns.Add('+', 3);
            cc.KeySigns.Add('-', 3);
            cc.KeySigns.Add('?', 2);
            cc.KeySigns.Add(':', 2);
            cc.KeySigns.Add(',', 2);
            int beginIndex = 0; //詞的起始位置
            int endIndex = 0;//詞的結束位置
            List<string> exprLine = new List<string>();
            for (int i = 0; i < s.Length; i++)
            {
                if (cc.KeySigns.Contains(s[i]))
                {
                    if (beginIndex < endIndex)
                    {
                        exprLine.Add(s.Substring(beginIndex, endIndex - beginIndex));
                    }
                    switch ((int)cc.KeySigns[s[i]])
                    {
                        case -1:
                            cc.proc1opt(exprLine);
                            cc.printList(exprLine);
                            TreeItem f = cc.proc2opt(exprLine, null);
                            cc.travTree(f);
                            exprLine.Clear();
                            cc.rw.WriteLine();
                            cc.output.WriteLine();
                            cc.lineDataCnt = 0;
                            break;
                        case 0: break;
                        case 1: exprLine.Add(s[i].ToString()); break;
                        case 2: exprLine.Add(s[i].ToString()); break;
                        case 3: exprLine.Add(s[i].ToString()); break;
                        case 4: exprLine.Add(s[i].ToString()); break;
                        case 5: exprLine.Add(s[i].ToString()); break;
                        case 6: exprLine.Add(s[i].ToString()); break;
                    }
                    beginIndex = i + 1;
                    endIndex = i + 1;
                }
                else
                {
                    endIndex++;
                }
            }
            cc.rw.Flush();
        }
        public static void run_console(string[] args)
        {
            StreamReader rdr;
            StreamWriter rrw;
            if (args.Length > 0)
            {
                rdr = new StreamReader(args[0]);
            }
            else
            {
                rdr = new StreamReader("rule.in");
            }
            if (args.Length > 1)
            {
                rrw = new StreamWriter(args[1]);
            }
            else
            {
                rrw = new StreamWriter("rule.out");
            }
            string rule_i=rdr.ReadToEnd();
            rdr.Close();
            run_stream(rule_i,rrw,Console.Out)  ;
            rrw.Close();
        }
         public static string run_cc(String rule_i,TextWriter _output)
        {
            _output.WriteLine(rule_i);
              string res="";
            using(MemoryStream memStream = new MemoryStream(rule_i.Length*2)){
            StreamWriter rrw=new StreamWriter(memStream);
            run_stream(rule_i,rrw,_output);
            _output.WriteLine(
                "Capacity = {0}, Length = {1}, Position = {2}\n",
                memStream.Capacity.ToString(),
                memStream.Length.ToString(),
                memStream.Position.ToString());
            memStream.Seek(0, SeekOrigin.Begin);
            byte[] byteArray = new byte[memStream.Length];
            int count = memStream.Read(byteArray, 0,(int) memStream.Length);
             while(count < memStream.Length)
             {
               count += memStream.Read(byteArray, 0, 20);
              }
            res=System.Text.Encoding.Default.GetString(byteArray);
            _output.WriteLine(res);
            rrw.Close();
            memStream.Close();
            }
            return res;
        }
    }
}
