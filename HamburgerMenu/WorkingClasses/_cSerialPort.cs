using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace HamburgerMenuApp
{
    
    class ComPort
    {
        private SerialPort ScanPort;
        private string Message;
        private bool DataInputOnPort { get; set; }
        private bool MessageReceived { get; set; }

        public enum Handler
        {
            LineRead,
            FullRead,
            Char
        }

        public bool CheckDataInputState()
        {
            return DataInputOnPort;
        }

        public ComPort(string port,int baud, Parity parity, StopBits stopBits, int Databits, Handshake handshake,Handler handler)
        {
            ScanPort = new SerialPort(port);

            ScanPort.BaudRate = baud;
            ScanPort.Parity = parity;
            ScanPort.StopBits = stopBits;
            ScanPort.DataBits = Databits;
            ScanPort.Handshake = handshake;

            switch (handler)
            {
                case Handler.FullRead:
                    ScanPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandlerExist);
                    break;
                case Handler.LineRead:
                    ScanPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandlerLine);
                    break;
                case Handler.Char:
                    ScanPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandlerChar);
                    break;
            }
        }

        public bool GetMessageStatus()
        {
            return MessageReceived;
        }

        public bool StartListenPort()
        {
            if (!ScanPort.IsOpen)
            {
                try
                {
                    ScanPort.Open();
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show( "'Open Comunication with Serial Port': " + e.ToString() + "Failed \r\n\r\n");
                    return false;
                }
                
            }
            else
                return false;
            //     throw new Exception("Port already open or not connected....");
        }

        public void CloseListenPort()
        {
            if (ScanPort.IsOpen)
            {
                try
                {
                    ScanPort.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show("'Closing Comunication with Serial Port': " + e.ToString() + "Failed \r\n\r\n");
                }
            }
        }

        public string GetData()
        {
            MessageReceived = false;
            return Message;
        }

        private void DataReceivedHandlerChar(
                           object sender,
                           SerialDataReceivedEventArgs e)
        {
            DataInputOnPort = true;
        }
        private void DataReceivedHandlerLine(
                           object sender,
                           SerialDataReceivedEventArgs e)
        {
           
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            callmyfunction(indata);
          
            
        }
        private  void DataReceivedHandlerExist(
                           object sender,
                           SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            callmyfunction(indata);
        }

      
        private void callmyfunction(string data)
        {
            MessageReceived=true;
            Message = data;
        }

        public void SendData(string data)
        {
            ScanPort.Write(data);
        }
        
        public string ReadComPortDirectly()
        {
            string data = ScanPort.ReadExisting();
            return data;
        }
        public void ResetDataInputStatus()
        {
            DataInputOnPort = false;
        }
    }
}