using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Keystone;
using System.IO;
using Microsoft.Win32;

namespace AsmBoBo
{
    /// <summary>
    /// shellcode_enc.xaml 的交互逻辑
    /// </summary>
    public partial class shellcode_enc : Window
    {
        public static byte key1 = 0x3c;
        public static byte key2 = 0x2e;
        public static Boolean x86 = false;
        public static Boolean FileMode = false;
        public static string SMC_ShellCode_Header;
        public static string SMC_ShellCode_Body =
            "        call get_eip                \r\n" +
            "        jmp asm_main                \r\n" +
            "    get_eip:                        \r\n" +
            "        mov ebx,[esp]               \r\n" +
            "        ret                         \r\n" +
            "    asm_main:                       \r\n" +
            "        lea eax,[ebx+0x37]          \r\n" +
            "        mov ebx,eax                 \r\n" +
            "        xor esi, esi                \r\n" +
            "        jmp decode                  \r\n" +
            "    xor_key2 :                      \r\n" +
            "        mov edi,[eax]               \r\n" +
            $"       xor edi, {key2}               \r\n" +
            "                                    \r\n" +
            "        mov[eax],edi                \r\n" +
            
            "        add eax, 1                  \r\n" +
            "        inc esi                     \r\n" +
            "        cmp esi, ecx                \r\n" +
            "        jl decode                   \r\n" +
            "        jmp ebx                     \r\n" +
            "    decode:                         \r\n" +
            "        test esi,1                  \r\n" +
            "        jz xor_key2                 \r\n" +
           
            "        mov edi,[eax]               \r\n" +
            $"       xor edi, {key1}               \r\n" +
            "                                    \r\n" +
            "        mov[eax],edi                \r\n" +
            "        add eax, 1                  \r\n" +
            "        inc esi                     \r\n" +
            "        cmp esi,ecx                 \r\n" +
            "        jl decode                   \r\n";


        public shellcode_enc()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.shellcode_enc_WindowList.RemoveAt(MainWindow.shellcode_enc_WindowList.Count - 1);

        }

        private void x86_select(object sender, RoutedEventArgs e)
        {
            x86 = true;
        }
        private void x64_select(object sender, RoutedEventArgs e)
        {
            x86 = false;
        }
        private void ShellcodeSMC_Select(object sender, RoutedEventArgs e)
        {

        }
        private void ShellcodeBase64_Select(object sender, RoutedEventArgs e)
        {

        }

