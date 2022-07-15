using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmBoBo.x86
{
    class ASMDynamicGen
    {
        public static int X86_ALIGN = 4;
        public static string GetASMCodeWithModule(string modulename, string funcname, string funcnamesuffix, List<string> param, int numberofparam)
        {
            string ASMWithModule = string.Empty;


            if (modulename.Equals("kernel32.dll"))
            {
                ASMWithModule = ASMGenWithKernel32(funcname, funcnamesuffix, param, numberofparam);
            }
            else
            {
                ASMWithModule = ASMGenWithOrtherModule(modulename, funcname, funcnamesuffix, param, numberofparam);
            }
            return ASMWithModule;
        }
        public static string ASMGenWithKernel32(string funcname, string funcnamesuffix, List<string> param, int numberofparam)
        {


            string CallFunctionContent = GenerateCallFunctionX86(funcname, funcnamesuffix, true);

            string FunctionParamContent = GenerateFunctionParamX86(param, numberofparam);
            string funcmainasm = CallFunctionContent + "  \n" + FunctionParamContent + "\n";
            return funcmainasm;
        }
        public static string ASMGenWithOrtherModule(string modulename, string funcname, string funcnamesuffix, List<string> param, int numberofparam)
        {
            string LoadlibraryContent = ASMLoadlibrary(funcname, modulename, funcnamesuffix);
            string CallFunctionContent = GenerateCallFunctionX86(funcname, funcnamesuffix, false);
            string FunctionParamContent = GenerateFunctionParamX86(param, numberofparam);
            string funcmainasm = LoadlibraryContent + " \n" + CallFunctionContent + "  \n" + FunctionParamContent + "\n";
            return funcmainasm;
        }
        public static string ASMLoadlibrary(string funcname, string modulename, string funcnamesuffix)
        {
            string content = funcname + $"{funcnamesuffix}:  \n"; ;
            int clearStack = StringToHexString(modulename, X86_ALIGN).Count * X86_ALIGN;

            List<string> ModuleNameHex = StringToHexString(modulename, X86_ALIGN);
            for (int i = 0; i < ModuleNameHex.Count; i++)
            {
                content += $"push {ModuleNameHex[i]}h  \n";
            }
            content += "push esp  \n";
            content += "call ebx  \n";
            content += CheckHEX_CH(clearStack);
            return content;
        }

        public static string GenerateFunctionParamX86(List<string> param, int numberofparam)
        {
            string content = string.Empty;
            param.Reverse();

            int framecount = 0;

            List<int> paramsize = new List<int>();
            List<string> paramtype = new List<string>();
            for (int i = 0; i < param.Count; i++)
            {
                int result;
                if (param[i].ToString().ToLower().Equals("null") || param[i].ToString().ToLower().Equals("0"))                           //参数为null
                {
                    content += $"push 0   \n";
                    paramsize.Add(sizeof(Int32));
                    paramtype.Add("int32");

                }
                else if (Int32.TryParse(param[i], out result))                              //参数为整形
                {

                    content += $"push { Int32.Parse(param[i]).ToString("x")}h   \n";
                    paramsize.Add(sizeof(Int32));
                    paramtype.Add("int32");
                }

                else                                                                      //参数为字符串类型               
                {
                    string swaptemp = string.Empty;
                    List<List<string>> paramstrhex = new List<List<string>>();
                    try
                    {
                        swaptemp = param[i].Trim('"');
                    }
                    catch { }
                    paramstrhex.Add(StringToHexString(swaptemp, X86_ALIGN));                        //返回字符串16进制小端模式字符串列表
                    paramsize.Add(StringToHexString(swaptemp, X86_ALIGN).Count * X86_ALIGN);      //返回字符串大小
                    paramtype.Add("string");                                                      //添加参数类型
                    for (int j = 0; j < paramstrhex.Count; j++)
                    {
                        List<string> swaplist = (List<string>)paramstrhex[j];
                        foreach (var a in swaplist)
                        {
                            if (a.Equals("00"))
                            {
                                content += $"push 0  \n";
                            }
                            else
                            {
                                content += $"push {a}h  \n";
                            }

                        }
                    }

                }

            }

            CalcEspOffset(paramsize, paramtype);
            content += CalcEspOffset(paramsize, paramtype) + "  \n";
            content += "call eax  \n";
            string clearStack = string.Empty;
            // clearStack+= $"add esp,{CalcClearStack(paramsize).ToString("x")}h  \n  \n";
            content += CheckHEX_CH(CalcClearStack(paramsize));
            content += clearStack;

            return content;
        }
        public static string CalcEspOffset(List<int> paramsize, List<string> paramtype)
        {
            string content = string.Empty;
            int pushcount = 0;                         //push ecx 次数
            List<string> offsetlist = new List<string>();
            for (int i = 0; i < paramsize.Count; i++)
            {
                int offset = 0;
                if (i == 0)                     //第一个参数
                {

                    offset = paramsize.Skip(i + 1).Take(paramsize.Count - (i + 1)).Sum();
                    content += ASMLeaOrMov(paramtype[i], offset.ToString("x"));
                    pushcount += 1;
                }
                else if (i != paramsize.Count - 1)
                {
                    offset = paramsize.Skip(i + 1).Take(paramsize.Count - (i + 1)).Sum() + X86_ALIGN * pushcount;
                    content += ASMLeaOrMov(paramtype[i], offset.ToString("x"));
                    offsetlist.Add(offset.ToString());
                    pushcount += 1;
                }
                else                              //最后一个参数
                {
                    offset = X86_ALIGN * pushcount;

                    content += ASMLeaOrMov(paramtype[i], offset.ToString("x"));
                    offsetlist.Add(offset.ToString());
                }

            }

            return content;
        }
        public static string ASMLeaOrMov(string type, string offset)
        {
            string content = string.Empty;
            if (offset.ToLower().Equals("c"))
            {
                offset = "0" + offset;
            }
            if (type.Equals("int32"))
            {
                content += $"mov ecx,[esp+{offset}h]  \n";
            }
            else if (type.Equals("string"))
            {
                content += $"lea ecx,[esp+{offset}h]  \n";
            }
            content += "push ecx  \n";
            return content;
        }

        public static String GenerateCallFunctionX86(string funcname, string funcnamesuffix, Boolean iskernel32)
        {
            string content = string.Empty;
            if (iskernel32)
            {
                content += funcname + $"{funcnamesuffix}:  \n";
            }
            int clearStack = StringToHexString(funcname, X86_ALIGN).Count * X86_ALIGN;

            List<string> funcnamehex = StringToHexString(funcname, X86_ALIGN);
            for (int i = 0; i < funcnamehex.Count; i++)
            {
                content += $"push {funcnamehex[i]}h  \n";
            }
            content += "push esp  \n";
            if (iskernel32)
            {
                content += "push edi  \n";
            }
            else
            {
                content += "push eax  \n";
            }
            content += "call esi  \n";
            content += CheckHEX_CH(clearStack);
            return content;
        }
        public static string CheckHEX_CH(int clearstack)
        {
            string content = string.Empty;
            if (clearstack.ToString("x").ToLower().Equals("c"))
            {
                content += $"add esp,0{clearstack.ToString("x")}h  \n";
            }
            else
            {
                content += $"add esp,{clearstack.ToString("x")}h  \n";
            }
            return content;
        }

        public static List<string> StringToHexString(string temp, int subsize)
        {

            byte[] b = Encoding.Default.GetBytes(temp.ToCharArray());
            byte[] newArray = new byte[b.Length + 1];
            b.CopyTo(newArray, 0);
            newArray[b.Length] = 0x00;
            List<byte[]> subArrayList = splitAry(newArray, subsize);
            List<string> hexstringlist = new List<string>();
            foreach (byte[] by in subArrayList)
            {
                string bytestring = BitConverter.ToString(by).Replace("-", "");
                hexstringlist.Add(bytestring);
            }
            hexstringlist.Reverse();
            return hexstringlist;

        }
        public static List<byte[]> splitAry(byte[] ary, int subSize)
        {
            int count = ary.Length % subSize == 0 ? ary.Length / subSize : ary.Length / subSize + 1;
            List<byte[]> subAryList = new List<byte[]>();
            for (int i = 0; i < count; i++)
            {
                int index = i * subSize;
                byte[] subary = ary.Skip(index).Take(subSize).ToArray();
                Array.Reverse(subary);
                subAryList.Add(subary);
            }
            return subAryList;
        }
        public static int CalcClearStack(List<int> paramsize)
        {
            return paramsize.Sum();

        }
    }
}
