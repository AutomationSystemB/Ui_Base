using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;

namespace HamburgerMenuApp
{
    class _cUpdateIO
    {
        private static Thread   UpdateStatus;
        private static bool     stop       = false,
                                running    = false;
        private _cMachineState  UpdateMachineState = new _cMachineState();

        public void StopThread()
        {
            stop = true;
        }

        public void StartThread()
        {
            UpdateStatus = new Thread(ThreadRun);
            UpdateStatus.IsBackground = true;
            UpdateStatus.Start();
        }
        private void ThreadRun()
        {
            stop = false;
            running = true;
            while (!stop)
            {
                _cGlobalVariables.IoComunication.ReadIO();
                UpdateMachineState.SetEmergencyState    (_cGlobalVariables.DigitalInputs[0].State);
                UpdateMachineState.SetAirState          (_cGlobalVariables.DigitalInputs[1].State);
                //check barrier status if no barrier then always true
                if (true) //
                    UpdateMachineState.SetSecurityBarrierState(true);
                else
                    UpdateMachineState.SetSecurityBarrierState(false);
                //check Door status if no door always true
                if (    _cGlobalVariables.DigitalInputs[2].State    &&
                        _cGlobalVariables.DigitalInputs[3].State    &&
                        _cGlobalVariables.DigitalInputs[4].State    &&
                        _cGlobalVariables.DigitalInputs[5].State    )

                    UpdateMachineState.SetDoorsState(true);
                else
                    UpdateMachineState.SetDoorsState(false);

                Thread.Sleep(50);
            }
            running = false;
        }
        public bool GetState()
        {
            return running;
        }
    }
}
