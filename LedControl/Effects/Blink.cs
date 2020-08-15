using System.Threading;
using System.Collections.Generic;

namespace LedControl.Effects
{
    class Blink : Effect
    {
        private int sleep;
        private List<string> colors = new List<string>()
        {
            "ff0000", "ff6600", "ffff00", "00ff00", "0000ff", "00ffff", "8800ff"
        };

        public Blink(Arduino arduino, string color, int brightness, int sleep) : base(arduino, color, brightness)
        {
            this.sleep = sleep;
        }
        public override void ThreadLoop ()
        {
            int i = 0;
            bool fullMode = false;
            if (this.color == "all")
            {
                fullMode = true;
            }

            string devCommitList = "";
            foreach (int[] dev in this.devices)
            {
                devCommitList += dev[0] + ";!#";
            }

            while (true)
            {
                if (fullMode)
                {
                    if (i >= this.colors.Count)
                        i = 0;
                    this.color = colors[i];
                    i++;
                }

                foreach (int[] dev in this.devices)
                {
                    this.arduino.AddCommand(dev[0] + ";0:" + dev[1] + ";" + this.color + ";" + this.brightness + "#");
                }
                this.arduino.AddCommand(devCommitList);
                Thread.Sleep(this.sleep);
                foreach (int[] dev in this.devices)
                {
                    this.arduino.AddCommand(dev[0] + ";0:" + dev[1] + ";000000;0#");
                }
                this.arduino.AddCommand(devCommitList);
                Thread.Sleep(this.sleep);
            }
        }
    }
}
