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
using Monaco.Wpf.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.ComponentModel;
using monacotest.Examples;

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

            Content.Children.Add(new CsharpUserControl());
        }

        private void CSharp_Click(object sender, RoutedEventArgs e)
        {
            Content.Children.Clear();
            Content.Children.Add(new CsharpUserControl());
        }

        private void Json_Click(object sender, RoutedEventArgs e)
        {
            Content.Children.Clear();
            Content.Children.Add(new JsonUserControl());
        }

        private void Typescript_Click(object sender, RoutedEventArgs e)
        {
            Content.Children.Clear();
            Content.Children.Add(new TypescriptUserControl());
        }
    }

  
}
