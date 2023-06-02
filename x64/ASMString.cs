using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmBoBo.x64
{
    internal class ASMString
    {
        public static string ASMBaseStringX64 =
            "\tpush rax\n\t" +
            "push rcx\n\t" +
            "push rdx\n\t" +
            "push rbx\n\t" +
            "push rsp\n\t" +
            "push rbp\n\t" +
            "push rsi\n\t" +
            "push rdi\n\t" +
            "MOV RAX,GS:[60H]\n\t" +
            "MOV RAX,[RAX+18H]\n\t" +
            "MOV RAX,[RAX+20H]\n\t" +
            "MOV RAX,[RAX]\n\t" +
            "MOV RAX,[RAX]\n\t" +
            "MOV RAX,[RAX+20H]\n\t" +
            "MOV R13,RAX\n\t" +
            "MOV R12,RAX                                          ;R12=IMAGEBASE\n\t" +
            "XOR RAX,RAX\n\t" +
            "MOV AL,BYTE PTR [R13+3CH]                            ;RAX=NTHEADER\n\t" +
            "MOV EAX,DWORD PTR [RAX+R13+88H]                      ;RAX=OPTHEADER\n\t" +
            "ADD RAX,R13                                          ;RAX=EXTABLE\n\t" +
            "MOV RDI,RAX;\n\t" +
            "ADD RAX,20H                                 \n\t" +
            "MOV EDX ,DWORD PTR [RAX]\n\t" +
            "ADD R13,RDX\n\t" +
            "MOV RDX,R13                                   ;RDX=RVAFOREVERYFUCNAME\n\t" +
            "MOV R13,R12\n\t" +
            "XOR RCX,RCX\n\t" +
            "MOV RSI,41636f7250746547H\n_LOOP:\n\t" +
            "MOV EBX,DWORD PTR [RDX]\n\t" +
            "ADD R13,RBX                                    ;[R13]=AcquireSRWLockExclusive\n\t" +
            "CMP QWORD PTR [R13],RSI\n\t" +
            "JNE _INCRSI\n\t" +
            "CMP BYTE PTR [R13+14],00\n\t" +
            "jNE _INCRSI\n\t" +
            "JMP _FOUND\n" +
            "_INCRSI:\n\t" +
            "INC RCX\n\t" +
            "MOV R13,R12\n\t" +
            "ADD RDX,4\n\t" +
            "JMP _LOOP\n" +
            "_FOUND:\n\t" +
            "MOV EBX,DWORD PTR [RDI+24H]\n\t" +
            " ADD RBX,R12;\n\t" +
            "MOV CX,WORD PTR [RBX+2*RCX]\n\t" +
            "MOV EBX,DWORD PTR [RDI+1CH]\n\t" +
            "ADD RBX,R12\n\t" +
            "MOV EAX,DWORD PTR [RBX+RCX*4]\n\t" +
            "ADD RAX,R12                           ;RAX=GetProcAddress\n\t" +
            "MOV RSI,RAX\n\t" +
            "PUSH 0\n\t" +
            "MOV DWORD PTR [RSP],41797261H\n\t" +
            "MOV DWORD PTR [RSP-4],7262694cH\n\t" +
            "MOV DWORD PTR [RSP-8],64616f4cH\n\t" +
            "SUB RSP,8\n\t" +
            "MOV RCX,R12\n\t" +
            "MOV RDX,RSP\n\t" +
            "SUB RSP,28h\n\t" +
            "CALL RSI\n\t" +
            "MOV RBX,RAX\n\t" +
            "ADD RSP,38h\n\t\n\t" +
            "mov rax, rsp                         ;对齐\n\t" +
            "MOV RCX,0FFFFFFF0h\n\t"+
            "and rsp, RCX\n\t"+
            "cmp rax, rsp\n\t" +
            "MOV RSP,RAX\n" +
            "jne _ALIGN\n\t" +
            "jmp _XOR\n" +
            "_ALIGN:\n\t" +
            "PUSH 12344321H\n" +
            "_XOR:\n\t" +
            "XOR RAX,RAX\n\n";



        public static string ASMRET =
                "mov rcx, [rsp]\n\t" +
                "cmp rcx, 12344321h\n\t" +
                "jne _Restore\n\t" +
                "add rsp,8\n" +
                "_Restore:\n\t"+
                "pop rdi\n\t" +
                "pop rsi\n\t" +
                "pop rbp\n\t" +
                "pop rsp\n\t" +
                "pop rbx\n\t" +
                "pop rdx\n\t" +
                "pop rcx\n\t" +
                "pop rax\n\t"+
                "ret\n";
    }

}
