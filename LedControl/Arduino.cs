using System.Collections.Generic;
using System.IO.Ports;

namespace LedControl
{
    class Arduino
    {
        public SerialPort Serial { get; set; }
        private List<string> commands = new List<string>() { };
        private bool loop = false;
        public Arduino ()
        {
        }

        public void Connect(string port)
        {
            Serial = new SerialPort(port);
            Serial.BaudRate = 9600;
            Serial.StopBits = StopBits.One;
            Serial.Parity = Parity.None;
            Serial.DataBits = 8;
            Serial.DtrEnable = true;
            Serial.Open();
        }

        public void Disconnect()
        {
            if (Serial != null)
            {
                this.loop = false;
                Serial.Close();
                Serial.Dispose();
                Serial = null;
            }
        }

        public void AddCommand(string command)
        {
            //this.commands.Add(command);
            Serial.Write(command);
        }

        public void ThreadLoop()
        {
            this.loop = true;
            while (this.loop)
            {
               /* if(this.commands.Count > 0)
                {
                    Serial.Write(this.commands[0]);
                    Serial.Write(this.commands[0]);
                    this.commands.RemoveAt(0);
                }*/
            }
        }
    }
}
