using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CMMTt111;

namespace HamburgerMenuApp
{
    class _cAxis

    {
        // error Values: -1: No Error 
        //               1 : timeout on the movement
        //               2 : Fail to launch thread
        //               3 : Falha a efetuar o Enable
        //               4 : Comunicação perdida
        //               5 : em falha
        //               6 : homing timeout
        /////////////////////////////////////////////////////////////////////////

        public enum AxisState
        {
            Idle,
            Moving,
            Error,
            Stop,
            InPosition,
            ExecutingHoming,
            LimitReached,

        }

        private Thread              Workthread, FestoThread;
        private festoCMMT           AxisCmmt;
        private _cIAIModbusASCII    AxisIai;

        public AxisState State;

        private int     IaiBaudrate     = 0,
                        AxisType        = 0,
                        MaxLimit        = 0,
                        MinLimit        = 0,
                        ErrorValue      = -1,
                        CycleTime       = 20,
                        TimeOut         = 500,
                        HomingTimeOut   = 1000,
                        PositionFactor  = 6,
                        VelocityFactor  = 3;


        private float   ActualPos   = 0,
                        ActualVel   = 0;

        private byte    IaiId       = 0,
                        CmmtIW      = 0,
                        CmmtOW      = 0,
                        cmd         = 0,
                        mode        = 0;

        private string  IaiPort     = "",
                        AxisName    = "",
                        CmmtIp      = "";

        private bool    Home        = false,
                        Connected   = false,
                        Enable      = false,
                        Running     = false,
                        StopThread  = false;
                       

        private void CheckIshomeValid()
        {
            switch (AxisType)
            {
                case (1):
                    Home = AxisCmmt.HomingActive;
                    break;
                case (2):
                    AxisIai.ReadRegisterBit(AxisIai.HomingCompletationStatus, ref Home);
                    break;
                default:
                    break;
            }
        }

        private void CheckIsEnabled()
        {
            switch (AxisType)
            {
                case (1):
                    Enable = AxisCmmt.EnableOperation;
                    break;
                case (2):
                    AxisIai.ReadRegisterBit(AxisIai.ServoOnStatus, ref Enable);
                    break;
                default:
                    break;
            }
        }

        private void CheckIsConnected()
        {
            switch (AxisType)
            {
                case (1):
                    Connected = AxisCmmt.Connected;
                    break;
                default:
                    break;
            }
        }

        public void CMD(byte c ,byte m)
        {
            cmd = c;
            mode = m;
        }

        public void FestoStart()
        {
            if (!Running)
            {
                FestoThread = new Thread(() => Festo());
                FestoThread.Start();
                if (FestoThread.IsAlive)
                {
                    Running = true;
                    State = AxisState.Stop;
                    ErrorValue = -1;
                }
                else
                {
                    Running = false;
                    ErrorValue = 2;
                    State = AxisState.Error;
                }
            }
        }

        private void Festo()
        {
            while (!StopThread)
            {
                Thread.Sleep(100);
                if (AxisCmmt.Connected)
                {
                    AxisCmmt.Override = 100.0F;
                    AxisCmmt.CmmtControl(mode, cmd); 
                                                      
                    cmd = 0;

                    switch (AxisCmmt.iCmmtStatus)
                    {
                        case 2:
                            ErrorValue = 5;
                            State = AxisState.Error;
                            break;
                        case 1401:
                            ErrorValue = 3;
                            State = AxisState.Error;
                            break;
                        case 11201:
                            ErrorValue = 6;
                            State = AxisState.Error;
                            break;
                        case 11202:
                            ErrorValue = -1;
                            State = AxisState.InPosition;
                            break;
                        case 1102:
                            ErrorValue = -1;
                            State = AxisState.ExecutingHoming;
                            break;
                        case 15303:
                            ErrorValue = -1;
                            State = AxisState.Moving;
                            break;
                        case 40301:
                            ErrorValue = 5;
                            State = AxisState.Idle;
                            break;
                    }
                }
            }
        }

        public bool IsConnected()
        {
            CheckIsConnected();
            return Connected;
        }

        public bool InHome()
        {
            CheckIshomeValid();
            return Home;
        }

        public string GetActError()
        {
            string s = "";
            switch (AxisType)
            { 
                case (1):
                    s =  AxisCmmt.iCmmtStatus.ToString();
                    break;
                case (2):
                    int valor = 0;
                    AxisIai.ReadCurrentPosition(ref valor);
                    ActualPos = (float)valor;
                    break;
            }
            return s;
        }

        public bool IsEnable()
        {
            CheckIsEnabled();
            return Enable;
        }

        public void SetTimeout(int Time)
        {
            TimeOut = Time;
        }

        public int GetAlarmCode()
        {
            int alarm = 0;

            switch (AxisType)
            {
                case 1:
                    alarm = AxisCmmt.iCmmtStatus;
                    break;
                case 2:
                    AxisIai.ReadPresentAlarmCode(ref alarm);
                    break;
            }

            return alarm;
        }

