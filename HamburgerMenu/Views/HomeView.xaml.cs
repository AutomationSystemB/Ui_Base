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
                    _tbCylStatus.Text = TextByTag(110);
                }
                else
                    _tbCylStatus.Text = TextByTag(109);
                if(_cGlobalVariables.Cylinders[_cbCylName.SelectedIndex].AsError())
                {
                    _tbCylStatus.Text = TextByTag(111);
                } 
            }

            if (_cbAxisName.SelectedIndex != -1)
            {
                switch (_cGlobalVariables.Axis[_cbAxisName.SelectedIndex].State)
                {
                    case _cAxis.AxisState.Idle:
                        _tbAxisStatus.Text = TextByTag(136);
                        break;
                    case _cAxis.AxisState.Moving:
                        _tbAxisStatus.Text = TextByTag(137);
                        break;
                    case _cAxis.AxisState.Error:
                        _tbAxisStatus.Text = TextByTag(138);
                        break;
                    case _cAxis.AxisState.Stop:
                        _tbAxisStatus.Text = TextByTag(139);
                        break;
                    case _cAxis.AxisState.InPosition:
                        _tbAxisStatus.Text = TextByTag(140);
                        break;
                    case _cAxis.AxisState.ExecutingHoming:
                        _tbAxisStatus.Text = TextByTag(141);
                        break;
                    case _cAxis.AxisState.LimitReached:
                        _tbAxisStatus.Text = TextByTag(142);
                        break;
                    default:
                        break;
                } 
                _tbCurrentPos.Text      = _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].GetActualPos().ToString();
                _tbCurrentVelocity.Text = _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].GetActualVel().ToString();
                _tbErrorValue.Text      = _cGlobalVariables.Axis[_cbAxisName.SelectedIndex].GetActError().ToString();
            }

            if(_cbScannerName.SelectedIndex != -1)
            {
                switch (_cGlobalVariables.Scanners[_cbScannerName.SelectedIndex].State)
                {
                    case _cScanner.ScannerState.Error:
                        _tbScannerState.Text = TextByTag(154);
                        break;
                    case _cScanner.ScannerState.Connected:
                        _tbScannerState.Text = TextByTag(155);
                        break;
                    case _cScanner.ScannerState.Disconnected:
                        _tbScannerState.Text = TextByTag(156);
                        break;
                    case _cScanner.ScannerState.Reading:
                        _tbScannerState.Text = TextByTag(157);
                        break;
                    case _cScanner.ScannerState.Ready:
                        _tbScannerState.Text = TextByTag(158);
                        break;
                    case _cScanner.ScannerState.Done:
                        _tbScannerState.Text = TextByTag(159);
                        _tbScannerDataRead.Text = _cGlobalVariables.Scanners[_cbScannerName.SelectedIndex].GetData();
                        break;
                    default:
                        break;
                }

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
                _lCylName.Content               = TextByTag(101);
                _lCylInHomeState.Content        = TextByTag(102);
                _lCylInWorkState.Content        = TextByTag(103);
                _bCylMoveHome.Content           = TextByTag(104);
                _bCylMoveWork.Content           = TextByTag(105);
                _bCylEmpty.Content              = TextByTag(106);
                _lCylStatus.Content             = TextByTag(108);
                _bCylFill.Content               = TextByTag(107);
                _tbCylStatus.Text               = TextByTag(112);
                _gbCylinders.Visibility         = Visibility.Visible;
                _cbCylName.ItemsSource          = _cGlobalVariables.Ds_CylindersWork.Tables[0].DefaultView;
                _cbCylName.DisplayMemberPath    = "Name";
                _cbCylName.SelectedValuePath    = "Name";
                _cbCylName.SelectedIndex        = -1;
                Grid.SetRow(_gbCylinders, gridRow);
                ManualMovRow.Children.Add(_gbCylinders);
                gridRow                         = gridRow + 2;
            }
            else
                _gbCylinders.Visibility         = Visibility.Hidden;

            // if Axis exists show Axis manual tab
            if (_cGlobalVariables.Axis.Count > 0)
            {
                _cbAxisName.Text                = TextByTag(120);
                _lAxisName.Content              = TextByTag(121);
                _lAxisStatus.Content            = TextByTag(122);
                _lAxisCurrentPos.Content        = TextByTag(123);
                _lAxisCurrentVelocity.Content   = TextByTag(124);
                _bAxisSave2List.Content         = TextByTag(125);
                _lAxisErrorValue.Content        = TextByTag(126);
                _lAxisEnable.Content            = TextByTag(127);
                _lAxisPosID.Content             = TextByTag(128);
                _lAxisMovePos.Content           = TextByTag(129);
                _lAxisMoveVel.Content           = TextByTag(130);
                _bAxisJogPlus.Content           = TextByTag(131);
                _bAxisJogMinus.Content          = TextByTag(132);
                _bAxisMove2Pos.Content          = TextByTag(133);
                _bAxisHoming.Content            = TextByTag(134);
                _bAxisReset.Content             = TextByTag(135);
                _tbAxisStatus.Text              = TextByTag(136);
                _gbAxis.Visibility              = Visibility.Visible;
                _cbAxisName.ItemsSource         = _cGlobalVariables.Ds_Axis.Tables[0].DefaultView;
                _cbAxisName.DisplayMemberPath   = "Name";
                _cbAxisName.SelectedValuePath   = "Name";
                _cbAxisName.SelectedIndex = -1;
                Grid.SetRow(_gbAxis, gridRow);
                ManualMovRow.Children.Add(_gbAxis);
                gridRow                         = gridRow + 2;
            }
            else
                _gbAxis.Visibility = Visibility.Hidden;

            if (_cGlobalVariables.Scanners.Count > 0)
            {
                _cbScannerName.Text                 = TextByTag(150);
                _lScannerName.Content               = TextByTag(151);
                _lScannerDataRead.Content           = TextByTag(152);
                _bReadScanner.Content               = TextByTag(153);
                _lScannerState.Content              = TextByTag(160);
                _tbScannerState.Text                = TextByTag(136);
                _gbScanner.Visibility               = Visibility.Visible;
                _cbScannerName.ItemsSource          = _cGlobalVariables.Ds_Scanners.Tables[0].DefaultView;
                _cbScannerName.DisplayMemberPath    = "Name";
                _cbScannerName.SelectedValuePath    = "Name";
                _cbScannerName.SelectedIndex        = -1;
            }
            else
                _gbScanner.Visibility               = Visibility.Hidden;

            _tbScannerDataRead.Text = "";
            Init_UpdatStates();
            UpdateMachineState.SetMachineInManual(true);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            UpdateMachineState.SetMachineInManual(false);
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

                _tbCylStatus.Text = TextByTag(109);
            }
            else
            {
                _tbCylStatus.Text = TextByTag(112);
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
                    _tbInfo.Text = TextByTag(170);
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

        private void _cbAxisName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_cbAxisName.SelectedIndex >= 0)
            {

                _tbAxisStatus.Text = TextByTag(139);
            }
            else
            {
                _tbAxisStatus.Text = TextByTag(136);
            }
        }

        private void _ckbEnable_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void _ckbEnable_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void _cbScannerName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            _tbScannerDataRead.Text = "";
        }

        private void _bReadScanner_Click(object sender, RoutedEventArgs e)
        {
            if(_cbScannerName.SelectedIndex>=0)
            {
                _cGlobalVariables.Scanners[_cbScannerName.SelectedIndex].SendTrigger();
            }
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

        private string TextByTag(int intTag)
        {
            try
            {
                return _cGlobalVariables.Ds_Lang.Tables[0].Select("Tag = '" + intTag + "'")[0][_cGlobalVariables.SelectLang].ToString().Replace("|", "\r\n");
            }
            catch (Exception)
            {
                return intTag.ToString();
            }
        }
   
    }
}
