using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Data;

namespace HamburgerMenuApp
{
    public class _cModbusManager
    {
        _cModBus modBusConnection;
        

        public _cModbusManager()
        {
            modBusConnection = new _cModBus();
        }

        //  get connection status
        public bool GetConectionStatus()
        {
            if (modBusConnection.wsStatus == 1)
                return true;
            else
                return false;
        }

        // Connection
        public bool Connect(string ip, int port)
        {
            modBusConnection.wsIPAddress = ip;
            modBusConnection.wsPort = port;
            modBusConnection.ConnectToServer();
            Thread.Sleep(200);
            return Convert.ToBoolean(modBusConnection.wsStatus);
        }
        //Disconnect
        public bool Disconnect()
        {
            modBusConnection.CloseSockect();

            return Convert.ToBoolean(modBusConnection.wsStatus);
        }

        #region Write function
        // Write
        public bool WriteSingleDO(short address, bool bValue)
        {
            if (modBusConnection.WriteDigitalOutput(address, bValue) == 0)
                return true;
            else
                return false;
        }
        // write multiple outputs
        public bool WriteMultipleDO(short firstAddress,byte[] Values)
        {

            if (modBusConnection.WriteMultipleDigitalOutputs(firstAddress, Values.Length, Values) == 0)
                return true;
            else
                return false;
        }
        #endregion

        #region Read Functions
        // read Output state from modbus and update Õutput structure
        public bool ReadMultipleDO(short firstAddress, Int16 count)
        {
           
            byte[] bValue= new byte[128];
            if (modBusConnection.ReadDigitalOutput(firstAddress, count, bValue) == 0)
            {
                int h = 0;
                BitArray bitarray = new BitArray(BitConverter.GetBytes(bValue[0]));
                for (int i = 0; h < _cGlobalVariables.DigitalOutputs.Count; i++)
                {
                    bitarray = new BitArray(BitConverter.GetBytes(bValue[i]));
                    for (int j = 0; j < bitarray.Length-8; j++)
                    {
                        _cGlobalVariables.DigitalOutputs[h+j].State = bitarray[j];
                    }
                    h=h+8;
                }
                return true;
            }
            else
                return false;
        }
        // read Input state from modbus and update input structure
        public bool ReadMultipleIN(short firstAddress, Int16 count)
        {

            byte[] bValue = new byte[128];
            if (modBusConnection.ReadDigitalInput(firstAddress, count, bValue) == 0)
            {
                int h = 0;
                BitArray bitarray = new BitArray(BitConverter.GetBytes(bValue[0]));
                for (int i = 0; h < _cGlobalVariables.DigitalInputs.Count; i++)
                {
                    bitarray = new BitArray(BitConverter.GetBytes(bValue[i]));
                    for (int j = 0; j < bitarray.Length - 8; j++)
                    {
                        _cGlobalVariables.DigitalInputs[h + j].State = bitarray[j];
                    }
                    h = h + 8;
                }
                return true;
            }
            else
                return false;
        }
        // read single Input
        public bool ReadSingleIO(short index)
        {
            int IdxByte = index / 8;
            int idxBit = index - (IdxByte*8);
            byte[] bValue = new byte[128];
            if (modBusConnection.ReadDigitalInput(0, 128, bValue) == 0)
            {
               
                BitArray bitarray = new BitArray(BitConverter.GetBytes(bValue[IdxByte]));
                return bitarray[idxBit];
            }
            else
                return false;
        }

        #endregion
    }

}
