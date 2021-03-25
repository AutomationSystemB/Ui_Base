using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HamburgerMenuApp
{
    class _cIOComunication
    {
        //Comunication type(modbus,fins,...)
        private static _cGlobalVariables.Comunications _pComunication;
        // case modbus
        private static string _pModbusIP;
        private static int _pModbusPort;
        public static _cModbusManager ConnectionModbus;

        #region Properties and metods
        // implement properties and metods
        public void SetComunication(_cGlobalVariables.Comunications value )
        {
              _pComunication = value; 
        }
        public _cGlobalVariables.Comunications GetComunication()
        {
           return _pComunication;
        }
        public string ModbusIP
        {
            set { _pModbusIP = value; }
        }
        public int ModbusPort
        {
            set { _pModbusPort = value; }
        }
        #endregion

        #region prepare and initialize the comunication with choosed device
        public void SetComunication()
        {
            switch(_pComunication)
            {
                case _cGlobalVariables.Comunications.Modbus:
                    ConnectionModbus = new _cModbusManager();
                    ConnectionModbus.Connect(_pModbusIP, _pModbusPort);
                    break;
            }
        }

        #endregion

        #region Write outputs to comunication
        // write a single output
        public void Write2ComunicationSingle(short Pos,bool value)
        {
            switch (_pComunication)
            {
                case _cGlobalVariables.Comunications.Modbus:
                    ConnectionModbus.WriteSingleDO(Pos,value);
                    break;
            }
        }
        // write multiple outputs 
        public void Write2ComunicationMultiple(short firstAddress,byte[] Values)
        {
            switch (_pComunication)
            {
                case _cGlobalVariables.Comunications.Modbus:
                    ConnectionModbus.WriteMultipleDO(firstAddress, Values);
                    break;
            }
        }
        #endregion

        #region read Inputs and output functions from comunication

        public void ReadIO()
        {
            switch (_pComunication)
            {
                case _cGlobalVariables.Comunications.Modbus:
                    ConnectionModbus.ReadMultipleDO(0, 128);
                    ConnectionModbus.ReadMultipleIN(0, 128);
                    break;
            }
        }
        public bool ReadFromComunicationSingle(short Pos)
        {
            switch (_pComunication)
            {
                case _cGlobalVariables.Comunications.Modbus:
                    return ConnectionModbus.ReadSingleIO(Pos);
                    break;
                default:
                    return false;
                    break;
            }
        }

        public bool ReadFromComunicationSingle(string Pos)
        {
            switch (_pComunication)
            {
                case _cGlobalVariables.Comunications.Modbus:
                    short index = -1;
                    for (int i = 0; i < _cGlobalVariables.DigitalInputs.Count; i++)
                    {
                        if (_cGlobalVariables.DigitalInputs[i].name.Equals(Pos))
                        {
                            index = _cGlobalVariables.DigitalInputs[i].index;
                            i = _cGlobalVariables.DigitalInputs.Count + 1;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return ConnectionModbus.ReadSingleIO(index);
                    break;
                default:
                    return false;
                    break;
            }
        }
        #endregion

        #region Connection Status   
        //  get connection status
        public bool GetConectionStatus()
        {
            switch (_pComunication)
            {
                case _cGlobalVariables.Comunications.Modbus:
                    if (ConnectionModbus.GetConectionStatus())
                        return true;
                    else
                        return false;
                    break;
                default:
                    return false;
                    break;
            }
        }
        #endregion
    }
}
