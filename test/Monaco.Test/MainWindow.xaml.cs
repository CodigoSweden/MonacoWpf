using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO.Compression;
using Monaco.Wpf;

namespace monacotest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

//            editor.EditorLanguage = EditorLanguage.CSharp;
//            editor.Value = @"
//public class Program
//{
//    public static void Main()
//    {
//        System.Console.WriteLine(""Hello World!"");
//    }
//}
//";

        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            //editor.EditorLanguage = EditorLanguage.Html;
        }
    }

   

    

}
