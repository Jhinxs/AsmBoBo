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

namespace AsmBoBo
{
    /// <summary>
    /// StringConverter.xaml 的交互逻辑
    /// </summary>
    public partial class StringConverter : Window
    {
        public static int Stack_ALIGN;
        public static Boolean stringmode1;
        public static Boolean stringmode2;
        public StringConverter()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            Arch_combobox.SelectedIndex = 0;
            CheckBoxmode1.IsChecked = true;
            Transform_combobox.SelectedIndex = 0;
        }

        private void converter_button_Click(object sender, RoutedEventArgs e)
        {
            string string_editor_text = new TextRange( string_editor.Document.ContentStart,
            string_editor.Document.ContentEnd).Text;
            if (String.IsNullOrEmpty(string_editor_text)) 
            {
                return;          
            }
            stringasm_editor.Text = string.Empty;
            switch (Transform_combobox.SelectedIndex) 
            {

                case 0: 
                    {
                        if (Arch_combobox.Text == "x86")
                        {
                            Stack_ALIGN = 4;
                            stringasm_editor.Text = ConverterString(string_editor_text.Trim(), Stack_ALIGN);
                            return;
                        }
                        if (Arch_combobox.Text == "x64")
                        {
                            Stack_ALIGN = 8;
                            stringasm_editor.Text = ConverterString(string_editor_text.Trim(), Stack_ALIGN);
                            return;
                        }
                    }
                    break;
                case 1:
                    stringasm_editor.TextWrapping = TextWrapping.Wrap;
                    stringasm_editor.Text = StrToCstr(string_editor_text);
                    break;
                case 2:
                    stringasm_editor.TextWrapping = TextWrapping.Wrap;
                    stringasm_editor.Text = StrToBase64(string_editor_text);
                    break;
                default:
                    break;
            }

        }
        
        public static string ConverterString(string temp,int stack_align) 
        {
            List<string> stringasm = StringToHexString(temp, stack_align);
            string swapstring = string.Empty;
            if (Stack_ALIGN == 4)
            {
                if (stringmode1)
                {
                    for (int i = 0; i < stringasm.Count; i++)
                    {
                        swapstring += $"push {stringasm[i]}h\n";
                    }
                    return swapstring;
                }
                if (stringmode2)
                {
                   
                    for (int i = 0; i < stringasm.Count; i++)
                    {
                        if (stringasm[i].Length == 8)
                        {
                            swapstring += $"push {stringasm[i].Substring(4)}h\n";
                            swapstring += $"mov word ptr [esp+2],{stringasm[i].Substring(0, 4)}h\n";
                        }
                        else 
                        {
                            swapstring += $"push {stringasm[i]}h\n";
                        }
                    }
                    return swapstring;
                }
                return swapstring;
            }
            if (Stack_ALIGN == 8)
            {
                if (stringmode1)
                {
                    for (int i = 0; i < stringasm.Count; i++)
                    {
                        swapstring += $"mov rax,{stringasm[i]}h\n";
                        swapstring += "push rax\n";
                    }
                    return swapstring;
                }
                if (stringmode2) 
                {
                    for (int i = 0; i < stringasm.Count; i++)
                    {
                        if (stringasm[i].Length > 8)
                        {
                            int substart = 8 - (16 - stringasm[i].Length);
                            swapstring += $"mov dword ptr [rsp],{stringasm[i].Substring(substart)}h\n";
                            swapstring += $"mov dword ptr [rsp+4],{stringasm[i].Substring(0,stringasm[i].Length-8)}h\n";
                            if (i != stringasm.Count - 1)
                            {
                                swapstring += "sub rsp,8\n";
                            }
                        }
                        else 
                        {
                            swapstring += $"mov rax,{stringasm[i]}h\n";
                            swapstring += "push rax\n";
                        }
                    }
                    return swapstring;

                }
                return swapstring;
            }
            else { return string.Empty; }
        }
        public static List<string> StringToHexString(string temp, int stack_align)
        {

            byte[] b = Encoding.Default.GetBytes(temp.ToCharArray());
            byte[] newArray = new byte[b.Length + 1];
            b.CopyTo(newArray, 0);
            newArray[b.Length] = 0x00;
            List<byte[]> subArrayList = splitAry(newArray, stack_align);
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
        private void mode1_check(object sender, RoutedEventArgs e) 
        {
            stringmode1 = true;
            stringmode2 = false;
            CheckBoxmode2.IsChecked = false;
        }
        private void mode2_check(object sender, RoutedEventArgs e)
        {
            stringmode1 = false;
            stringmode2 = true;
            CheckBoxmode1.IsChecked = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.StringConverterWindowList.RemoveAt(MainWindow.StringConverterWindowList.Count-1);
        }

        private void StrToCstr_Selected(object sender, RoutedEventArgs e)
        {
            Arch_combobox.IsEnabled = false;
            CheckBoxmode1.IsEnabled = false;
            CheckBoxmode2.IsEnabled = false;
        }
        public static string StrToCstr(string sourcestring)
        {
            string cstr = "//Ansi String\nCHAR buff[] = {\n";
            char[] cstrarrary = sourcestring.Trim().ToCharArray();
            foreach (char a in cstrarrary) 
            {
                string temp = string.Empty;
                if (a.Equals('\\'))
                {
                    temp += $"'\\\\',";
                }
                else 
                {
                    temp += $"'{a}',";
                }
                cstr += temp;
            }
            cstr += "0";
            cstr += "};";
            return cstr;
        }
        private void StrToBase64_Selected(object sender, RoutedEventArgs e)
        {
            Arch_combobox.IsEnabled = false;
            CheckBoxmode1.IsEnabled = false;
            CheckBoxmode2.IsEnabled = false;
        }
        public static string StrToBase64(string sourcestring) 
        {
            string base64str = Convert.ToBase64String(Encoding.Default.GetBytes(sourcestring));
            return base64str;
        }
        private void StrToAsm_Selected(object sender, RoutedEventArgs e)
        {
            Arch_combobox.IsEnabled = true;
            CheckBoxmode1.IsEnabled = true;
            CheckBoxmode2.IsEnabled = true;
        }
    }
}