        public float GetActualPos()
        {
            switch (AxisType)
            {
                case (1):
                    ActualPos = AxisCmmt.ActualPosition;
                    break;
                case (2):
                    int valor = 0;
                    AxisIai.ReadCurrentPosition(ref valor);
                    ActualPos = (float)valor;
                    break;
            }
            return ActualPos;
        }

        public float GetActualVel()
        {
            switch (AxisType)
            {
                case (1):
                    ActualVel = AxisCmmt.ActualVelocity;
                    break;
                case (2):
                    int valor = 0;
                    AxisIai.ReadCurrentSpeed(ref valor);
                    ActualVel = (float)valor;
                    break;
            }
            return ActualVel;
        }

        public void SetBaudrate(int baud)
        {
            IaiBaudrate = baud;
        }

        public void SetIAIId(byte id)
        {
            IaiId = id;
        }

        public void SetAxisName(string Name)
        {
            AxisName = Name;
        }

        #region FestoCmmt Modbus
        // in case Festo Modbus possibility to change
        //cycle time  
        public void SetCycleTime(int cycle)
        {
            if (AxisType == 1)
                CycleTime = cycle;
        }
        // Homing Timeout
        public void SetHomingTimeout(int Time)
        {
            if (AxisType == 1)
                HomingTimeOut = Time;
        }
        // fill extended IW
        public void SetIW(byte InWord)
        {
            if (AxisType == 1)
                CmmtIW = InWord;
        }
        // fill extended OW
        public void SetOW(byte OutWord)
        {
            if (AxisType == 1)
                CmmtIW = OutWord;
        }
        #endregion
        
        //create axis type
        public _cAxis(int type)
        {
            AxisType = type;
            switch (AxisType)
            {
                case (1):
                    AxisCmmt = new festoCMMT();
                    break;
                case (2):
                    AxisIai = new _cIAIModbusASCII();
                    break;
                default:
                    break;
            }
        }

        //connect axis type
        /// <summary>
        /// In case of Modbus Festo type 1 Axis:
        /// pls take note the default values are 
        ///  CycleTime       = 20        
        ///  ModBusTimeOut   = 500
        ///  HomingTimeOut   = 1000
        ///  PositionFactor  = -6
        ///  VelocityFactor  = -3
        ///   IW             = 8
        ///   OW             = 4
        ///   IP   no default value you must fill it before connect.
        ///  In case this values doesn't match your configuration, you must change them before calling the connection function.
        ///  In case Of IAI comPort connection Type 2 axis:
        ///  IaiPort 
        ///  IaiId 
        ///  IaiBaudrate 
        ///  doesn't have defined default values you must fill them before connection.
        /// </summary>
        /// <param name="limitMax"> maximum reach of the axis</param>
        /// <param name="limitMin"> minimum reach of the axis</param>
        public void connect(string comms,int limitMax,int limitMin, int Timeout)
        {
            MaxLimit    = limitMax;
            MinLimit    = limitMin;
            TimeOut     = Timeout;
            switch (AxisType)
            {
                case (1):
                    CmmtIp                          = comms;
                    AxisCmmt.Timeout                = 0;
                    AxisCmmt.Homingtimeout          = HomingTimeOut;
                    AxisCmmt.Timewait               = 3;
                    AxisCmmt.CycleTime              = CycleTime;
                    AxisCmmt.ModbusTimeOut          = TimeOut;
                    AxisCmmt.ConvertPositionInput   = 1 * (10 ^ PositionFactor);
                    AxisCmmt.ConvertVelocityInput   = 1 * (10 ^ VelocityFactor);
                    AxisCmmt.ConvertPositionOutput  = 1 * (10 ^ PositionFactor);
                    AxisCmmt.ConvertVelocityOutput  = 1 * (10 ^ VelocityFactor);
                    AxisCmmt.Connect(CmmtIp,CmmtIW,CmmtOW) ;
                    if(AxisCmmt.Connected)
                    {
                        Connected                   = AxisCmmt.Connected;
                        AxisCmmt.PLCMasterControl   = true;
                        AxisCmmt.EnableOperation    = true;
                        AxisCmmt.FastStop           = true;
                        AxisCmmt.DriveCoast         = true;
                        FestoStart();
                    }
                    break;
                case (2):
                    IaiPort     = comms;
                    AxisIai.FillConnectionInfo(IaiPort, IaiId, IaiBaudrate);
                    Connected   = AxisIai.OpenComPort();
                    break;
                default:
                    break;
            }
        }

        public void Disconnect()
        {
            switch (AxisType)
            {
                case (1):
                   
                    AxisCmmt.Disconnect();
                    StopThread  = true;
                    if (!AxisCmmt.Connected && !FestoThread.IsAlive)
                    {
                        Connected                   = AxisCmmt.Connected;
                        AxisCmmt.PLCMasterControl   = false;
                        AxisCmmt.EnableOperation    = false;
                        AxisCmmt.FastStop           = false;
                        AxisCmmt.DriveCoast         = false;
                        StopThread                  = false;
                    }
                    break;
                case (2):

                    AxisIai.CloseComPort();
                    Connected = false;
                    break;
                default:
                    break;
            }
        }

