using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmBoBo.x86
{
    class ASMString
    {
		public static string ASMBaseStringX86 =
		 "xor eax, eax						            \n" +
		 //";ASSUME FS:NOTHING	    ;Increment the ordinal  			        \n";
		 "mov eax, fs:[eax+30h]						    \n" +
		 //";ASSUME FS:ERROR		  ;masm编译时取消开头注释				    \n" +
		 "mov eax, [eax + 0ch]					        \n" +
		 "mov eax, [eax + 14h]					        \n" +
		 "mov eax, [eax]							    \n" +
		 "mov eax, [eax]							    \n" +
		 "mov eax, [eax + 10h]                  ; EAX = kernel32.base \n \n" +

		 "xor edx, edx                                  \n" +
		 "mov edx, eax                                  \n" +
		 "mov edi, eax                                  \n \n" +

		 "xor eax, eax								    \n" +
		 "mov eax, [edx + 3ch]						    \n" +
		 "mov eax, [eax + edx + 78h]				    \n" +
		 "add eax, edx                          ; EAX = pe.export_table \n \n" +

		 "mov ebx, [eax+20h]											  \n" +
		 "add ebx, edx													  \n" +
		 "mov esi, ebx                          ;ESI = pe.export_address_of_funcname_table \n" +
		 "xor ecx,ecx                                                     \n \n" +

	 "_LOOP:	                                                         \n \n" +

		"add edx,[esi]                          ;EDX = VA_of_funcname    \n" +
		"cmp dword ptr [edx],50746547h          ;string = GetP                    \n" +
		"jne INCESI                                                      \n" +
		"cmp dword ptr[edx + 4],41636f72h       ;string = rocA          \n" +
		"jne INCESI                                                      \n" +
		"cmp dword ptr[edx + 8], 65726464h      ;string = ddre          \n" +
		"jne INCESI                                                      \n" +
		"jmp FOUND                                                       \n \n" +

	"INCESI:                                                            \n \n" +

		"inc ecx                                                       \n" +
		"mov edx, edi                                                  \n" +
		"add esi,4                                                     \n" +
		"jne _LOOP                                                     \n \n" +

	"FOUND:                                                           \n \n" +

		"mov ebx, [eax + 24h]                                          \n" +
		"add ebx, edi                           ;EBX = PE.ordinals_table      \n \n" +

		"mov cx,[ebx + 2 * ecx]                 ;ECX = index_of_addressfunc_table_in_ordinals  \n \n" +

		"mov ebx, [eax + 1ch]                                          \n" +
		"add ebx, edi                           ;EBX = address_of_func_table   \n" +
		"mov eax, [ebx + ecx * 4]                                      \n" +
		"add eax, edi                                                  \n" +
		"mov esi, eax                           ;ESI = GetProcAddress_Function_Address  \n \n" +

		"push 0                                 ; string = '0'             \n" +
		"push 41797261h                         ; string = aryA           \n" +
		"push 7262694ch                         ; string = Libr           \n" +
		"push 64616f4ch                         ; string = Load           \n" +
		"push esp                                                       \n" +
		"push edi                                                       \n" +
		"call esi                                                       \n" +
		"xor ebx,ebx                                                    \n" +
		"mov ebx,eax                                                    \n" +
		"add esp,10h                            ; EBX = LoadlibraryA_Function_Address  \n \n";


		public static string ASMRET =
			   "ret \n";
	}
}
