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

            
            editor.OnEditorInitialized += (o, e) =>
            {
                var ctx = new CSharpContext(
                new List<Argument>
                {
                   new Argument { Name = "seq", Type="List<string>", Description="" }
                },
                new Argument { Name = "", Type = "bool", Description = "" },
                "",
                new List<string> { "System", "System.Linq", "System.Collections.Generic" },
                new List<string> { "System.Linq.*","bool", "System.Collections.Generic.List<*>", "string", "DynamicScript"},
                new List<MetadataReference>
                {
                     MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.ExpressionType).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Dictionary<,>).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ValueTuple<,>).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),

                }
                );

                editor.AddCSharpLanguageService(ctx);
                var langs = editor.GetEditorLanguages();
                editor.SetLanguage("csharp");
            };
            var vm = new ViewModel { Value = @"return true;" };
            DataContext = vm;
            
        }
        
    }

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            var pc = PropertyChanged;
            if(pc != null)
            {
                pc.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Value
        private string mValue;
        /// <summary>
        /// 
        /// </summary>
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                OnPropertyChanged("Value");
            }
        }
        #endregion Value

    }
}
