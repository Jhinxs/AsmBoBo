using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace AsmBoBo.x64
{

    internal class ASMDynamicGen
    {

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
            string AsmForFindFunctionStr = AsmForFindFunction(funcname, funcnamesuffix, true);
            //string 
            string AsmForFunctionCallStr = AsmForFunctionCall(param, numberofparam,true);
            string AsmFuncWithKernel32 = AsmForFindFunctionStr + "\n\t" + AsmForFunctionCallStr + "\n\t";
            return AsmFuncWithKernel32;
        }
        public static string ASMGenWithOrtherModule(string modulename, string funcname, string funcnamesuffix, List<string> param, int numberofparam)
        {
            string AsmForLoadlibrary = ASMLoadlibrary(funcname, modulename, funcnamesuffix);
            string AsmForFindFunctionStr = AsmForFindFunction(funcname, funcnamesuffix, false);
            string AsmForFunctionCallStr = AsmForFunctionCall(param, numberofparam,false);
            string ASMGenWithOrtherModule = AsmForLoadlibrary + "\n\t"+ AsmForFindFunctionStr+"\n\t" + AsmForFunctionCallStr + "\n\t";
            return ASMGenWithOrtherModule;
        }

        public static string ASMLoadlibrary(string funcname, string modulename, string funcnamesuffix)
        {
            string content = $"{modulename.Replace(".dll","")}@{funcname}:  \n\t";

            content += ManagerStack(modulename, out int subrsp, out int addrsp,false);
            content += $"mov rcx,rsp\n\t";
            content += $"sub rsp,{subrsp}\n\t";
            content += $"call rbx\n\t";
            content += $"mov r14,rax\n\t";
            content += $"add rsp,{addrsp}\n\t";
            return content;
        }

        public static bool IsString(string v)
        {
            int result;
            if (Int32.TryParse(v, out result))
            {
                return false;
            }
            return true;
        }
        public static string HandleParamWithCD89(string regname,string param,out int addrsp,out int subrsp)
        {
            addrsp = 0;
            subrsp = 0;
            string content = string.Empty;
            
            
            if (param.ToString().ToLower().Equals("null") || param.ToString().ToLower().Equals("0"))
            {
                subrsp = 48;
                addrsp = 48;
                content += $"mov {regname},00h\n\t";
            }
            else if (!IsString(param))
            {
                subrsp = 48;
                addrsp = 48;
                content += $"mov {regname},{param}\n\t";
            }
            else
            {

                string swapstr = param.Trim('"');
                content += ManagerStack(swapstr,out int subrsptmp, out int addrsptmp,true);
                content += $"mov {regname},rsp\n\t";
                addrsp = addrsptmp;
                subrsp = subrsptmp;

            }
            return content;
        }
        static int BaseStackSize = 0x40;
        static int cd89pushcount = 0;
        public static string AsmForFunctionCall(List<string> param, int numberofparam,bool iskernel32)
        {
            string content = string.Empty;
            int addrspsum = 0;
            int subrspsum = 0;
            int addrsp, subrsp;

            for (int i = 0; i < param.Count; i++)
            {
                if (i <= 3)
                {
                    switch (i)
                    {
                        //CD89传参
                        case 0:
                            content += HandleParamWithCD89("rcx", param[i],out addrsp,out subrsp);
                            addrspsum += addrsp;
                            subrspsum += subrsp;
                            break;
                        case 1:
                            content += HandleParamWithCD89("rdx", param[i], out addrsp, out subrsp);
                            addrspsum += addrsp;
                            subrspsum += subrsp;
                            break;
                        case 2:
                            content += HandleParamWithCD89("r8", param[i], out addrsp, out subrsp);
                            addrspsum += addrsp;
                            subrspsum += subrsp;
                            break;
                        case 3:
                            content += HandleParamWithCD89("r9", param[i], out addrsp, out subrsp);
                            addrspsum += addrsp;
                            subrspsum += subrsp;
                            break;
                    }
                }
                else
                {
                    //堆栈传参
                    //从第五个参数开始从右到左压栈
                    List<string> StackParam = param.Skip(4).Reverse().ToList();
                    content += HandleParamWithStack(StackParam,StackParam.Count, out addrsp, out subrsp);
                    addrspsum += addrsp;
                    subrspsum += subrsp;
                }

            }
            if (iskernel32)
            {
                content += $"sub rsp,{subrspsum}\n\t";
                content += "call rdi\n\t";
                content += $"add rsp,{addrspsum}\n";
            }
            else 
            {
                content += "call rdi\n\t";
                content += $"add rsp, {BaseStackSize + cd89pushcount * 8}\n";
            }

            return content;
        }
        public static string HandleParamWithStack(List<string> StackParam, int ParmCount, out int addrsp, out int subrsp)
        {
            addrsp = 0;
            subrsp = 0;
            string content = string.Empty;
            int baseframe = 0x20;


            content += $"sub rsp, {BaseStackSize + (StackParam.Count - 1) * 0x8}\n\t";
            for (int i = 0; i < StackParam.Count; i++) 
            {
                if (StackParam[i].ToString().ToLower().Equals("null") || StackParam[i].ToString().ToLower().Equals("0"))
                {
                    content += $"mov qword ptr [rsp+{baseframe+i*0x8}],00h\n\t";

                }
                else if (!IsString(StackParam[i]))
                {
                    long result;
                    Int64.TryParse(StackParam[i], out result);
                    content += $"mov qword ptr [rsp+{baseframe + i * 0x8}], {result.ToString("x")}h\n\t";
     
                }
                else 
                {
                    ManagerStack(StackParam[i],out subrsp,out addrsp,true);
                }
            }
            return content;
        }
        public static string AsmForFindFunction(string funcname, string funcnamesuffix, Boolean iskernel32)
        {
            string content = string.Empty;
            if (iskernel32)
            { 
                content += funcname + $"{funcnamesuffix}:  \n\t";
            }
            content += ManagerStack(funcname,out int subrsp, out int addrsp,false);
            if (iskernel32)
            {
                content += "mov rcx,r12\n\t";
            }
            else 
            {
                content += "mov rcx,r14\n\t";
            }
            content += "mov rdx,rsp\n\t";
            content += $"sub rsp,{subrsp}\n\t";
            content += "call rsi\n\t";
            content += "mov rdi,rax\n\t";
            
            content += $"add rsp, {addrsp}\n\t";

            return content;
        }
        public static string ManagerStack(string str, out int subrsp, out int addrsp,bool cd89) 
        {
            string content = string.Empty;
            List<string> strhex = StringToHexString(str, 8);
            strhex.Insert(0, "00");
            
            for (int i = 0; i < strhex.Count; i++)
            {
                content += $"mov rax,{strhex[i]}h\n\t";
                content += "push rax\n\t";
            }
            subrsp = 48;
            if (strhex.Count % 2 != 0)
            {
                subrsp += 8;
            }
            addrsp = subrsp + strhex.Count * 8;
            if (cd89)
            {
                cd89pushcount += strhex.Count;
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
    }
}
