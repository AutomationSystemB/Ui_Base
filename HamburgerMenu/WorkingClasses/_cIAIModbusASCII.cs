using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Data;

namespace HamburgerMenuApp
{
    class _cIAIModbusASCII
    {
        #region properties

        private string PORT;
        public byte ID;
        public string Port
        {
            get { return PORT; }
            set { PORT = value; }
        }

        private int BAUDRATE;
        public int BaudRate
        {
            get { return BAUDRATE; }
            set { BAUDRATE = value; }
        }

        private string ERRORSTRING;
        public string ErrorString
        {
            get { return ERRORSTRING; }
            set { ERRORSTRING = value; }
        }

        #endregion

        #region Constructors
        public _cIAIModbusASCII()
        {
            
        }

        public void  FillConnectionInfo(String CommPort, byte eixoId, int Baud)
        {
            ID = eixoId;
            int temp = Convert.ToInt32(ID) + 1;
            ID = Convert.ToByte(temp);
            PORT = CommPort;
            BAUDRATE = Baud;
        }

        #endregion


        SerialPort comPort;
        private byte TimeOut = 10;    // 10 msec

        #region Device control register 1
        const ushort SafetySpeedCommand = 0x0401;
        const ushort ServoOnCommand = 0x0403;
        const ushort AlarmResetCommand = 0x0407;
        const ushort BrakeForceReleaseCommand = 0x0408;
        const ushort PauseCommand = 0x040A;
        const ushort HomingCommand = 0x040B;
        const ushort PositionStartCommand = 0x040C;
        const ushort JogInchCommand = 0x0411;
        const ushort TeachingModeCommand = 0x0414;
        const ushort PositionDataLoadComamnd = 0x415;
        const ushort JogPlusCommand = 0x0416;
        const ushort JogMinusCommand = 0x0417;

        const ushort ModeValidModbus = 0x0427;
        #endregion

        #region Controller Monitor Information Registers
        const ushort CurrentPositionMonitor = 0x9000;
        const ushort PresentAlarmCodeQuery = 0x9002;
        const ushort InputPortQuery = 0x9003;
        const ushort OutputPortMonitorQuery = 0x9004;
        const ushort DeviceStatusQuery1 = 0x9005;
        const ushort DeviceStatusQuery2 = 0x9006;
        const ushort ExpansionDeviceStatusQuery = 0x9007;
        const ushort SystemStatusQuery = 0x9008;
        const ushort CurrentSpeedMonitor = 0x900A;
        const ushort CurrentAmpereMonitor = 0x900C;
        const ushort DeviationMonitor = 0x900E;
        const ushort SystemTimerQuery = 0x9010;
        const ushort SpecialInputPortQuery = 0x9012;
        const ushort ZoneStatusQuery = 0x9013;
        const ushort CompletePositionNumberStatusQuery = 0x9014;
        #endregion

        #region Device Status register
        public ushort ServoOnStatus = 0x0103;
        public ushort HomingCompletationStatus = 0x010B;
        public ushort PositioningCompletationStatus = 0x10C;
        #endregion

