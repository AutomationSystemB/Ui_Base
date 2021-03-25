using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using HamburgerMenuApp;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace HamburgerMenu.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        private static DataSet _dsComboBox;
        private _cGlobalVariables.Permission LoggedUserLevel;
        private static DataRow SelectedUser;
        private static bool FlagChoose                                  = false;
        private System.Windows.Threading.DispatcherTimer ClearUserInfo  = new System.Windows.Threading.DispatcherTimer();
        private static DataSet _dsUser                                  = new DataSet();
        _cMachineState MachineState                                     = new _cMachineState();
        _cWorkXMLFiles XmlFiles                                         = new _cWorkXMLFiles();
        private static string   InsertedPSW                             = "";
        private static string   LoggedUser                              = "";
        


        public string GetUser()
        {
            return LoggedUser;
        }
        public int GetUserLevel()
        {
            return (int)LoggedUserLevel;
        }

        public LoginView()
        {
            InitializeComponent();
            ClearUserInfo.Tick     += new EventHandler(ClearMessageInfo);
            ClearUserInfo.Interval  = new TimeSpan(0,0,0,0,5000);
            
        }

        public void LoginDefaultUser(int value)
        {
            SelectedUser= _cGlobalVariables.Ds_Users.Tables[0].Rows[value-1];
            LoggedUser = TextByTag(Convert.ToInt16(SelectedUser["UsernameTag"].ToString()));
            LoggedUserLevel = (_cGlobalVariables.Permission)Convert.ToInt32(SelectedUser["AccessMask"].ToString());
        }

        private void ClearMessageInfo(object sender, EventArgs e)
        {
            _tbUserMessage.Text = "";
            ClearUserInfo.Stop();
        }

        private void _bNumeric_Click(object sender, RoutedEventArgs e)
        {
            Button NumberPressed     = (Button) sender;
            _tbPassword.Text        += "*";
            InsertedPSW             += NumberPressed.Content;
        }

        private void _bClose_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void _bClear_Click(object sender, RoutedEventArgs e)
        {
            InsertedPSW         = "";
            _tbPassword.Text    = "";
        }

        private void _bLogin_Click(object sender, RoutedEventArgs e)
        {
          
            if (_cbUsers.SelectedIndex<=0)
            {
                _tbUserMessage.Text = TextByTag(16);
                _bClear_Click(sender , e);
                ClearUserInfo.Start();
            }
            else if (InsertedPSW.Length != 4)
            {
                _tbUserMessage.Text = TextByTag(15);
                _tbPassword.Text    = "";
                InsertedPSW         = "";
                ClearUserInfo.Start();
            }else
            {
                if (CheckUser())
                {
                    LoggedUser          = TextByTag(Convert.ToInt16(SelectedUser["UsernameTag"].ToString()));
                    LoggedUserLevel     = (_cGlobalVariables.Permission) Convert.ToInt32(SelectedUser["AccessMask"].ToString());
                    _tbName.Text        = LoggedUser;
                    _tbPermission.Text  = TextByTag(Convert.ToInt16(SelectedUser["IdentificationTag"].ToString()));
                    _tbUserMessage.Text = TextByTag(18);
                    _bClear_Click(sender, e);
                    ClearUserInfo.Start();
                    _cGlobalVariables._wMainWindowInterface.SetPermissions((int)LoggedUserLevel);
                }
                else
                {
                    _tbUserMessage.Text = TextByTag(15);
                    _bClear_Click(sender, e);
                    ClearUserInfo.Start();
                }

            }
        }

        private bool CheckUser()
        {
            
            if (InsertedPSW.Equals(SelectedUser["Psw"].ToString())) { return true; }
            else { return false; }
        }

        private void UpdateFormLanguage()
        {
            _lwindowId.Content                          = TextByTag(8);
            _lUser.Content                              = TextByTag(1);
            _lName.Content                              = TextByTag(4);
            _bLogin.Content                             = TextByTag(3);
            _bClear.Content                             = TextByTag(6);
            _bClose.Content                             = TextByTag(7);
            _lPermission.Content                        = TextByTag(5);
            _lPassword.Content                          = TextByTag(2);
            LoggedUser = TextByTag(Convert.ToInt16(SelectedUser["UsernameTag"].ToString()));
            _tbName.Text = LoggedUser;
            _tbPermission.Text = TextByTag(Convert.ToInt16(SelectedUser["IdentificationTag"].ToString()));

        }

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



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _tbName.Text        = LoggedUser;
            _tbPermission.Text  = LoggedUserLevel.ToString();
            _cbUsers.Focus();
            _dsUser             = _cGlobalVariables.Ds_Users;

            try
            {
                if (!FlagChoose)
                {
                    DataRow Dr2         = _dsUser.Tables[0].NewRow();
                    Dr2["ID_User"]      = "0";
                    Dr2["UsernameTag"]  = "14";
                    _dsUser.Tables[0].Rows.InsertAt(Dr2, 0);
                    FlagChoose          = true;
                }
                _dsComboBox = _dsUser.Copy();

                    int i = 0;
                    foreach (DataRow Dr in _dsUser.Tables[0].Rows)
                    {

                        _dsComboBox.Tables[0].Rows[i]["ID_User"]    = _dsUser.Tables[0].Rows[i]["ID_User"].ToString();
                        _dsComboBox.Tables[0].Rows[i]["Username"]   = TextByTag(Convert.ToInt16(_dsUser.Tables[0].Rows[i]["UsernameTag"].ToString()));
                        i++;
                    }
                    _cbUsers.ItemsSource        = _dsComboBox.Tables[0].DefaultView;
                    _cbUsers.DisplayMemberPath  = "Username";
                    _cbUsers.SelectedValuePath  = "ID_User";
                    _cbUsers.SelectedIndex      = 0;

                UpdateFormLanguage();

            }
            catch (Exception exp)
            {
                MessageBox.Show(TextByTag(1036) + "\r\n" + exp.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
                Window.GetWindow(this).Close();
            }
        }

        private void _cbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_cbUsers.SelectedIndex>0)
                {
                    SelectedUser = _dsUser.Tables[0].Rows[_cbUsers.SelectedIndex];
                    _tbPassword.Focus();
                }
            }
            catch
            {
                MessageBox.Show(TextByTag(17), TextByTag(8), MessageBoxButton.OK, MessageBoxImage.Error);
                Window.GetWindow(this).Close();
            }
        }
    }
}
