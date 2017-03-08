using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace SAL.cc
{
    static class Program
    {
        /// <summary>
        /// </summary>

        static void Main()
        {
		//example 1
        //String[] par={};
		//CCMachine.run_console(par);
        
        //example 2
        StreamReader rdr = new StreamReader("rule.in");
        string rule_i=rdr.ReadToEnd();
        rdr.Close();
        string rule_o=CCMachine.run_cc(rule_i,Console.Out);
        Console.WriteLine(rule_o);
        SAL.cx.SampleVarMgr tvm = new SAL.cx.SampleVarMgr();
        SAL.cx.CXMachine.run_cx(tvm,rule_o,Console.Out);
        Console.WriteLine("-------output--------");
        foreach (DictionaryEntry entry in tvm.getHashtable())
            {
                Console.WriteLine(entry.Key + " = " + entry.Value);
            }
            Console.WriteLine("---------------");
        }
    }
}

