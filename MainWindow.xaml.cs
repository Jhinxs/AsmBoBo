using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Keystone;
using System.IO;
using Microsoft.Win32;

namespace AsmBoBo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public static Boolean NoErrorWhenAssembly;
        public static Boolean Usex64;
        public static int SwitchToMode1 =0;
        public static int SwitchToMode2 =0;
        public static string AsmCache = string.Empty;
        public static string CppCodeCache = string.Empty;
        public static List<StringConverter> StringConverterWindowList = new List<StringConverter>();
        public static List<shellcode_enc> shellcode_enc_WindowList = new List<shellcode_enc>();
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            NoErrorWhenAssembly = true;
            Usex64 = false;
            InitializeComponent();
            Mode_combobox.SelectedIndex = 0;
            AsmCache = string.Empty;
            CppCodeCache = string.Empty;
    }
        private void converter_button_Click(object sender, RoutedEventArgs e)
        {
            string source_code_editor_text = new TextRange(source_code_editor.Document.ContentStart,
            source_code_editor.Document.ContentEnd).Text;
            if (source_code_editor_text == string.Empty)
            {
                op_code_editor.Text = string.Empty;
                return;
            }
            if (Mode_combobox.SelectedIndex == 0)
            {
                string formattingcode = Format_text(source_code_editor_text.Split('\n'));
                if (Arch_combobox.Text == "x86")
                {
                    
                    string asmcode = x86.x86main.ParseSimulationCode(formattingcode);
                    op_code_editor.Text = asmcode;
                }
                if (Arch_combobox.Text == "x64")
                {
                }
                NoErrorWhenAssembly = true;
            } 
            else if(Mode_combobox.SelectedIndex == 1)
            {
                string opcode = string.Empty;
                string[] sourcecode = source_code_editor_text.Split('\n');
                if (Arch_combobox.Text == "x86")
                {
                    Usex64 = false;
                    opcode = Assemble(sourcecode, Usex64);
                }
                if (Arch_combobox.Text == "x64")
                {
                    Usex64 = true;
                    opcode = Assemble(sourcecode, Usex64);
                }
                if (Format_combobox.Text == "bin")
                {
                    op_code_editor.Text = string.Empty;
                    op_code_editor.Text += opcode;
                }
                if (Format_combobox.Text == "cpp")
                {
                    op_code_editor.Text = string.Empty;
                    op_code_editor.Text += OpcodeToCStyle(opcode);
                }

                NoErrorWhenAssembly = true; 
            }
        }

        public static string Assemble(string[] sourcecode, Boolean x64)
        {
            string opcode = string.Empty;
            string codeline = string.Empty;

            foreach (string code in sourcecode)
            {
                try
                {
                    if (code.Substring(0, 2) != "//")                                                  //处理注释
                    {
                        codeline += code.Replace('\t', ' ').Replace('\r', ' ').Trim(' ');
                        codeline += "\r\n";
                    }
                }
                catch { }

            }
            opcode = AssemblyToShellcode(codeline, x64);
            return opcode;
        }
        public static string AssemblyToShellcode(string Assembly, Boolean x64)
        {
            string shellcodestring = string.Empty;
            byte[] enc = new byte[] { };
            if (!x64)
            {
                using (Engine keystone = new Engine(Architecture.X86, Mode.X32) { ThrowOnError = true })
                {
                    ulong address = 0;
                    int size = 0;
                    int count = 0;
                    try
                    {
                        enc = keystone.Assemble(Assembly, address, out size, out count);
                    }
                    catch (Exception ex)
                    {
                        shellcodestring = ex.ToString();
                        NoErrorWhenAssembly = false;
                        return shellcodestring;
                    }
                }
            }
            else
            {
                using (Engine keystone = new Engine(Architecture.X86, Mode.X64) { ThrowOnError = true })
                {
                    ulong address = 0;
                    int size = 0;
                    int count = 0;
                    try
                    {
                        enc = keystone.Assemble(Assembly, address, out size, out count);
                    }
                    catch (Exception ex)
                    {
                        shellcodestring = ex.ToString();
                        NoErrorWhenAssembly = false;
                        return shellcodestring;
                    }
                }
            }
            foreach (var bytecode in enc)
            {
                shellcodestring += bytecode.ToString("x");
                shellcodestring += " ";
            }
            return shellcodestring.TrimEnd(' ');
        }
        public static string OpcodeToCStyle(string opcode)
        {
            string cheader = "unsigned char buff[] = {\n";
            string[] opcodearry = opcode.Split(' ');
            for (int i = 0; i < opcodearry.Length; i++)
            {
                if (i != 0 && i % 15 == 0)
                {
                    cheader += $"0x{opcodearry[i]},\n";
                }
                else
                {
                    cheader += $"0x{opcodearry[i]},";
                }
            }
            cheader = cheader.TrimEnd('\n').TrimEnd(',');
            cheader += "};\n\n";
            cheader += $"//size: {opcodearry.Length.ToString()} Bytes";
            return cheader;
        }

        private void op_code_editor_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Format_combobox.SelectedIndex == 0) 
            {
                if (op_code_editor.ContextMenu.Items.Count == 6) 
                {
                    return;
                }
                MenuItem Save = new MenuItem();
                Save.Header = "保存";
                Save.Click += new RoutedEventHandler(savebin);               
                op_code_editor.ContextMenu.Items.Add(Save);
            }
            
        }
        private void savebin(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] opcode_string = op_code_editor.Text.Split(' ');
                byte[] opcode_byte = new byte[opcode_string.Length];
                for (int i = 0; i < opcode_string.Length; i++)
                {
                    opcode_byte[i] = Convert.ToByte(opcode_string[i], 16);
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "bin file(*.bin)| *.bin";
                saveFileDialog.ShowDialog();
                File.WriteAllBytes(saveFileDialog.FileName, opcode_byte);
            }
            catch (Exception ex) 
            {
                op_code_editor.Text = $"\n{ex.ToString()}";  
            }
        }

        private void Mode_Select_To_Asm(object sender, RoutedEventArgs e)
        {
            op_code_editor.TextWrapping = TextWrapping.NoWrap;
            string text = new TextRange(source_code_editor.Document.ContentStart, source_code_editor.Document.ContentEnd).Text;
            CppCodeCache = text;
            if (SwitchToMode1 == 0)
            {
                if (AsmCache != string.Empty)
                {
                    source_code_editor.Document.Blocks.Clear();
                    source_code_editor.AppendText(AsmCache);
                }
                else
                {
                    string templatecode = template.cppcode;
                    source_code_editor.Document.Blocks.Clear();
                    source_code_editor.AppendText(templatecode);
                    SwitchToMode1++;
                }
            }
            else
            {
                source_code_editor.Document.Blocks.Clear();
                source_code_editor.AppendText(AsmCache);
            }

            Format_combobox.IsEnabled = false;
        }

        private void Mode_Select_To_Opcode(object sender, RoutedEventArgs e)
        {
            op_code_editor.TextWrapping = TextWrapping.Wrap;
            string text = new TextRange(source_code_editor.Document.ContentStart, source_code_editor.Document.ContentEnd).Text;
            AsmCache = text;
            if (SwitchToMode2 == 0)
            {
                if (CppCodeCache!=string.Empty)
                {
                    source_code_editor.Document.Blocks.Clear();
                    source_code_editor.AppendText(CppCodeCache);
                }
                else
                {
                    string templatecode = template.AsmCode;
                    source_code_editor.Document.Blocks.Clear();
                    source_code_editor.AppendText(templatecode);
                    SwitchToMode2++;
                }
            }
            else
            {
                source_code_editor.Document.Blocks.Clear();
                source_code_editor.AppendText(CppCodeCache);
            }

            Format_combobox.IsEnabled = true;
        }

        private void source_code_editor_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (source_code_editor.ContextMenu.Items.Count == 6)
            {
                return;
            }
            MenuItem format = new MenuItem();
            format.Header = "格式化";
            format.Click += new RoutedEventHandler(formatting);
            source_code_editor.ContextMenu.Items.Add(format);

        }
        private void formatting(object sender, RoutedEventArgs e)
        {
            string source_code_editor_text = new TextRange(source_code_editor.Document.ContentStart, source_code_editor.Document.ContentEnd).Text;
            string[] sourcecode = source_code_editor_text.Split('\n');
            string formattingcode = Format_text(sourcecode);
            source_code_editor.Document.Blocks.Clear();
            source_code_editor.AppendText(formattingcode);
        }
        public static string Format_text(string[] text) 
        {
            string formattingcode = string.Empty;
            foreach (string code in text)
            {
                try
                {

                    formattingcode += code.Replace('\t', ' ').Replace('\r', ' ').Trim(' ');
                    formattingcode += "\n";
                }
                catch { }

            }
            return formattingcode;

        }
        private void Converter_String_click(object sender, RoutedEventArgs e)
        {
            StringConverter StringConverterWindow = new StringConverter();
            if (StringConverterWindowList.Count != 0)
            {
                foreach (var a in StringConverterWindowList)
                {
                    a.Activate();
                }
            }
            else
            {
                StringConverterWindow.Show();
                StringConverterWindowList.Add(StringConverterWindow);
            }



        }
        private void Shellcode_Enc_Click(object sender, RoutedEventArgs e)
        {
            shellcode_enc shellcode_window = new shellcode_enc();
            if (shellcode_enc_WindowList.Count != 0)
            {
                foreach (var a in shellcode_enc_WindowList)
                {
                    a.Activate();
                }
            }
            else
            {
                shellcode_window.Show();
                shellcode_enc_WindowList.Add(shellcode_window);
            }
        }
    }
}
