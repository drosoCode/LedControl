using System.Collections.Generic;

namespace LedControl.Effects
{
    abstract class Effect
    {
        protected string color;
        protected int brightness;
        protected List<int[]> devices = new List<int[]>() { };
        protected Arduino arduino;
        public Effect(Arduino arduino, string color, int brightness)
        {
            this.arduino = arduino;
            this.color = color;
            this.brightness = brightness;
        }

        public void addDevice(int id, int nbLeds)
        {
            int[] dev = { id, nbLeds };
            this.devices.Add(dev);
        }

        public abstract void ThreadLoop();
    }
}