        public bool OpenComPort()
        {
            try
            {
                comPort = new SerialPort(PORT, BAUDRATE, Parity.None, 8, StopBits.One)
                {
                    Handshake = Handshake.None,
                    ReceivedBytesThreshold = 1
                };
                comPort.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void CloseComPort()
        {
            if (comPort.IsOpen) comPort.Close();
        }

        #region primitive functions
        private int Convert4ByteToInt(byte Byte1, byte Byte2, byte Byte3, byte Byte4)
        {
            return Byte3 * (int)Math.Pow(256, 3) + Byte4 * (int)Math.Pow(256, 2) + Byte1 * 256 + Byte2;
        }

        private byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return newByte;

        }

        private string BuildAsciiCommand(byte[] chkbuf, byte len)
        {
            string CRLF = "\r\n";
            uint LRC;
            string auxLRC = ":";

            byte uchLRC = 0;

            for (int i = 0; i < len; i++)
            {
                uchLRC += chkbuf[i];
            }
            LRC = ((byte)(-((uchLRC))));

            //junta a trama toda em Hexa("X")
            for (int i = 0; i < len; i++)
            {
                auxLRC = auxLRC + chkbuf[i].ToString("X").PadLeft(2, '0');
            }

            string strCommand = auxLRC + LRC.ToString("X").PadLeft(2, '0') + CRLF;

            return strCommand;
        }

        private bool ReadCoilStatus(ushort StartAddress, ushort Count, byte[] RetBuf)
        {
            byte[] ar = new byte[6];
            //UInt16 CRC=0;
            const byte FunctionCode = 0x01;

            ar[0] = ID;
            ar[1] = FunctionCode;                           // function code
            ar[2] = Convert.ToByte(StartAddress / 256);     // High Address to read from
            ar[3] = Convert.ToByte(StartAddress % 256);     // Low Address to read from
            ar[4] = Convert.ToByte(Count / 256);            // Count High 
            ar[5] = Convert.ToByte(Count % 256);            // Count Low (number of locations to be read)

            string AsciiCommand = BuildAsciiCommand(ar, 6);

            comPort.Write(AsciiCommand);

            //comPort.Write(ar, 0, 8);
            System.Threading.Thread.Sleep(TimeOut * ar.Length);
            comPort.Read(RetBuf, 0, comPort.BytesToRead);

            if ((RetBuf[0] == ID) && (RetBuf[1] == FunctionCode))
                return true;
            else
            {
                ErrorString = "mensagem corrompida";
                return false;
            }
        }

        private bool ReadInputStatus(ushort StartAddress, ushort Count, byte[] RetBuf)
        {
            byte[] ar = new byte[6];
            //UInt16 CRC=0;
            const byte FunctionCode = 0x02;

            ar[0] = ID;
            ar[1] = FunctionCode;                           // function code
            ar[2] = Convert.ToByte(StartAddress / 256);     // High Address to read from
            ar[3] = Convert.ToByte(StartAddress % 256);     // Low Address to read from
            ar[4] = Convert.ToByte(Count / 256);            // Count High 
            ar[5] = Convert.ToByte(Count % 256);            // Count Low (number of locations to be read)
                                                            //RV//CRC = CalcCRC(ar, 6);
                                                            //ar[6] = Convert.ToByte(CRC / 256);
                                                            //ar[7] = Convert.ToByte(CRC % 256);

            string AsciiCommand = BuildAsciiCommand(ar, 6);

            comPort.Write(AsciiCommand);

            //comPort.Write(ar, 0, 8);
            System.Threading.Thread.Sleep(TimeOut * ar.Length);
            comPort.Read(RetBuf, 0, comPort.BytesToRead);

            byte SlaveAdd_Received = HexToByte(Convert.ToChar(RetBuf[1]).ToString() + Convert.ToChar(RetBuf[2]).ToString());
            byte FunctionCode_Received = HexToByte(Convert.ToChar(RetBuf[3]).ToString() + Convert.ToChar(RetBuf[4]).ToString());

            if ((SlaveAdd_Received == ID) && (FunctionCode_Received == FunctionCode))
                return true;
            else
            {
                ErrorString = "mensagem corrompida";
                return false;
            }
        }

        private bool ForceSingleCoil(ushort StartAddress, bool Status, byte[] RetBuf)
        {
            byte[] ar = new byte[6];
            //UInt16 CRC = 0;
            const byte FunctionCode = 0x05;

            ar[0] = ID;
            ar[1] = FunctionCode;                           // function code
            ar[2] = Convert.ToByte(StartAddress / 256);     // High Address to read from
            ar[3] = Convert.ToByte(StartAddress % 256);     // Low Address to read from
            if (Status == true)
            {
                ar[4] = 0xFF;
                ar[5] = 0x00;
            }
            else
            {
                ar[4] = 0x00;
                ar[5] = 0x00;
            }
            //RV//CRC = CalcCRC(ar, 6);
            //ar[6] = Convert.ToByte(CRC / 256);
            //ar[7] = Convert.ToByte(CRC % 256);

            string AsciiCommand = BuildAsciiCommand(ar, 6);

            comPort.Write(AsciiCommand);

            //comPort.Write(ar, 0, 8);
            System.Threading.Thread.Sleep(TimeOut * ar.Length);
            comPort.Read(RetBuf, 0, comPort.BytesToRead);

            byte SlaveAdd_Received = HexToByte(Convert.ToChar(RetBuf[1]).ToString() + Convert.ToChar(RetBuf[2]).ToString());
            byte FunctionCode_Received = HexToByte(Convert.ToChar(RetBuf[3]).ToString() + Convert.ToChar(RetBuf[4]).ToString());

            if ((SlaveAdd_Received == ID) && (FunctionCode_Received == FunctionCode))
                return true;
            else
            {
                ErrorString = "mensagem corrompida";
                return false;
            }
        }

        private bool ReadHoldingRegisters(ushort StartAddress, ushort Count, byte[] RetBuf)
        {
            byte[] ar = new byte[6];
            //UInt16 CRC = 0;
            const byte FunctionCode = 0x03;

            ar[0] = ID;
            ar[1] = FunctionCode;                           // function code
            ar[2] = Convert.ToByte(StartAddress / 256);     // High Address to read from
            ar[3] = Convert.ToByte(StartAddress % 256);     // Low Address to read from
            ar[4] = Convert.ToByte(Count / 256);            // Count High 
            ar[5] = Convert.ToByte(Count % 256);            // Count Low (number of locations to be read)
                                                            //RV//CRC = CalcCRC(ar, 6);
                                                            //ar[6] = Convert.ToByte(CRC / 256);
                                                            //ar[7] = Convert.ToByte(CRC % 256);

            string AsciiCommand = BuildAsciiCommand(ar, 6);

            comPort.Write(AsciiCommand);

            //comPort.Write(ar, 0, 8);
            System.Threading.Thread.Sleep(TimeOut * ar.Length);
            comPort.Read(RetBuf, 0, comPort.BytesToRead);

            byte SlaveAdd_Received = HexToByte(Convert.ToChar(RetBuf[1]).ToString() + Convert.ToChar(RetBuf[2]).ToString());
            byte FunctionCode_Received = HexToByte(Convert.ToChar(RetBuf[3]).ToString() + Convert.ToChar(RetBuf[4]).ToString());

            if ((SlaveAdd_Received == ID) && (FunctionCode_Received == FunctionCode))
                return true;
            else
            {
                ErrorString = "mensagem corrompida";
                return false;
            }
        }

        private void ReadInputRegisters()
        {

        }

        private bool PresetSingleRegister(ushort StartAddress, Int16 Value, byte[] RetBuf)
        {
            byte[] ar = new byte[6];
            //UInt16 CRC = 0;
            const byte FunctionCode = 0x6;

            ar[0] = ID;
            ar[1] = FunctionCode;                           // function code
            ar[2] = Convert.ToByte(StartAddress / 256);     // High Address to read from
            ar[3] = Convert.ToByte(StartAddress % 256);     // Low Address to read from
            ar[4] = Convert.ToByte(Value / 256);            // Value High 
            ar[5] = Convert.ToByte(Value % 256);            // Value Low 

            //RV//CRC = CalcCRC(ar, 6);
            //ar[6] = Convert.ToByte(CRC / 256);
            //ar[7] = Convert.ToByte(CRC % 256);

            string AsciiCommand = BuildAsciiCommand(ar, 6);

            comPort.Write(AsciiCommand);

            //comPort.Write(ar, 0, 8);
            System.Threading.Thread.Sleep(TimeOut * ar.Length);
            comPort.Read(RetBuf, 0, comPort.BytesToRead);

            byte SlaveAdd_Received = HexToByte(Convert.ToChar(RetBuf[1]).ToString() + Convert.ToChar(RetBuf[2]).ToString());
            byte FunctionCode_Received = HexToByte(Convert.ToChar(RetBuf[3]).ToString() + Convert.ToChar(RetBuf[4]).ToString());

            if ((SlaveAdd_Received == ID) && (FunctionCode_Received == FunctionCode))
                return true;
            else
            {
                ErrorString = "mensagem corrompida";
                return false;
            }
        }

        public bool ReadExeptionStatus(ref byte ExceptionStatus)
        {
            byte[] RetBuf = new byte[5];
            byte[] ar = new byte[2];
            //UInt16 CRC = 0;
            const byte FunctionCode = 0x07;

            ar[0] = ID;
            ar[1] = FunctionCode;                           // function code
                                                            //RV//CRC = CalcCRC(ar, 2);
                                                            //ar[2] = Convert.ToByte(CRC / 256);
                                                            //ar[3] = Convert.ToByte(CRC % 256);

            string AsciiCommand = BuildAsciiCommand(ar, 2);

            comPort.Write(AsciiCommand);

            //comPort.Write(ar, 0, 4);
            System.Threading.Thread.Sleep(TimeOut * ar.Length);
            comPort.Read(RetBuf, 0, comPort.BytesToRead);

            byte SlaveAdd_Received = HexToByte(Convert.ToChar(RetBuf[1]).ToString() + Convert.ToChar(RetBuf[2]).ToString());
            byte FunctionCode_Received = HexToByte(Convert.ToChar(RetBuf[3]).ToString() + Convert.ToChar(RetBuf[4]).ToString());

            if ((SlaveAdd_Received == ID) && (FunctionCode_Received == FunctionCode))
            {
                ExceptionStatus = RetBuf[2];
                return true;
            }
            else
            {
                ErrorString = "mensagem corrompida";
                return false;
            }
        }

        private void ForceMultipleCoils()
        {

        }

        private bool PresetMultipleRegisters(ushort StartAddress, ushort RegisterCount,
                                                                ushort ByteCount, byte[] InpBuf, byte[] RetBuf)
        {
            byte[] ar = new byte[6];
            //UInt16 CRC = 0;
            const byte FunctionCode = 0x10;   // 16 dec

            ar[0] = ID;
            ar[1] = FunctionCode;                           // function code
            ar[2] = Convert.ToByte(StartAddress / 256);     // High Address to read from
            ar[3] = Convert.ToByte(StartAddress % 256);     // Low Address to read from
            ar[4] = Convert.ToByte(RegisterCount / 256);    // Count High 
            ar[5] = Convert.ToByte(RegisterCount % 256);    // Count Low (number of locations to be read)

            //RV//CRC = CalcCRC(ar, 6);
            //ar[6] = Convert.ToByte(CRC / 256);
            //ar[7] = Convert.ToByte(CRC % 256);

            string AsciiCommand = BuildAsciiCommand(ar, 6);

            comPort.Write(AsciiCommand);

            //comPort.Write(ar, 0, 8);
            System.Threading.Thread.Sleep(TimeOut * ar.Length);
            comPort.Read(RetBuf, 0, comPort.BytesToRead);

            byte SlaveAdd_Received = HexToByte(Convert.ToChar(RetBuf[1]).ToString() + Convert.ToChar(RetBuf[2]).ToString());
            byte FunctionCode_Received = HexToByte(Convert.ToChar(RetBuf[3]).ToString() + Convert.ToChar(RetBuf[4]).ToString());

            if ((SlaveAdd_Received == ID) && (FunctionCode_Received == FunctionCode))
                return true;
            else
            {
                ErrorString = "mensagem corrompida";
                return false;
            }
        }

        private void ReportSlave()
        {

        }

        private void ReadWritRegisters()
        {

        }

        #endregion

        #region ForceSingleCoil -- Write
        public bool ServoOn(bool SOn)
        {
            byte[] ar = new byte[255];
            return ForceSingleCoil(ServoOnCommand, SOn, ar);
        }

        public bool SafetySpeed(bool OnOff)
        {
            byte[] ar = new byte[255];
            return ForceSingleCoil(SafetySpeedCommand, OnOff, ar);
        }

        public bool ChangeModeValidModbus(bool OnOff)
        {
            byte[] ar = new byte[255];
            return ForceSingleCoil(ModeValidModbus, OnOff, ar);
        }

        public bool AlarmReset()
        {
            byte[] ar = new byte[255];
            return (ForceSingleCoil(AlarmResetCommand, true, ar) && ForceSingleCoil(AlarmResetCommand, false, ar));
        }

        public bool Pause(bool Status)
        {
            byte[] ar = new byte[255];
            return ForceSingleCoil(PauseCommand, Status, ar);
        }

        public bool Homing()
        {
            byte[] ar = new byte[255];

            return ForceSingleCoil(HomingCommand, true, ar) && ForceSingleCoil(HomingCommand, false, ar);
        }

        public bool PositionStart(byte ID)
        {
            byte[] ar = new byte[255];
            return (ForceSingleCoil(PositionStartCommand, true, ar) && ForceSingleCoil(PositionStartCommand, false, ar));
        }

        /// <summary>
        /// true->Jogging
        /// false->Inching
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="OnOff"></param>
        /// <returns></returns>
        public bool JogInchSwitching(bool OnOff)
        {
            byte[] ar = new byte[255];
            return ForceSingleCoil(JogInchCommand, OnOff, ar);
        }

        public bool JogPlus(bool OnOff)
        {
            byte[] ar = new byte[255];
            return ForceSingleCoil(JogPlusCommand, OnOff, ar);
        }

        public bool JogMinus(bool OnOff)
        {
            byte[] ar = new byte[255];
            return ForceSingleCoil(JogMinusCommand, OnOff, ar);
        }

        /// <summary>
        /// Acelaração e Desacelaração juntas
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="TargetPosition"></param>
        /// <param name="InPositionBand"></param>
        /// <param name="Speed"></param>
        /// <param name="AccDec"></param>
        /// <param name="PushCurrentLimiting"></param>
        /// <param name="ControlFlag"></param>
        /// <param name="RetBuf"></param>
        /// <returns></returns>      
        public bool MoveActuator(int TargetPosition, ushort InPositionBand,
                        uint Speed, ushort AccDec, ushort PushCurrentLimiting, ushort ControlFlag, byte[] RetBuf)
        {
            byte[] ar = new byte[25];

            const byte FunctionCode = 0x10;   // 16 dec     

            int P3 = (int)Math.Pow(256, 3);
            int P2 = (int)Math.Pow(256, 2);
            int P1 = 256;
            int StartAddress = 0x9900;

            ar[0] = ID;
            ar[1] = FunctionCode;                           // function code
            ar[2] = Convert.ToByte(StartAddress / 256);     // High Address to read from
            ar[3] = Convert.ToByte(StartAddress % 256);     // Low Address to read from

            ar[4] = 0;
            ar[5] = 9;                                      // resgister count
            ar[6] = 18;                                     // byte count 

            //ar[7] = Convert.ToByte((TargetPosition % P3) / P2);
            //ar[8] = Convert.ToByte(TargetPosition / P3);
            ar[7] = Convert.ToByte(TargetPosition / P3);//RV
            ar[8] = Convert.ToByte((TargetPosition % P3) / P2);//RV	

            ar[9] = Convert.ToByte((TargetPosition % P2) / P1);
            ar[10] = Convert.ToByte((TargetPosition % P1));

            ar[11] = Convert.ToByte(InPositionBand / P3);//RV
            ar[12] = Convert.ToByte((InPositionBand % P3) / P2);//RV	

            ar[13] = Convert.ToByte((InPositionBand % P2) / P1);
            ar[14] = Convert.ToByte((InPositionBand % P1));

            ar[15] = Convert.ToByte(Speed / P3);//RV
            ar[16] = Convert.ToByte((Speed % P3) / P2);//RV	

            ar[17] = Convert.ToByte((Speed % P2) / P1);
            ar[18] = Convert.ToByte((Speed % P1));

            ar[19] = Convert.ToByte(AccDec / 256);
            ar[20] = Convert.ToByte(AccDec % 256);

            ar[21] = Convert.ToByte(PushCurrentLimiting / 256);
            ar[22] = Convert.ToByte(PushCurrentLimiting % 256);

            ar[23] = Convert.ToByte(ControlFlag / 256);
            ar[24] = Convert.ToByte(ControlFlag % 256);

            string AsciiCommand = BuildAsciiCommand(ar, 25);

            //ComportWrite(AsciiCommand);

            comPort.Write(AsciiCommand);

            System.Threading.Thread.Sleep(TimeOut * ar.Length);

            try
            {
                comPort.Read(RetBuf, 0, comPort.BytesToRead);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                return false;
            }



            byte SlaveAdd_Received = HexToByte(Convert.ToChar(RetBuf[1]).ToString() + Convert.ToChar(RetBuf[2]).ToString());
            byte FunctionCode_Received = HexToByte(Convert.ToChar(RetBuf[3]).ToString() + Convert.ToChar(RetBuf[4]).ToString());


            if ((SlaveAdd_Received == ID) && (FunctionCode_Received == FunctionCode))
                return true;
            else
            {
                ErrorString = "mensagem corrompida";
                return false;
            }
        }

        /// <summary>
        /// Acelaração e Desacelaração separadas
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="TargetPosition"></param>
        /// <param name="InPositionBand"></param>
        /// <param name="Speed"></param>
        /// <param name="AccDec"></param>
        /// <param name="PushCurrentLimiting"></param>
        /// <param name="ControlFlag"></param>
        /// <param name="RetBuf"></param>
        /// <returns></returns>
        public bool MoveActuator(uint TargetPosition, ushort InPositionBand,
                        uint Speed, int BondaryZonePosition_Low, int BondaryZonePosition_High,
                        ushort Acceleration, ushort Decceleration, ushort PushCurrentLimiting,
                        ushort LoadOutputCurrentThreshold, ushort ControlFlag, byte[] RetBuf)
        {
            byte[] ar = new byte[37];

            const byte FunctionCode = 0x10;   // 16 dec     

            int P3 = (int)Math.Pow(256, 3);
            int P2 = (int)Math.Pow(256, 2);
            int P1 = 256;
            int StartAddress = 0x10C0;

            ar[0] = ID;
            ar[1] = FunctionCode;                           // function code
            ar[2] = Convert.ToByte(StartAddress / 256);     // High Address to read from
            ar[3] = Convert.ToByte(StartAddress % 256);     // Low Address to read from

            ar[4] = 0;
            ar[5] = 15;                                      // resgister count
            ar[6] = 30;                                     // byte count 

            ar[7] = Convert.ToByte((TargetPosition % P3) / P2);
            ar[8] = Convert.ToByte(TargetPosition / P3);
            ar[9] = Convert.ToByte((TargetPosition % P2) / P1);
            ar[10] = Convert.ToByte((TargetPosition % P1));

            ar[11] = Convert.ToByte((InPositionBand % P3) / P2);
            ar[12] = Convert.ToByte(InPositionBand / P3);
            ar[13] = Convert.ToByte((InPositionBand % P2) / P1);
            ar[14] = Convert.ToByte((InPositionBand % P1));

            ar[15] = Convert.ToByte((Speed % P3) / P2);
            ar[16] = Convert.ToByte(Speed / P3);
            ar[17] = Convert.ToByte((Speed % P2) / P1);
            ar[18] = Convert.ToByte((Speed % P1));

            //Limites de uma qualquer zona que possamos querer activar uma saída no controlador High>Low
            ////////////////////////////////////////////////////////////////////////////////////////////
            ar[19] = Convert.ToByte((BondaryZonePosition_High % P3) / P2);
            ar[20] = Convert.ToByte(BondaryZonePosition_High / P3);
            ar[21] = Convert.ToByte((BondaryZonePosition_High % P2) / P1);
            ar[22] = Convert.ToByte((BondaryZonePosition_High % P1));

            ar[23] = Convert.ToByte((BondaryZonePosition_Low % P3) / P2);
            ar[24] = Convert.ToByte(BondaryZonePosition_Low / P3);
            ar[25] = Convert.ToByte((BondaryZonePosition_Low % P2) / P1);
            ar[26] = Convert.ToByte((BondaryZonePosition_Low % P1));
            /////////////////////////////////////////////////////////////////////////////////////////////

            ar[27] = Convert.ToByte(Acceleration / 256);
            ar[28] = Convert.ToByte(Acceleration % 256);

            ar[29] = Convert.ToByte(Decceleration / 256);
            ar[30] = Convert.ToByte(Decceleration % 256);

            ar[31] = Convert.ToByte(PushCurrentLimiting / 256);
            ar[32] = Convert.ToByte(PushCurrentLimiting % 256);

            ar[33] = Convert.ToByte(LoadOutputCurrentThreshold / 256);
            ar[34] = Convert.ToByte(LoadOutputCurrentThreshold % 256);

            ar[35] = Convert.ToByte(ControlFlag / 256);
            ar[36] = Convert.ToByte(ControlFlag % 256);

            string AsciiCommand = BuildAsciiCommand(ar, 36);

            //ComportWrite(AsciiCommand);

            comPort.Write(AsciiCommand);

            System.Threading.Thread.Sleep(TimeOut * ar.Length);
            comPort.Read(RetBuf, 0, comPort.BytesToRead);

            byte SlaveAdd_Received = HexToByte(Convert.ToChar(RetBuf[1]).ToString() + Convert.ToChar(RetBuf[2]).ToString());
            byte FunctionCode_Received = HexToByte(Convert.ToChar(RetBuf[3]).ToString() + Convert.ToChar(RetBuf[4]).ToString());


            if ((SlaveAdd_Received == ID) && (FunctionCode_Received == FunctionCode))
                return true;
            else
            {
                ErrorString = "mensagem corrompida";
                return false;
            }
        }
        #endregion

        #region (Read holding registers) -- Read Controller Monitor Information Registers

        public bool ReadCurrentPosition(ref int MotorPos)
        {
            byte[] ar = new byte[255];
            if (ReadHoldingRegisters(CurrentPositionMonitor, 2, ar))
            {
                char[] charAr = new char[8];
                byte[] byteAr = new byte[4];

                for (int i = 7; i < 15; i++)
                {
                    charAr[i - 7] = Convert.ToChar(ar[i]);
                }

                for (int i = 0; i < 4; i++)
                {
                    string strHex = charAr[2 * i].ToString() + charAr[2 * i + 1].ToString();
                    try
                    {
                        byteAr[i] = HexToByte(strHex);
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                        return false;
                    }
                }
                MotorPos = (byteAr[1] * (int)Math.Pow(256, 2) + byteAr[2] * 256 + byteAr[3]);

                ////para ter resultados em milimetros
                //string str = MotorPosAux.ToString();
                //try
                //{
                //  MotorPos = Convert.ToDouble(str.Substring(0, str.Length - 2) + "," + str.Substring(str.Length - 2));
                //}
                //catch (Exception) { }

                return true;
            }
            else
                return false;
        }

        public bool ReadPresentAlarmCode(ref int AlarmCode)
        {
            byte[] ar = new byte[255];
            if (ReadHoldingRegisters(PresentAlarmCodeQuery, 1, ar))
            {
                byte AlarmMSB_Received = HexToByte(Convert.ToChar(ar[7]).ToString() + Convert.ToChar(ar[8]).ToString());
                byte AlarmLSB_Received = HexToByte(Convert.ToChar(ar[9]).ToString() + Convert.ToChar(ar[10]).ToString());

                AlarmCode = AlarmMSB_Received * 256 + AlarmLSB_Received;
                return true;
            }
            else
                return false;
        }

        public bool ReadDeviceStatusQuery1(ref int Status)
        {
            byte[] ar = new byte[255];
            if (ReadHoldingRegisters(ZoneStatusQuery, 1, ar))
            {
                Status = ar[3] * 256 + ar[4];
                return true;
            }
            else
                return false;
        }

        public bool ReadSystemStatus(ref int Status)  // 2 registers (4 byte)
        {
            byte[] ar = new byte[255];
            if (ReadHoldingRegisters(SystemStatusQuery, 2, ar))
            {
                Status = Convert4ByteToInt(ar[3], ar[4], ar[5], ar[6]);
                return true;
            }
            else
                return false;
        }

        public bool ReadCurrentSpeed(ref int Speed)
        {
            byte[] ar = new byte[255];
            if (ReadHoldingRegisters(CurrentSpeedMonitor, 2, ar))
            {
                Speed = Convert4ByteToInt(ar[3], ar[4], ar[5], ar[6]);
                return true;
            }
            else
                return false;
        }

        public bool ReadCurrentAmpere(ref int Ampere)
        {
            byte[] ar = new byte[255];
            if (ReadHoldingRegisters(CurrentAmpereMonitor, 2, ar))
            {
                Ampere = Convert4ByteToInt(ar[3], ar[4], ar[5], ar[6]);
                return true;
            }
            else
                return false;
        }

        public bool ReadDeviation(ref int Dev)
        {
            byte[] ar = new byte[255];
            if (ReadHoldingRegisters(DeviationMonitor, 2, ar))
            {
                Dev = Convert4ByteToInt(ar[3], ar[4], ar[5], ar[6]);
                return true;
            }
            else
                return false;
        }

        public bool ReadSystemTimer(ref int msec)
        {
            byte[] ar = new byte[255];
            if (ReadHoldingRegisters(SystemTimerQuery, 2, ar))
            {
                msec = Convert4ByteToInt(ar[3], ar[4], ar[5], ar[6]);
                return true;
            }
            else
                return false;
        }

        public bool ReadZoneStatus(ref int ZStatus)
        {
            byte[] ar = new byte[255];
            if (ReadHoldingRegisters(ZoneStatusQuery, 1, ar))
            {
                ZStatus = ar[3] * 256 + ar[4];
                return true;
            }
            else
                return false;
        }

        public bool ReadRegisterBit(ushort BitAddress, ref bool OnOff)
        {
            byte[] ar = new byte[255];
            if (ReadInputStatus(BitAddress, 1, ar))
            {
                byte PosOkFlag = HexToByte(Convert.ToChar(ar[7]).ToString() + Convert.ToChar(ar[8]).ToString());

                if (PosOkFlag == 0)
                {
                    OnOff = false;
                    return true;
                }
                else if (PosOkFlag == 1)
                {
                    OnOff = true;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        #endregion

        #region MyFunctions

        public bool MoveAxisToPosition(int Target, ushort Inposband, uint Speed, ushort AccDec)
        {
            ushort Flag = 0;
            byte[] Ret = new byte[255];


            ushort Push = 0;

            if (!MoveActuator(Target, Inposband, Speed, AccDec, Push, Flag, Ret)) return false;

            return true;


        }

        public bool MoveAxisToForce(int Target, ushort Inposband, uint Speed, ushort AccDec, ushort Force, bool RV)
        {
            ushort Flag = 6;
            if (RV)
                Flag = 2;

            byte[] Ret = new byte[255];

            if (!MoveActuator(Target, Inposband, Speed, AccDec, Force, Flag, Ret)) return false;

            return true;


        }

        #endregion
    }
}