        private void converter_button_Click(object sender, RoutedEventArgs e)
        {
            String Oridinal_text = new TextRange(original_editor.Document.ContentStart,
             original_editor.Document.ContentEnd).Text.Trim();
            if (String.IsNullOrEmpty(Oridinal_text))
            {
                return;
            }
            switch (Transform_combobox.SelectedIndex) 
            {
                case 0:
                    ciphertext_editor.Text = CipherText_SMC(Oridinal_text);
                    break;
                case 1:
                    break;
            }
        }
        public static string CipherText_SMC(string OriginalText)
        {
            if (FileMode)
            {
                return ReadFromFile(OriginalText)+"\n\n 文件已生成";
            }
            else 
            {
                return ReadFromEditor(OriginalText)+$"\n\n//key1:0x{key1.ToString("x")} key2:0x{key2.ToString("x")}";
            }
           
            
        }
        public static string ReadFromFile(string filepath) 
        {

            byte[] bytes = File.ReadAllBytes(filepath);
            byte[] enc = new byte[] { };
            List<byte> bytelist = new List<byte>();
            int length = bytes.Length;
            string retstring = string.Empty;
            string dir = System.IO.Path.GetDirectoryName(filepath);
            string file = System.IO.Path.GetFileName(filepath);
            string despath = dir + $@"\SMC_{file}";
            if (x86)
            {
                SMC_ShellCode_Header = $"mov ecx,0x{length.ToString("x")} \r\n";
                using (Engine keystone = new Engine(Architecture.X86, Mode.X32) { ThrowOnError = true })
                {
                    ulong address = 0;
                    int size = 0;
                    int count = 0;
                    try
                    {
                        enc = keystone.Assemble(SMC_ShellCode_Header + SMC_ShellCode_Body, address, out size, out count);
                    }
                    catch (Exception ex)
                    {
                        retstring = ex.ToString();

                    }
                }
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        bytes[i] ^= key2;
                    }
                    else
                    {
                        bytes[i] ^= key1;
                    }
                }
                
            }
            File.WriteAllBytes(despath, enc);
            using (var stream = new FileStream(despath, System.IO.FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            return despath;
        }
        public static string ReadFromEditor(string OriginalText) 
        {

            List<byte> OriginalTextList = new List<byte>();
            int length = OriginalText.Trim(',').Split(',').Length;
            string ciphertext = string.Empty;
            foreach (string e in OriginalText.Split(','))
            {

                OriginalTextList.Add(Convert.ToByte(e.Trim().TrimStart('0').TrimStart('x'), 16));
            }
            for (int i = 0; i < OriginalTextList.Count; i++)
            {
                if (i % 2 == 0)
                {
                    OriginalTextList[i] ^= key2;
                }
                else
                {
                    OriginalTextList[i] ^= key1;
                }
            }
            for (int i = 0; i < OriginalTextList.Count; i++)
            {
                if (i != 0 && i % 16 == 0)
                {
                    ciphertext += $"0x{OriginalTextList[i].ToString("x")},\n";
                }
                else
                {
                    ciphertext += $"0x{OriginalTextList[i].ToString("x")},";
                }
            }

            byte[] enc = new byte[] { };
            string SMC_HEADER = string.Empty;
            if (x86)
            {
                string shellcodestring = string.Empty;
                SMC_ShellCode_Header = $"mov ecx,0x{length.ToString("x")} \r\n";
                using (Engine keystone = new Engine(Architecture.X86, Mode.X32) { ThrowOnError = true })
                {
                    ulong address = 0;
                    int size = 0;
                    int count = 0;
                    try
                    {
                        enc = keystone.Assemble(SMC_ShellCode_Header + SMC_ShellCode_Body, address, out size, out count);
                    }
                    catch (Exception ex)
                    {
                        shellcodestring = ex.ToString();

                    }
                }
            }
            for (int i = 0; i < enc.Length; i++)
            {
                if (i != 0 && i % 16 == 0)
                {
                    SMC_HEADER += $"0x{enc[i].ToString("x")},\n";
                }
                else
                {
                    SMC_HEADER += $"0x{enc[i].ToString("x")},";
                }

            }
            return SMC_HEADER + ciphertext.TrimEnd(',');

        }

        private void original_editor_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (Transform_combobox.SelectedIndex) 
            {

                case 0:
                    if (original_editor.ContextMenu.Items.Count == 7)
                    {
                        return;
                    }
                    MenuItem Clear = new MenuItem();
                    Clear.Header = "Clear";
                    Clear.Click += new RoutedEventHandler(ClearAll);
                    original_editor.ContextMenu.Items.Add(Clear);

                    MenuItem Save = new MenuItem();
                    Save.Header = "打开文件";
                    Save.Click += new RoutedEventHandler(ReadFromFileMenu);
                    original_editor.ContextMenu.Items.Add(Save);
                   
                    break;
                case 1:
                    break;
                default:
                    break;
            }
        }
        private void ClearAll(object sender, RoutedEventArgs e) 
        {
            original_editor.Document.Blocks.Clear();
            FileMode = false;
        }

        private void ReadFromFileMenu(object sender, RoutedEventArgs e) 
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Filter = "file(*)|*";
            openfile.ShowDialog();
            original_editor.Document.Blocks.Clear();
            original_editor.AppendText(openfile.FileName);
            FileMode = true;
        }
    }
}
