
namespace LedControl.Effects
{
    class Static : Effect
    {
        public Static(Arduino arduino, string color, int brightness) : base(arduino, color, brightness)
        {
        }
        public override void ThreadLoop()
        {
            string devCommitList = "";
            foreach (int[] dev in this.devices)
            {
                this.arduino.AddCommand(dev[0] + ";0:" + dev[1] + ";" + this.color + ";" + this.brightness + "#");
                devCommitList += dev[0] + ";!#";
            }
            this.arduino.AddCommand(devCommitList);
        }
    }
}
