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

namespace HamburgerMenu.Views
{
    /// <summary>
    /// Interaction logic for PrivateView.xaml
    /// </summary>
    public partial class AutomaticView : UserControl
    {
        public AutomaticView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _iMachineState.Image = System.Drawing.Image.FromFile("Resources/MachineAuto.gif");
            _iMachineState.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        }
    }
}
