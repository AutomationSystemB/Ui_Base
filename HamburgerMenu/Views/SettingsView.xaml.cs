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
using HamburgerMenuApp;

namespace HamburgerMenu.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
   
        public SettingsView()
        {
            InitializeComponent();
        }

        private void _bEnLang_Click(object sender, RoutedEventArgs e)
        {
            _cGlobalVariables.SelectLang = "EN";
        }

        private void _bPtLang_Click(object sender, RoutedEventArgs e)
        {
            _cGlobalVariables.SelectLang = "PT";
        }
    }
}
