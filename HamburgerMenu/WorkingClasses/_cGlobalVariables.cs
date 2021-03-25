using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Data;
using HamburgerMenu.Views;

namespace HamburgerMenuApp
{
    class _cGlobalVariables
    {
        //define the structure for the input and output signals
        public static MainWindow _wMainWindowInterface;

        //define Enum for User permissions
        public enum Permission { Operator = 1, Manutencao =  2 }
        public enum Comunications {Modbus = 1 }
        //menu options
        public static bool  PagAuto     = false,
                            PagManual   = false, 
                            PagAbout    = false, 
                            PagConfig   = false, 
                            PagLogin    = false;
        
        public static DataSet   Ds_Refs, 
                                Ds_Lang, 
                                Ds_Params, 
                                Ds_Init,
                                Ds_IO,
                                Ds_Counters,
                                Ds_Users,
                                Ds_CylindersLoad,
                                Ds_CylindersWork,
                                Ds_Scanners,
                                Ds_Axis;

        public static string SelectLang;

        public static _cUpdateIO UpdateIOs                      = new _cUpdateIO();
        public static _cIOComunication IoComunication           = new _cIOComunication();
        public static LoginView UserInfo                        = new LoginView();
        public static BindingList<_cDigitalIO> DigitalInputs    = new BindingList<_cDigitalIO>();
        public static BindingList<_cDigitalIO> DigitalOutputs   = new BindingList<_cDigitalIO>();
        public static List<_cAnalogIO> AnalogInputs             = new List<_cAnalogIO>();
        public static List<_cAnalogIO> AnalogOutputs            = new List<_cAnalogIO>();
        public static List<_cCylinder> Cylinders                = new List<_cCylinder>();
        public static List<_cAxis>  Axis                        = new List<_cAxis>();
        public static List<_cScanner> Scanners                  = new List<_cScanner>();


        #region Variables from the InitXML information


        #endregion
    }
}
