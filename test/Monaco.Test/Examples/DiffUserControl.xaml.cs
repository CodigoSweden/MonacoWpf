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
    public partial class DiffUserControl : UserControl
    {
        public DiffUserControl()
        {
            InitializeComponent();

            leftTxt.Text = "This line is removed on the right.\njust some text\nabcd\nefgh\nSome more text";
            rightTxt.Text = "just some text\nabcz\nzzzzefgh\nSome more text.\nThis line is removed on the left.";
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            editor.SetContent(leftTxt.Text, rightTxt.Text);
        }
    }
}
