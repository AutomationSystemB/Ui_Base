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
using System.Text.RegularExpressions;
using HamburgerMenuApp;

namespace HamburgerMenu.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private _cMachineState UpdateMachineState = new _cMachineState();
        private System.Windows.Forms.Timer UpdatStates;

        float timeout_Info = 3000;
        public HomeView()
        {
            InitializeComponent();
        }

        public void Init_UpdatStates()
        {
            UpdatStates = new System.Windows.Forms.Timer();
            UpdatStates.Tick += new EventHandler(UpdatStates_Tick);
            UpdatStates.Interval = 100; // in miliseconds
            UpdatStates.Start();
        }

        private void UpdatStates_Tick(object sender, EventArgs e)
        {
            if (_cbCylName.SelectedIndex >= 0)
            {
                Check_CylHomeStatus();
                Check_CylWorkStatus();
                if (_cGlobalVariables.Cylinders[_cbCylName.SelectedIndex].IsRunning())
                {
                    _tbCylStatus.Text = "Running";
                }
                else
                    _tbCylStatus.Text = "Waiting Comand";
                if(_cGlobalVariables.Cylinders[_cbCylName.SelectedIndex].AsError())
                {
                    _tbCylStatus.Text = "In Error";
                } 
            }
            if (_cbAxisName.SelectedIndex != -1)
            {
                _tbAxisStatus.Text      = _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].State.ToString();
                _tbCurrentPos.Text      = _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].GetActualPos().ToString();
                _tbCurrentVelocity.Text = _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].GetActualVel().ToString();
                _tbErrorValue.Text      = _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].GetActError().ToString();
            }
            if(timeout_Info < StepTime())
            {
                _tbInfo.Text = "";
            }
        }
       

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _lvDInputs.ItemsSource = _cGlobalVariables.DigitalInputs;
            _lvDOutputs.ItemsSource = _cGlobalVariables.DigitalOutputs;
            int gridRow = 0;
            ManualMovRow.Children.Clear();
            //Grid.SetRow(ManualMovRow, gridRow);
            // if cylinders exists show cylinders manual tab
            if (_cGlobalVariables.Cylinders.Count > 0)
            {
                _gbCylinders.Visibility = Visibility.Visible;
                _cbCylName.ItemsSource = _cGlobalVariables.Ds_CylindersWork.Tables[0].DefaultView;
                _cbCylName.DisplayMemberPath = "Name";
                _cbCylName.SelectedValuePath = "Name";
                _cbCylName.SelectedIndex = -1;
                Grid.SetRow(_gbCylinders, gridRow);
                ManualMovRow.Children.Add(_gbCylinders);
                gridRow=gridRow + 2;
            }
            else
                _gbCylinders.Visibility = Visibility.Hidden;

            // if Axis exists show Axis manual tab
            if (_cGlobalVariables.Axis.Count > 0)
            {
                _gbAxis.Visibility = Visibility.Visible;
                _cbAxisName.ItemsSource = _cGlobalVariables.Ds_Axis.Tables[0].DefaultView;
                _cbAxisName.DisplayMemberPath = "Name";
                _cbAxisName.SelectedValuePath = "Name";
                _cbAxisName.SelectedIndex = -1;
                Grid.SetRow(_gbAxis, gridRow);
                ManualMovRow.Children.Add(_gbAxis);
               gridRow = gridRow + 2;
            }
            else
                _gbAxis.Visibility = Visibility.Hidden;
            
            Init_UpdatStates();
            UpdateMachineState.SetMachineInManual(true);
        }

        private void _bDoutput_Click(object sender, RoutedEventArgs e)
        {
            _cDigitalIO item =(_cDigitalIO) (sender as FrameworkElement).DataContext;
            if (_cGlobalVariables.DigitalOutputs[item.index].State)
            {
                _cGlobalVariables.IoComunication.Write2ComunicationSingle(_cGlobalVariables.DigitalOutputs[item.index].index
                                                                         ,false);
            }
            else
            {
                _cGlobalVariables.IoComunication.Write2ComunicationSingle(_cGlobalVariables.DigitalOutputs[item.index].index
                                                                          ,true);                                             
            }
        }

        private void Check_CylHomeStatus()
        {
            if(_cbCylName.SelectedIndex >= 0)
            {
                if (_cGlobalVariables.DigitalInputs[Convert.ToInt16(_cGlobalVariables.Ds_CylindersWork.Tables[0].Rows[_cbCylName.SelectedIndex]["InHomeIdx"].ToString())].State)
                    _iCylHomeSt.Source =new BitmapImage(new System.Uri(@"pack://application:,,,/HamburgerMenu;component/Resources/green_led.png"));
                else
                    _iCylHomeSt.Source = new BitmapImage(new Uri(@"pack://application:,,,/HamburgerMenu;component/Resources/red_led.png"));
            }
            else
                _iCylWorkSt.Source = new BitmapImage(new Uri(@"pack://application:,,,/HamburgerMenu;component/Resources/red_led.png"));
        }
        private void Check_CylWorkStatus()
        {
            if (_cbCylName.SelectedIndex >= 0)
            {
                if (_cGlobalVariables.DigitalInputs[Convert.ToInt16(_cGlobalVariables.Ds_CylindersWork.Tables[0].Rows[_cbCylName.SelectedIndex]["InWorkIdx"].ToString())].State)
                    _iCylWorkSt.Source = new BitmapImage(new Uri(@"pack://application:,,,/HamburgerMenu;component/Resources/green_led.png"));
                else
                    _iCylWorkSt.Source = new BitmapImage(new Uri(@"pack://application:,,,/HamburgerMenu;component/Resources/red_led.png"));
            }
            else
                _iCylWorkSt.Source = new BitmapImage(new Uri(@"pack://application:,,,/HamburgerMenu;component/Resources/red_led.png"));
        }

        private void _lvDInputs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_lvDInputs.SelectedIndex != -1) _lvDInputs.SelectedIndex = -1;
        }

        private void _lvDOutputs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_lvDOutputs.SelectedIndex != -1) _lvDOutputs.SelectedIndex = -1;
        }

        private void _bMoveHome_Click(object sender, RoutedEventArgs e)
        {
            if (_cbCylName.SelectedIndex >= 0)
            {
                _cGlobalVariables.Cylinders[_cbCylName.SelectedIndex].Move2Home();
            }
        }

        private void _bMoveWork_Click(object sender, RoutedEventArgs e)
        {
            if (_cbCylName.SelectedIndex >= 0)
            {
                _cGlobalVariables.Cylinders[_cbCylName.SelectedIndex].Move2Work();
            }
        }

        private void _cbCylName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_cbCylName.SelectedIndex >=0)
            {

                _tbCylStatus.Text = "Wait Instruction";
            }
            else
            {
                _tbCylStatus.Text = "Idle";
            }
        }

        private void _bCylFill_Click(object sender, RoutedEventArgs e)
        {
            if (_cbCylName.SelectedIndex >= 0)
                _cGlobalVariables.Cylinders[_cbCylName.SelectedIndex].FillChambers();
        }

        private void _bCylEmpty_Click(object sender, RoutedEventArgs e)
        {
            if (_cbCylName.SelectedIndex >= 0)
                _cGlobalVariables.Cylinders[_cbCylName.SelectedIndex].EmptyChambers();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            UpdateMachineState.SetMachineInManual(false);
        }

        private void _bAxisMove2Pos_Click(object sender, RoutedEventArgs e)
        {
            if (_cbAxisName.SelectedIndex != -1)
            {
                
                if ((   _tbMove2Pos.Text   == "" || _tbMove2Pos.Text   == null)    &&
                   (    _tbMoveVel.Text     == "" || _tbMoveVel.Text    == null)   &&
                   (    _tbMoveAcl.Text     == "" || _tbMoveAcl.Text    == null)   ||
                   (    !Regex.IsMatch(_tbMove2Pos.Text, @"\d")                    || 
                        !Regex.IsMatch(_tbMoveVel.Text, @"\d")                     || 
                        !Regex.IsMatch(_tbMoveAcl.Text, @"\d"))                    )
                {
                    _tbInfo.Text = "Need To fill the needed information correctly. Position, Velocity and Aceleration must be filled.";
                    StartTimer();
                }
                else
                    _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].Move2Position(float.Parse(_tbMove2Pos.Text), float.Parse(_tbMoveVel.Text), float.Parse(_tbMoveAcl.Text));
            }
        }

        private void _bAxisSave2List_Click(object sender, RoutedEventArgs e)
        {
            if (_cbAxisName.SelectedIndex != -1)
            {

            }
        }

        private void _bAxisJogPlus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_cbAxisName.SelectedIndex != -1)
            {
                _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].JogPlus();
            }
        }

        private void _bAxisJogPlus_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_cbAxisName.SelectedIndex != -1)
            {
                _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].JogStop();
            }
        }

        private void _bAxisJogMinus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_cbAxisName.SelectedIndex != -1)
            {
                _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].JogMinus();
            }
        }

        private void _bAxisJogMinus_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_cbAxisName.SelectedIndex != -1)
            {
                _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].JogStop();
            }
        }

        private void _bAxisHoming_Click(object sender, RoutedEventArgs e)
        {
            if (_cbAxisName.SelectedIndex != -1)
            {
                _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].Homing();
            }
        }

        private void _bAxisReset_Click(object sender, RoutedEventArgs e)
        {
            if (_cbAxisName.SelectedIndex != -1)
            {
                _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].ResetAxis();
            }
        }

        private void _ckbEnable_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void _ckbEnable_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        #region Timer counter
        private static DateTime time_ini;

        public static void StartTimer()
        {
            time_ini = DateTime.Now;
        }
        public static double StepTime()
        {
            DateTime time_fin = DateTime.Now;
            TimeSpan time = time_fin - time_ini;
            double timer = Convert.ToDouble(time.TotalMilliseconds);
            return timer;
        }

        #endregion



        
    }
}
