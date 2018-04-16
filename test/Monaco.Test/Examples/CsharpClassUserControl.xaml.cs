using Microsoft.CodeAnalysis;
using Monaco.Wpf;
using Monaco.Wpf.CSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace monacotest.Examples
{
    /// <summary>
    /// Interaction logic for CsharpUserControl.xaml
    /// </summary>
    public partial class CsharpClassUserControl : UserControl
    {
        public CsharpClassUserControl()
        {
            InitializeComponent();

            editor.OnEditorInitialized += async (o, e) =>
            {
                var ctx = new CSharpClassContext(
              name: "C",
              staticClass: true,
                helpers: @"
public const int MagicNbr = 42;
                ",
                usings: new List<string> { "System", "System.Linq", "System.Collections.Generic" },
                types: new List<string> { "System.Linq.*", "bool", "System.Collections.Generic.List<*>", "string", "DynamicScript", "Helpers" },
                references: new List<MetadataReference>
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

                await Task.Delay(5_000);
                editor.AddCSharpLanguageService(ctx);
                var langs = editor.GetEditorLanguages();
                editor.SetLanguage("csharp");
            };
            var vm = new ViewModel { Value = @"
public bool Foo()
{
    return MagicNbr == 42;
}
" };
            DataContext = vm;
        }
    }

 
}
