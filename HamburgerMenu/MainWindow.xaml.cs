using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using HamburgerMenu.Views;

namespace HamburgerMenuApp
{ 
    public partial class MainWindow : MetroWindow
    {
        private _cWorkXMLFiles  XmlFiles;
        private _cMachineState  MachineState;
        private _cUpdateIO      IOTread;


        #region Funtions for inicialization of Machine on Load
        private void CreateDataSets()
        {
            _cGlobalVariables.Ds_Init           = new DataSet();
            _cGlobalVariables.Ds_Lang           = new DataSet();
            _cGlobalVariables.Ds_Counters       = new DataSet();
            _cGlobalVariables.Ds_IO             = new DataSet();
            _cGlobalVariables.Ds_Params         = new DataSet();
            _cGlobalVariables.Ds_Refs           = new DataSet();
            _cGlobalVariables.Ds_Users          = new DataSet();
            _cGlobalVariables.Ds_CylindersLoad  = new DataSet();
            _cGlobalVariables.Ds_CylindersWork  = new DataSet();
            _cGlobalVariables.Ds_Axis           = new DataSet();
        }
        private void CreateVariables()
        {
            MachineState    = new _cMachineState();
            XmlFiles        = new _cWorkXMLFiles();
            IOTread         = new _cUpdateIO();
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            //  set the window instance to the global variable to manipulate elements
            _cGlobalVariables._wMainWindowInterface = _wUserInterface;

            CreateDataSets();
            CreateVariables();
            //Load all necessarie data from files
            XmlFiles.InitVars();
            _wUserInterface.Title = MachineState.GetStationName();
            _cGlobalVariables.UserInfo.LoginDefaultUser((int)_cGlobalVariables.Permission.Manutencao);
            SetPermissions(_cGlobalVariables.UserInfo.GetUserLevel());
            SetMenuButtons();
            IOTread.StartThread();


        }
        public void SetMenuButtons()
        {

            _bAboutWindow.IsVisible     = _cGlobalVariables.PagAbout;
            _bAutomaticWindow.IsVisible = _cGlobalVariables.PagAuto;
            _bLoginWindow.IsVisible     = _cGlobalVariables.PagLogin;
            _bSettingsWindow.IsVisible  = _cGlobalVariables.PagConfig;
            _bHomeWindow.IsVisible      = _cGlobalVariables.PagManual;
        }
        public void SetPermissions(int value)
        {
            switch (value)
            {
                case 1:
                    _bAboutWindow.IsEnabled     = true;
                    _bAutomaticWindow.IsEnabled = true;
                    _bLoginWindow.IsEnabled     = true;
                    _bSettingsWindow.IsEnabled  = false;
                    _bHomeWindow.IsEnabled      = false;
                    break;
                case 2:
                    _bAboutWindow.IsEnabled = true;
                    _bAutomaticWindow.IsEnabled = true;
                    _bLoginWindow.IsEnabled = true;
                    _bSettingsWindow.IsEnabled = true;
                    _bHomeWindow.IsEnabled = true;
                    break;
               
            }
            
        }
        private void HamburgerMenuControl_OnItemClick(object sender, ItemClickEventArgs e)
        {
            this.HamburgerMenuControl.Content = e.ClickedItem;
            this.HamburgerMenuControl.IsPaneOpen = false;
        }

        private void _wUserInterface_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IOTread.StopThread();
            while (IOTread.GetState()) ;

        }
    }
}
