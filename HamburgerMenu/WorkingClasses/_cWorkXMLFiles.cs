using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.IO;
using System.IO.Ports;

using System.Net.Sockets;
using System.Net;

using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace HamburgerMenuApp
{
    class _cWorkXMLFiles
    {
       
        private _cMachineState MachineState = new _cMachineState();

        public bool InitVars()
        {
            try
            {

                //---------------------------------------------------------------------------------------
                //                                    INIT.XML FILE
                //---------------------------------------------------------------------------------------
                LoadDataSets();
                

                //Linguagem predefinida:
                _cGlobalVariables.SelectLang = _cGlobalVariables.Ds_Init.Tables[0].Rows[0]                                      [ "DEFAULT_LANGUAGE" ].ToString().ToUpper();
                //Definições da maquina:
                MachineState.SetLoginOnStart    ( Convert.ToBoolean(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0] [ "LoginOnstart"     ].ToString())));
                MachineState.SetStationName     (_cGlobalVariables.Ds_Init.Tables[0].Rows[0]                                    [ "STATION_NAME"     ].ToString());
                MachineState.SetUseDayCounter   (Convert.ToBoolean(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0]  [ "DayCounter"       ].ToString())));
                MachineState.SetUseTotalCounter (Convert.ToBoolean(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0]  [ "TotalCounter"     ].ToString())));
                MachineState.SetUsePuttyPaste   (Convert.ToBoolean(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0]  [ "PuttyPaste"       ].ToString())));
                MachineState.SetUseScrewDriver  (Convert.ToBoolean(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0]  [ "Screwdriver"      ].ToString())));
                MachineState.SetUseCim          (Convert.ToBoolean(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0]  [ "CIM"              ].ToString())));
                
                _cGlobalVariables.IoComunication.SetComunication((_cGlobalVariables.Comunications)(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0]["COMUNICATION"].ToString())));
                //Endereço IP para ligação ao BK9000/9100
                switch (_cGlobalVariables.IoComunication.GetComunication())
                {
                    case _cGlobalVariables.Comunications.Modbus:
                        _cGlobalVariables.IoComunication.ModbusIP    = _cGlobalVariables.Ds_Init.Tables[0].Rows[0]                   [ "BK_IP_ADDR" ].ToString();
                        _cGlobalVariables.IoComunication.ModbusPort  = Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0]   [ "BK_PORT"    ].ToString());
                        _cGlobalVariables.IoComunication.SetComunication();
                        break;

                    default:
                        break;
                }

                //Escolhas de menu

                _cGlobalVariables.PagAuto   = Convert.ToBoolean(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0][ "Auto"             ].ToString()));
                _cGlobalVariables.PagManual = Convert.ToBoolean(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0][ "Manual"           ].ToString()));
                _cGlobalVariables.PagLogin  = Convert.ToBoolean(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0][ "Login"            ].ToString()));
                _cGlobalVariables.PagConfig = Convert.ToBoolean(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0][ "Configurations"   ].ToString()));
                _cGlobalVariables.PagAbout  = Convert.ToBoolean(Convert.ToInt16(_cGlobalVariables.Ds_Init.Tables[0].Rows[0][ "About"            ].ToString()));

                //---------------------------------------------------------------------------------------
                //                                  LANG.XML FILE
                //---------------------------------------------------------------------------------------


                DataColumn[] Keys = new DataColumn[1];
                Keys[0] = _cGlobalVariables.Ds_Lang.Tables[0].Columns["Tag"];
                _cGlobalVariables.Ds_Lang.Tables[0].PrimaryKey = Keys;

                // Load IOs
                short i=0, j=0, h=0, k = 0;
                foreach (DataRow IO in _cGlobalVariables.Ds_IO.Tables[0].Rows)
                {
                    if (IO["IOType"].ToString() == "DI")
                    {
                        _cDigitalIO IOtoAdd = new _cDigitalIO();
                        IOtoAdd.index       = i;
                        IOtoAdd.name        = IO["IOName"].ToString();
                        IOtoAdd.State       = false;
                        _cGlobalVariables.DigitalInputs.Add(IOtoAdd);
                        i++;
                    }
                    if (IO["IOType"].ToString() == "DO")
                    {
                        _cDigitalIO IOtoAdd = new _cDigitalIO();
                        IOtoAdd.index       = j;
                        IOtoAdd.name        = IO["IOName"].ToString();
                        IOtoAdd.State = false;
                        _cGlobalVariables.DigitalOutputs.Add(IOtoAdd);
                        j++;
                    }
                    if (IO["IOType"].ToString() == "AI")
                    {
                        _cAnalogIO AnIOtoAdd    = new _cAnalogIO();
                        AnIOtoAdd.index         = h;
                        AnIOtoAdd.name          = IO["IOName"].ToString();
                        AnIOtoAdd.value         = 0.0;
                        _cGlobalVariables.AnalogInputs.Add(AnIOtoAdd);
                        h++;
                    }
                    if (IO["IOType"].ToString() == "AO")
                    {
                        _cAnalogIO AnIOtoAdd    = new _cAnalogIO();
                        AnIOtoAdd.index         = k;
                        AnIOtoAdd.name          = IO["IOName"].ToString();
                        AnIOtoAdd.value         = 0.0;
                        _cGlobalVariables.AnalogOutputs.Add(AnIOtoAdd);
                        k++;
                    }
                }
                // Load Cylinders
              
                if (_cGlobalVariables.Ds_CylindersLoad.Tables.Count >0)
                {
                    _cGlobalVariables.Ds_CylindersWork = _cGlobalVariables.Ds_CylindersLoad.Clone();
                    _cGlobalVariables.Ds_CylindersWork.Clear();
                    foreach (DataRow Cyl in _cGlobalVariables.Ds_CylindersLoad.Tables[0].Rows)
                    {
                        short Input_H, Input_W, Output_H, Output_W;
                        double time;
                        bool Alreadyread = false;
                        switch (Convert.ToInt16(Cyl["ConfigType"].ToString()))
                        {
                            case 1:
                                Input_H = Convert.ToInt16(Cyl["InHomeIdx"].ToString());
                                Input_W = Convert.ToInt16(Cyl["InWorkIdx"].ToString());
                                Output_H = Convert.ToInt16(Cyl["OutHomeIdx"].ToString());
                                Output_W = Convert.ToInt16(Cyl["OutWorkIdx"].ToString());
                                time = Convert.ToDouble(Cyl["Timeout"].ToString());
                                _cCylinder CyltoAdd = new _cCylinder(Input_H, Input_W, Output_H, Output_W, time);
                                CyltoAdd.Name = Cyl["Name"].ToString();
                                _cGlobalVariables.Cylinders.Add(CyltoAdd);
                                _cGlobalVariables.Ds_CylindersWork.Tables[0].Rows.Add(Cyl.ItemArray);
                                break;
                            case 2:
                                for (int CylCnt = 0; CylCnt < _cGlobalVariables.Cylinders.Count; CylCnt++)
                                {
                                    if (_cGlobalVariables.Cylinders[CylCnt].Name.Equals(Cyl["Name"].ToString()))
                                        Alreadyread = true;
                                }
                                if (!Alreadyread)
                                {
                                    List<short> InputArray_H = new List<short>(),
                                                InputArray_W = new List<short>();
                                    foreach (DataRow Cylinder in _cGlobalVariables.Ds_CylindersLoad.Tables[0].Rows)
                                    {
                                        if (Cyl["Name"].ToString().Equals(Cylinder["Name"].ToString()))
                                        {
                                            InputArray_H.Add(Convert.ToInt16(Cylinder["InHomeIdx"].ToString()));
                                            InputArray_W.Add(Convert.ToInt16(Cylinder["InWorkIdx"].ToString()));
                                        }
                                    }
                                    Output_H = Convert.ToInt16(Cyl["OutHomeIdx"].ToString());
                                    Output_W = Convert.ToInt16(Cyl["OutWorkIdx"].ToString());
                                    time = Convert.ToDouble(Cyl["Timeout"].ToString());
                                    _cCylinder CyltoAdd2 = new _cCylinder(InputArray_H, InputArray_W, Output_H, Output_W, time);
                                    CyltoAdd2.Name = Cyl["Name"].ToString();
                                    _cGlobalVariables.Cylinders.Add(CyltoAdd2);
                                    _cGlobalVariables.Ds_CylindersWork.Tables[0].Rows.Add(Cyl.ItemArray);
                                }
                                break;
                        }

                    }
                }
                // Load Axis
                if (_cGlobalVariables.Ds_Axis.Tables.Count > 0)
                {
                    foreach (DataRow axis in _cGlobalVariables.Ds_Axis.Tables[0].Rows)
                    {
                        _cAxis Axis2add = new _cAxis(Convert.ToInt16(axis["ConfigType"].ToString()));
                        switch (Convert.ToInt16(axis["ConfigType"].ToString()))
                        {
                            case 2:
                                Axis2add.SetBaudrate(Convert.ToInt16(axis["BaudRate"].ToString()));
                                Axis2add.SetIAIId(Convert.ToByte(axis["ConfigType"].ToString()));
                                break;
                        }
                        Axis2add.connect(axis["Comms"].ToString(),
                                            Convert.ToInt16(axis["LimitMax"].ToString()),
                                            Convert.ToInt16(axis["LimitMin"].ToString()),
                                            Convert.ToInt16(axis["Timeout"].ToString()));
                        _cGlobalVariables.Axis.Add(Axis2add);
                    }
                }
                if (_cGlobalVariables.Ds_Scanners.Tables.Count > 0)
                {
                    foreach (DataRow scanner in _cGlobalVariables.Ds_Scanners.Tables[0].Rows)
                    {
                        _cScanner Scanner2Add = new _cScanner();
                        switch (Convert.ToInt16(scanner["ConfigType"].ToString()))
                        {
                            case 1:
                                Scanner2Add.SetName(scanner["Name"].ToString());
                                Scanner2Add.SetType(1);
                                Scanner2Add.SetTimeout(Convert.ToInt16(scanner["Timeout"].ToString()));
                                Scanner2Add.SetComPort( scanner["ComPort"].ToString());
                                Scanner2Add.SetBaudRate( Convert.ToInt32(scanner["BaudRate"].ToString()));
                                Scanner2Add.SetStopBits ((StopBits) Convert.ToInt16(scanner["StopBits"].ToString()));
                                Scanner2Add.SetDataBits ( Convert.ToInt16(scanner["DataBits"].ToString()));
                                Scanner2Add.SetParity ((Parity) Convert.ToInt16(scanner["Parity"].ToString()));
                                Scanner2Add.SetHandler ((ComPort.Handler) Convert.ToInt16(scanner["Handler"].ToString()));
                                Scanner2Add.SetTrigger(scanner["Trigger"].ToString());
                                Scanner2Add.Connect();
                                break;
                        }
                        
                        _cGlobalVariables.Scanners.Add(Scanner2Add);
                    }
                }
                return true;
            }
            catch (Exception exp)
            {
                MessageBox.Show(TextByTag(1037) + exp.ToString());
                return false;
            }
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


        private void LoadDataSets()
        {
            _cGlobalVariables.Ds_Init.ReadXml               (   "INIT.xml"      );
            _cGlobalVariables.Ds_Lang.ReadXml               (   "LANG.xml"      );
            _cGlobalVariables.Ds_IO.ReadXml                 (   "IO.XML"        );
            _cGlobalVariables.Ds_Params.ReadXml             (   "Params.XML"    );
            _cGlobalVariables.Ds_Refs.ReadXml               (   "Refs.XML"      );
            _cGlobalVariables.Ds_Users.ReadXml              (   "Users.XML"     );
            if(File.Exists  (   "Counters.XML"  ))
                _cGlobalVariables.Ds_Counters.ReadXml       (   "Counters.XML"  );
            if (File.Exists (   "Cylinders.XML" ))
                _cGlobalVariables.Ds_CylindersLoad.ReadXml  (   "Cylinders.XML" );
            if (File.Exists (   "Axis.XML"      ))
                _cGlobalVariables.Ds_Axis.ReadXml           (   "Axis.XML"      );
            if (File.Exists (   "Scanners.XML"  ))
                _cGlobalVariables.Ds_Scanners.ReadXml       (   "Scanners.XML"  );

        }
    }


}
