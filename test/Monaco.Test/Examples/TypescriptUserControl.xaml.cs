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
    /// Interaction logic for TypescriptUserControl.xaml
    /// </summary>
    public partial class TypescriptUserControl : UserControl
    {
        public TypescriptUserControl()
        {
            InitializeComponent();
            editor.OnEditorInitialized += (o, e) =>
            {

                var langs = editor.GetEditorLanguages();
                editor.SetLanguage("typescript");
            };
            var vm = new ViewModel { Value = @"
 class Animal {
    constructor(public name: string) { }
    move(distanceInMeters: number = 0) {
        console.log(`${this.name} moved ${distanceInMeters}m.`);
    }
}

class Snake extends Animal {
    constructor(name: string) { super(name); }
    move(distanceInMeters = 5) {
        console.log(""Slithering..."");
        super.move(distanceInMeters);
        }
    }

    class Horse extends Animal
    {
        constructor(name: string) { super(name); }
        move(distanceInMeters = 45) {
            console.log(""Galloping..."");
            super.move(distanceInMeters);
        }
    }

    let sam = new Snake(""Sammy the Python"");
    let tom: Animal = new Horse(""Tommy the Palomino"");

    sam.move();
tom.move(34); 
" };
            DataContext = vm;
        }
    }
}
