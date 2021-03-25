using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace HamburgerMenuApp
{
    class _cCylinder
    {
        // error Values: -1: No Error 
        //               1 : timeout on the movement
        //               2 : Fail to launch thread
        /////////////////////////////////////////////////////////////////////////
       
        public string Name;

        private  Thread Workthread;

        private bool InHomeSensor    =  false,
                     InWorkSensor    =  false,
                     InError         =  false,
                     Running         =  false,
                     SecurityOK      =  false;

        private int  ErrorValue      = -1,
                     ConfigType      =  0;

        private double Timeout       =  0;

        private short InHomeIdx      = -1,
                      InWorkIdx      = -1,
                      OutHomeIdx     = -1,
                      OutWorkIdx     = -1;

        private List<short> InHomeIdxArray,
                            InWorkIdxArray;


        ///<summary>
        /// Constructor of the cylinder information
        ///     homeInputIndex - index of the input for the sensor of home position for the cilinder
        ///     workInputIndex - index of the input for the sensor of Work position for the cilinder
        ///     homeOutputIndex - index of the Output for the movement to home position for the cilinder
        ///     workOutputIndex - index of the Output for the movement to Work position for the cilinder
        ///     timeout         - timeout for the execution of the movements
        ///</summary>
        public _cCylinder(short homeInputIndex, short workInputIndex, short homeOutputIndex, short workOutputIndex, double timeout)
        {
            InHomeIdx   = homeInputIndex;
            InWorkIdx   = workInputIndex;
            OutHomeIdx  = homeOutputIndex;
            OutWorkIdx  = workOutputIndex;
            Timeout     = timeout;
            ConfigType  = 1;
        }
        public _cCylinder(List<short> homeInputIndex, List<short> workInputIndex, short homeOutputIndex, short workOutputIndex, double timeout)
        {
            InHomeIdxArray  = new List<short>();
            InWorkIdxArray  = new List<short>();
            InHomeIdxArray  = homeInputIndex;
            InWorkIdxArray  = workInputIndex;
            OutHomeIdx      = homeOutputIndex;
            OutWorkIdx      = workOutputIndex;
            Timeout         = timeout;
            ConfigType      = 2;
        }
        // information on sensor state
        public bool IsHome()
        {
            return InHomeSensor;
        }

        public bool IsWork()
        {
            return InWorkSensor;
        }
        // information if the job is running
        public bool IsRunning()
        {
            return Running; 
        }
        // information if the job is in error
        public bool AsError()
        {
            return InError;
        }
        // get the error feedback
        public int GetError()
        {
            return ErrorValue;
        }

        //Function to move the cylinder home 
        public bool Move2Home()
        {
            if (!Running)
            {
                Workthread = new Thread(MoveHomeExecution);
                Workthread.Start();
                if (Workthread.IsAlive)
                {
                    Running = true;
                    InError = false;
                    ErrorValue = -1;
                }
                else
                {
                    Running = false;
                    ErrorValue = 2;
                    InError = true;
                }
            }
            return Running;
        }

        //Function to move the cylinder Work 
        public bool Move2Work()
        {
            if (!Running)
            {
                Workthread = new Thread(MoveWorkExecution);
                Workthread.Start();
                if (Workthread.IsAlive)
                {
                    Running = true;
                    InError = false;
                    ErrorValue = -1;
                }
                else
                {
                    Running = false;
                    ErrorValue = 2;
                    InError = true;
                }
            }
            return Running;
        }

        // thread to handle the Move home job
        private void MoveHomeExecution()
        {
            StartTimer();
            bool first = false;
            double Time = StepTime();
            while (Time<Timeout && !InHomeSensor)
            {
                switch (ConfigType)
                {
                    case 1:
                        InHomeSensor = _cGlobalVariables.DigitalInputs[InHomeIdx].State;
                        InWorkSensor = _cGlobalVariables.DigitalInputs[InWorkIdx].State;
                        break;
                    case 2:
                        int cnt_H = 0 , cnt_W=0;
                        foreach (short sensor in InHomeIdxArray)
                        {
                            if(_cGlobalVariables.DigitalInputs[sensor].State)
                            {
                                cnt_H++;
                            }
                        }
                        if (cnt_H == InHomeIdxArray.Count)
                            InHomeSensor = true;
                        else
                            InHomeSensor = false;
                        foreach (short sensor in InWorkIdxArray)
                        {
                            if (_cGlobalVariables.DigitalInputs[sensor].State)
                            {
                                cnt_W++;
                            }
                        }
                        if (cnt_W == InWorkIdxArray.Count)
                            InWorkSensor = true;
                        else
                            InWorkSensor = false;
                        break;
                }
                
                if(!InHomeSensor && !first)
                {
                    _cGlobalVariables.IoComunication.Write2ComunicationSingle(OutHomeIdx, true);
                    _cGlobalVariables.IoComunication.Write2ComunicationSingle(OutWorkIdx, false);
                    first = true;
                }
                Time = StepTime();
                Thread.Sleep(50);
            }
            if (!InHomeSensor)
            {
                InError         = true;
                ErrorValue      = 1;
            }
            else
            {
                InError         = false;
                ErrorValue      = -1;
            }
            Running = false;
            Workthread.Abort();
        }

        // thread to handle the Move Work job
        private void MoveWorkExecution()
        {
            StartTimer();
            bool first = false;
            double Time = StepTime();
            while (Time < Timeout && !InWorkSensor)
            {
                switch (ConfigType)
                {
                    case 1:
                        InHomeSensor = _cGlobalVariables.DigitalInputs[InHomeIdx].State;
                        InWorkSensor = _cGlobalVariables.DigitalInputs[InWorkIdx].State;
                        break;

                    case 2:
                        int cnt_H = 0, cnt_W = 0;
                        foreach (short sensor in InHomeIdxArray)
                        {
                            if (_cGlobalVariables.DigitalInputs[sensor].State)
                            {
                                cnt_H++;
                            }
                        }
                        if (cnt_H == InHomeIdxArray.Count)
                            InHomeSensor = true;
                        else
                            InHomeSensor = false;
                        foreach (short sensor in InWorkIdxArray)
                        {
                            if (_cGlobalVariables.DigitalInputs[sensor].State)
                            {
                                cnt_W++;
                            }
                        }
                        if (cnt_W == InWorkIdxArray.Count)
                            InWorkSensor = true;
                        else
                            InWorkSensor = false;
                        break;
                }
                if (!InWorkSensor && !first)
                {
                    _cGlobalVariables.IoComunication.Write2ComunicationSingle(OutHomeIdx, false);
                    _cGlobalVariables.IoComunication.Write2ComunicationSingle(OutWorkIdx, true);
                    first = true;
                }
                Time = StepTime();
                Thread.Sleep(50);
            }
            if (!InWorkSensor)
            {
                InError = true;
                ErrorValue = 1;
            }
            else
            {
                InError = false;
                ErrorValue = -1;
            }
            Running = false;
            Workthread.Abort();
        }

        // empty both chambers of cylinder
        public void EmptyChambers()
        {
            _cGlobalVariables.IoComunication.Write2ComunicationSingle(OutHomeIdx, false);
            _cGlobalVariables.IoComunication.Write2ComunicationSingle(OutWorkIdx, false);
        }

        // Fill both chambers of cylinders
        public void FillChambers()
        {
            _cGlobalVariables.IoComunication.Write2ComunicationSingle(OutHomeIdx, true);
            _cGlobalVariables.IoComunication.Write2ComunicationSingle(OutWorkIdx, true);
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
