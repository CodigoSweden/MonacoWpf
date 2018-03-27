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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace monacotest.Examples
{
    /// <summary>
    /// Interaction logic for JsonUserControl.xaml
    /// </summary>
    public partial class JsonUserControl : UserControl
    {
        public JsonUserControl()
        {
            InitializeComponent();

            editor.OnEditorInitialized += (o, e) =>
            {
              
                var langs = editor.GetEditorLanguages();
                editor.SetLanguage("json");
            };
            var vm = new ViewModel { Value = @"{
  ""compilerOptions"": {

    ""noImplicitAny"": false,
    ""noEmitOnError"": true,
    ""removeComments"": false,
    
    ""module"": ""none"",
    ""moduleResolution"": ""classic"",

    ""target"": ""es5"",
    ""lib"": [ ""es6"", ""dom""],
    ""sourceMap"": false,
    
    ""outFile"": ""Site/Out/editor.js""
  },
  ""include"": [ ""Site/Src/**/*.ts"" ],
  ""exclude"": [
    ""node_modules"",
    ""wwwroot""
  ]
}
" };
            DataContext = vm;
        }
    }
}
