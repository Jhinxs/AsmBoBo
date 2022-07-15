using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmBoBo.x86
{
    class x86main
    {
        public static List<string> typedef = new List<string>();
        public static List<string> mainfunc = new List<string>();
        public static string ParseSimulationCode(string cppcode)
        {
            typedef.Clear();
            mainfunc.Clear();
            bool intypedefsection = false;
            bool inmainsection = false;
            string[] SimulationCodeLines = cppcode.Split('\n');
            foreach (var code in SimulationCodeLines)
            {

                if (code.Equals("[typdef]") || intypedefsection)
                {
                    typedef.Add(code);
                    intypedefsection = true;
                }
                if (code.Equals("[main]") || inmainsection)
                {
                    mainfunc.Add(code);
                    intypedefsection = false;
                    inmainsection = true;
                }

            }
            typedef.Remove("[typdef]");
            typedef.Remove("[main]");
            typedef.Remove("");
            mainfunc.Remove("[main]");
            mainfunc.Remove("");
            string mainasm = PareseModuleAndFunc();
            string ASMCODE = ASMString.ASMBaseStringX86 + mainasm + ASMString.ASMRET;
            return ASMCODE;
        }
        public static string PareseModuleAndFunc()
        {
            string mainasm = string.Empty;
            int suffixnum = 0;
            foreach (var func in mainfunc)
            {
                string funcnamesuffix = $"@{suffixnum.ToString()}";
                if (func != string.Empty)
                {
                    string funcname = func.Split('(')[0];
                    foreach (var def in typedef)
                    {
                        if (def != string.Empty)
                        {
                            string typedeffuncname = def.Split(' ')[2];
                            string modulename = def.Split(' ')[4];
                            if (typedeffuncname == funcname)
                            {
                                string[] swapstring = func.Split('(');
                                string paratemp = swapstring[swapstring.Length - 1].Trim(';').Trim(')');
                                string[] paramarry = paratemp.Split(',');
                                int paramnum = paramarry.Length;
                                List<string> param = new List<string>();
                                foreach (var a in paramarry)
                                {
                                    param.Add(a.Trim());
                                }
                                mainasm += ASMDynamicGen.GetASMCodeWithModule(modulename, funcname, funcnamesuffix, param, paramnum);
                            }
                        }
                    }
                }
                suffixnum++;
            }

            return mainasm;
        }
    }
}
