using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Net.Sockets;
using System.Net;

namespace HamburgerMenuApp
{
    class _cScanner
    {
        public enum ScannerState
        {
            Error,
            Connected,
            Disconnected,
            Reading,
            Ready,
            Done
        }

        private Thread Workthread;

        private string  Name,
                        Trigger,
                        DataRead,
                        ComPort,
                        IP,
                        Port;

        private int     Type,
                        Timeout,
                        Databits,
                        Baudrate;

        private IPEndPoint IPEndPoint;

        private Socket Socket;

        private ComPort ScannerObj;

        public ScannerState State      = ScannerState.Disconnected;

        private Parity Parity;

        private StopBits StopBits;

        private Handshake Handshake = Handshake.None;

        private ComPort.Handler Handler;

        public void SetStopBits(StopBits sbits)
        {
            StopBits = sbits;
        }

        public void SetParity(Parity par)
        {
            Parity = par;
        }

        public void SetHandshake( Handshake hs)
        {
            Handshake = hs;
        }

        public void SetHandler(ComPort.Handler hd)
        {
            Handler = hd;
        }

        public void SetIP(string ip)
        {
            IP = ip;
        }

        public void SetComPort(string cport)
        {
            ComPort = cport;
        }

        public void SetPort(string port)
        {
            Port = port;
        }

        public void SetBaudRate(int baud)
        {
            Baudrate = baud;
        }

        public void SetDataBits(int dbits)
        {
            Databits = dbits;
        }

        public void SetTrigger(string trg)
        {
            Trigger = trg;
        }

        public void SetName(string name)
        {
            Name = name;
        }

        public void SetTimeout(int t)
        {
            Timeout = t;
        }

        public void SetType(int type)
        {
            Type = type;
        }

        public _cScanner()
        {
            
        }

        public void Connect()
        {
            switch (Type)
            {
                case 1:
                    ScannerObj = new ComPort(ComPort, Baudrate, Parity, StopBits, Databits, Handshake.None, Handler);
                    if (ScannerObj.StartListenPort())
                        State = ScannerState.Connected;
                    else
                        State = ScannerState.Error;
                    break;
                case 2:

                    break;
                default:
                    break;
            }
        }
        public void SendTrigger()
        {
            switch (Type)
            {
                case 1:
                    if (Workthread == null)
                    {
                        ScannerObj.SendData(Trigger);
                        Workthread = new Thread(WaitComResponse);
                        Workthread.Start();
                    }
                    else if (!Workthread.IsAlive)
                    {
                        ScannerObj.SendData(Trigger);
                        Workthread = new Thread(WaitComResponse);
                        Workthread.Start();
                    }
                    else
                        State = ScannerState.Error;
                    break;
                case 2:

                    break;
                default:
                    break;
            }
        }

        private void WaitComResponse()
        {
            StartTimer();
            State = ScannerState.Reading;
            while (!ScannerObj.GetMessageStatus() && StepTime() < Timeout)
            {
                Thread.Sleep(50);
            }
            if (StepTime() < Timeout)
            {
                DataRead = ScannerObj.GetData();
                State = ScannerState.Done;
            }else
            {
                State = ScannerState.Error;
            }
            Workthread.Abort();
        }

        public string GetData()
        {
            string dat  = DataRead;
            DataRead    = "";
            State = ScannerState.Ready;
            return dat;
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
