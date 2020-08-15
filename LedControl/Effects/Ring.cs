using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace LedControl.Effects
{    class Ring : Effect
    {
        private int sleep;
        private string color2;
        private List<List<int>> devData = new List<List<int>>() {};

        public Ring(Arduino arduino, string color, int brightness, int sleep, string color2) : base(arduino, color, brightness)
        {
            this.sleep = sleep;
            this.color2 = color2;
        }
        public override void ThreadLoop()
        {
            string devCommitList = "";
            foreach (int[] dev in this.devices)
            {
                devCommitList += dev[0] + ";!#";
                int num = (int)Math.Round(dev[1] * 0.05);
                if (num < 1)
                    num = 1;
                devData.Add(new List<int>() { 0, num });
            }

            while (true)
            {
                for (int i = 0; i < this.devices.Count; i++)
                {
                    this.arduino.AddCommand(this.devices[i][0] + ";0:" + this.devices[i][1] + ";" + this.color + ";" + this.brightness + "#");

                    if ((devData[i][0]+devData[i][1]) > this.devices[i][1])
                        devData[i][0] = 0;

                    this.arduino.AddCommand(this.devices[i][0] + ";" + devData[i][0] + ":" + (devData[i][0]+ devData[i][1]) + ";" + this.color2 + ";" + this.brightness + "#");
                    devData[i][0] += devData[i][1];
                }
                this.arduino.AddCommand(devCommitList);
                Thread.Sleep(this.sleep);
            }
        }
    }
}
