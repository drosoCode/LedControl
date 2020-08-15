using System;
using System.Threading;
using System.Diagnostics;

namespace LedControl.Effects
{
    class Stats : Effect
    {
        private string type;
        private int sleep;
        public Stats(Arduino arduino, string color, int brightness, int sleep, string devType) : base(arduino, color, brightness)
        {
            this.type = devType;
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
                foreach (int[] dev in this.devices)
                {
                    this.arduino.AddCommand(dev[0] + ";0:" + dev[1] + ";" + this.getColor() + ";" + this.brightness + "#");
                }
                this.arduino.AddCommand(devCommitList);
                Thread.Sleep(this.sleep);
            }
        }

        private string getColor()
        {
            if (this.type == "cpu")
            {
                PerformanceCounter cpu = new PerformanceCounter();
                cpu.CategoryName = "Processor";
                cpu.CounterName = "% Processor Time";
                cpu.InstanceName = "_Total";
                cpu.NextValue();
                Thread.Sleep(200);
                return this.percentToColor((int)cpu.NextValue());
            }
            else if (this.type == "gpuLoad")
            {
                // GPU (nvidia-smi)
                Process pProcess = new Process();
                pProcess.StartInfo.FileName = "C:\\Program Files\\NVIDIA Corporation\\NVSMI\\nvidia-smi.exe";
                pProcess.StartInfo.Arguments = "--query-gpu=utilization.gpu --format=csv,noheader,nounits";
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.CreateNoWindow = true;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.Start();
                string strOutput = pProcess.StandardOutput.ReadToEnd();
                pProcess.WaitForExit();

                return this.percentToColor(Int16.Parse(strOutput));
            }
            else if (this.type == "gpuTemp")
            {
                // GPU (nvidia-smi)
                Process pProcess = new Process();
                pProcess.StartInfo.FileName = "C:\\Program Files\\NVIDIA Corporation\\NVSMI\\nvidia-smi.exe";
                pProcess.StartInfo.Arguments = "--query-gpu=temperature.gpu --format=csv,noheader,nounits";
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.CreateNoWindow = true;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.Start();
                string strOutput = pProcess.StandardOutput.ReadToEnd();
                pProcess.WaitForExit();

                return this.percentToColor((int)((Int16.Parse(strOutput) - 40) / 40 * 100));
            }
            else
            {
                return "000000";
            }
        }

        private string percentToColor(int value)
        {
            string color;
            if (value < 10)
                color = "0000ff";
            else if (value >= 10 && value < 20)
                color = "00b1fc";
            else if (value >= 20 && value < 30)
                color = "035706";
            else if (value >= 30 && value < 40)
                color = "00ff00";
            else if (value >= 40 && value < 50)
                color = "91ff00";
            else if (value >= 50 && value < 60)
                color = "ffff00";
            else if (value >= 60 && value < 70)
                color = "ffaa00";
            else if (value >= 70 && value < 80)
                color = "ff6600";
            else if (value >= 80 && value < 90)
                color = "ff2a00";
            else
                color = "ff0000";
            return color;
        }
    }
}
