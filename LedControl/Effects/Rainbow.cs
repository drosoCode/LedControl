using System.Threading;

namespace LedControl.Effects
{
    class Rainbow : Effect
    {
        private int sleep;

        public Rainbow(Arduino arduino, string color, int brightness, int sleep) : base(arduino, color, brightness)
        {
            this.sleep = sleep;
        }

        public override void ThreadLoop()
        {
            string devCommitList = "";
            foreach (int[] dev in this.devices)
            {
                devCommitList += dev[0] + ";!#";
            }

            while (true)
            {
                for (int i = 0; i < 1785; i += 20)
                {
                    foreach (int[] dev in this.devices)
                    {
                        this.arduino.AddCommand(dev[0] + ";0:" + dev[1] + ";" + getColor(i) + ";" + this.brightness + "#");
                    }
                    this.arduino.AddCommand(devCommitList);
                    Thread.Sleep(this.sleep);
                }
            }
        }

        private string getColor(int i)
        {
            int r = 0;
            int g = 0;
            int b = 0;
            if (i <= 255)
            {
                r = i;
                g = 0;
                b = 0;
            }
            else if (i > 255 && i <= 510)
            {
                r = 255;
                g = i - 255;
                b = 0;
            }
            else if (i > 510 && i <= 765)
            {
                r = 765 - i;
                g = 255;
                b = 0;
            }
            else if (i > 765 && i <= 1020)
            {
                r = 0;
                g = 255;
                b = i - 765;
            }
            else if (i > 1020 && i <= 1275)
            {
                r = 0;
                g = 1275 - i;
                b = 255;
            }
            else if (i > 1275 && i <= 1530)
            {
                r = i - 1275;
                g = 0;
                b = 255;
            }
            else if (i > 1530 && i <= 1785)
            {
                r = 255;
                g = 0;
                b = 1785 - i;
            }
            string boglo = r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
            return boglo;
        }
    }
}
