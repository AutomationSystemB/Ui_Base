using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamburgerMenuApp
{
    class _cMachineState
    {

        private static bool HomeValid           = false, 
                            AirOn               = false, 
                            Emergency           = false,
                            SecurityBarrier     = false,
                            DoorsState          = false,
                            PlcConected         = false, 
                            AutoReady           = false, 
                            AutoModeActive      = false,
                            ManualModeActive    = false,
                            LoginOnStart        = false, 
                            UseDayCounter       = false, 
                            UseTotalCounter     = false,
                            UsePuttyPaste       = false, 
                            UseScrewDriver      = false, 
                            UseCim              = false;

        private static string StationName       = "";


        public bool GetHomeValid()
        {
            return HomeValid;
        }
        public void SetHomeValid(bool Value)
        {
            HomeValid = Value;
        }
        public bool GetAirState()
        {
            return AirOn;
        }
        public void SetAirState(bool Value)
        {
            AirOn = Value;
        }
        public bool GetEmergencyState()
        {
            return Emergency;
        }
        public void SetEmergencyState(bool Value)
        {
            Emergency = Value;
        }
        public bool GetSecurityBarrierState()
        {
            return SecurityBarrier;
        }
        public void SetSecurityBarrierState(bool Value)
        {
            SecurityBarrier = Value;
        }

        public bool GetDoorsState()
        {
            return DoorsState;
        }
        public void SetDoorsState(bool Value)
        {
            DoorsState= Value;
        }
        public string GetStationName()
        {
            return StationName;
        }
        public void SetStationName(string value)
        {
             StationName=value;
        }
        public bool GetPlcConnectionState()
        {
            return PlcConected;
        }
        public void SetPlcConnectionState(bool Value)
        {
            PlcConected = Value;
        }
        public bool GetLoginOnStart()
        {
            return LoginOnStart;
        }
        public void SetLoginOnStart(bool value)
        {
            LoginOnStart = value;
        }
        public bool GetUseDayCounter()
        {
            return UseDayCounter;
        }
        public void SetUseDayCounter(bool value)
        {
            UseDayCounter = value;
        }
        public bool GetUseTotalCounter()
        {
            return UseTotalCounter;
        }
        public void SetUseTotalCounter(bool value)
        {
            UseTotalCounter = value;
        }
        public bool GetUsePuttyPaste()
        {
            return UsePuttyPaste;
        }
        public void SetUsePuttyPaste(bool value)
        {
            UsePuttyPaste = value;
        }
        public bool GetUseScrewDriver()
        {
            return UseScrewDriver;
        }
        public void SetUseScrewDriver(bool value)
        {
            UseScrewDriver = value;
        }
        public bool GetUseCim()
        {
            return UseCim;
        }
        public void SetUseCim(bool value)
        {
            UseCim = value;
        }

        public bool CheckAutoModeReady()
        {
            return AutoReady;
        }
        public void SetAutoModeReady(bool Value)
        {
            AutoReady = Value;
        }
        public bool CheckMachineInAuto()
        {
            return AutoModeActive;
        }
        public void SetMachineInAuto(bool Value)
        {
            AutoModeActive = Value;
        }
        public bool CheckMachineInManual()
        {
            return ManualModeActive;
        }
        public void SetMachineInManual(bool Value)
        {
            ManualModeActive = Value;
        }
    }
}