        public void ResetAxis()
        {
            switch (AxisType)
            {
                case (1):

                    mode    = 0;
                    cmd     = 4;
                   
                    break;
                case (2):
                    AxisIai.AlarmReset();
                    int Alarm=0;
                    if (AxisIai.ReadPresentAlarmCode(ref Alarm))
                    {
                        if (Alarm != 0)
                        {
                            ErrorValue  = 5;
                            State       = AxisState.Error;
                        }
                        else
                            State       = AxisState.Stop;
                    }
                    else
                    {
                        State = AxisState.Error;
                        ErrorValue      = 4;
                        Connected       = false;
                    }

                    break;
                default:
                    break;
            }
        }

        public bool JogPlus()
        {
            switch (AxisType)
            {
                case (1):
                    CheckIsEnabled();
                    if (Enable)
                    {
                        AxisCmmt.Jogging1   = true;
                        AxisCmmt.Jogging2   = false;
                        Running      = true;

                    }
                    else
                        Running             = false;
                    
                   
                    break;
                case (2):
                    if (AxisIai.JogPlus(true))
                        Running = true;
                    else
                        Running = false;
                    break;
            }
            return Running;
        }

        public bool JogMinus()
        {
            switch (AxisType)
            {
                case (1):

                    CheckIsEnabled();
                    if (Enable)
                    {
                        AxisCmmt.Jogging1 = false;
                        AxisCmmt.Jogging2 = true;
                        Running = true;
                    }
                    else
                        Running = false;

                    break;
                case (2):
                    if (AxisIai.JogMinus(true))
                        Running = true;
                    else
                        Running = false;
                    break;
            }
            return Running;
        }

        public void JogStop()
        {
            switch (AxisType)
            {
                case (1):
                    AxisCmmt.Jogging1 = false;
                    AxisCmmt.Jogging2 = false;
                    break;
                case (2):
                    AxisIai.JogPlus(false);
                    AxisIai.JogMinus(false);
                    break;
            }
        }

        public bool Move2Position(float pos, float vel, float acel)
        {
            if (!Running)
            {
                Workthread = new Thread(() => MoveWorkExecution(pos,vel,acel));
                Workthread.Start();
                if (Workthread.IsAlive)
                {
                    Running = true;
                    State = AxisState.Stop;
                    ErrorValue = -1;
                }
                else
                {
                    Running = false;
                    ErrorValue = 2;
                    State = AxisState.Error;
                }
            }   
            return Running;
        }

        private void MoveWorkExecution(float Pos, float Vel, float AcelDecl)
        {
            switch (AxisType)
            {
                case (1):
                  
                    AxisCmmt.Acceleration   = AcelDecl;
                    AxisCmmt.Deceleration   = AcelDecl;
                    AxisCmmt.TargetPosition = Pos;
                    AxisCmmt.TargetVelocity = Vel;
                    mode                    = 2;
                    cmd                     = 1;
                    break;
                case (2):
                    AxisIai.MoveAxisToPosition((int)Pos,10,(uint)Vel,(ushort)AcelDecl);
                    bool InPosition= false;
                    StartTimer();
                    double Time = StepTime();
                    while (!InPosition && Time < TimeOut)
                    {
                        AxisIai.ReadRegisterBit(AxisIai.PositioningCompletationStatus, ref InPosition);
                        State = AxisState.Moving;
                        Thread.Sleep(50);
                        Time = StepTime();
                    }
                    if(InPosition)
                    {
                        State       = AxisState.InPosition;
                        ErrorValue  = -1;
                    }
                    else
                    {
                        State       = AxisState.Error;
                        ErrorValue  = 1;
                    }
                    break;
            }
            Workthread.Abort();
        }

        public bool Homing()
        {
            if (!Running)
            {
                Workthread = new Thread(() => MoveHome());
                Workthread.Start();
                if (Workthread.IsAlive)
                {
                    Running = true;
                    State = AxisState.Stop;
                    ErrorValue = -1;
                }
                else
                {
                    Running = false;
                    ErrorValue = 2;
                    State = AxisState.Error;
                }
            }
            return Running;
        }

        private void MoveHome()
        {
            switch (AxisType)
            {
                case (1):
                    cmd     = 1;
                    mode    = 1;
                    break;
                case (2):
                    AxisIai.Homing();
                    bool InPosition = false;
                    StartTimer();
                    double Time = StepTime();
                    while (!InPosition && Time < TimeOut)
                    {
                        AxisIai.ReadRegisterBit(AxisIai.HomingCompletationStatus, ref InPosition);
                        State = AxisState.ExecutingHoming;
                        Thread.Sleep(50);
                        Time = StepTime();
                    }
                    if (InPosition)
                    {
                        State = AxisState.InPosition;
                        ErrorValue = -1;
                    }
                    else
                    {
                        State = AxisState.Error;
                        ErrorValue = 1;
                    }
                    break;
            }
            Workthread.Abort();
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
