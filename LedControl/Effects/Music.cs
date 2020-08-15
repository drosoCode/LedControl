using System;
using Un4seen.Bass;
using Un4seen.BassWasapi;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace LedControl.Effects
{
    class Music : Effect
    {
        private int device;
        private int hanctr = 0;
        private bool initialized = false;
        private WASAPIPROC process;
        private float[] fft = new float[1024];
        private int lastLevel = 0;

        private List<int> windowLedLeft = new List<int>() { 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
            16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 };

        private List<int> windowLedRight = new List<int>() { 68, 67, 66, 65, 64, 63, 62, 61, 60, 59, 58, 57, 56, 55,
            54, 53, 52, 51, 50, 49, 48, 47, 46, 45, 44, 43, 42, 41, 
            40, 39, 38, 37, 36, 35, 34, 33, 32, 31, 30 };


        public Music(Arduino arduino, string color, int brightness, int device) : base(arduino, color, brightness)
        {
            this.device = device;
        }

        //flag for enabling and disabling program functionality
        private void Init()
        {
            if (!this.initialized)
            {
                this.process = new WASAPIPROC(Process);
                bool result = BassWasapi.BASS_WASAPI_Init(this.device, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, this.process, IntPtr.Zero);
                if (result)
                {
                    this.initialized = true;
                }
            }
            BassWasapi.BASS_WASAPI_Start();
        }

        public override void ThreadLoop()
        {
            this.Init();

            string devCommitList = "";
            foreach (int[] dev in this.devices)
            {
                devCommitList += dev[0] + ";!#";
            }

            while (true)
            {
                decimal value = this.getData();
                foreach (int[] dev in this.devices)
                {
                    if (dev[0] == 0)
                    {
                        // window
                        this.arduino.AddCommand(dev[0] + ";0:" + dev[1] + ";000000;" + this.brightness + "#");
                        int nbLed = dev[1] / 2;
                        int maxLed = Convert.ToInt32((decimal)nbLed / 100m * value);
                        int parts = Convert.ToInt32(nbLed / 3m);
                        if (maxLed < parts)
                        {
                            this.arduino.AddCommand(dev[0] + ";0:" + maxLed + ";00ff00;" + this.brightness + "#");
                            int b = 68 - maxLed;
                            this.arduino.AddCommand(dev[0] + ";" + b + ":68" + ";00ff00;" + this.brightness + "#");
                        }
                        else if (maxLed > parts)
                        {
                            this.arduino.AddCommand(dev[0] + ";55:80;00ff00;" + this.brightness + "#");
                            if (maxLed < parts * 2)
                            {
                                int a = maxLed - parts;
                                this.arduino.AddCommand(dev[0] + ";0:" + a + ";ff0000;" + this.brightness + "#");
                                int b = 54 - (maxLed - parts);
                                this.arduino.AddCommand(dev[0] + ";" + b + ":54" + ";ff0000;" + this.brightness + "#");
                            }
                            else
                            {
                                this.arduino.AddCommand(dev[0] + ";0:15;ffff00;" + this.brightness + "#");
                                this.arduino.AddCommand(dev[0] + ";41:54;ffff00;" + this.brightness + "#");

                                int a = 16 + (maxLed - 2 * parts);
                                if (a > 29)
                                    a = 29;
                                this.arduino.AddCommand(dev[0] + ";16:" + a + ";ff0000;" + this.brightness + "#");
                                int b = 40 - (maxLed - 2 * parts);
                                if (b < 30)
                                    b = 30;
                                this.arduino.AddCommand(dev[0] + ";" + b + ":40" + ";ff0000;" + this.brightness + "#");
                            }
                        }
                    }
                    else
                    {
                        this.arduino.AddCommand(dev[0] + ";0:" + dev[1] + ";000000;" + this.brightness + "#");
                        int maxLed = Convert.ToInt32((decimal)dev[1] / 100m * value);
                        int parts = Convert.ToInt32(dev[1] / 3m);
                        if (maxLed < parts)
                        {
                            this.arduino.AddCommand(dev[0] + ";0:" + maxLed + ";00ff00;" + this.brightness + "#");
                        }
                        else if (maxLed > parts)
                        {
                            this.arduino.AddCommand(dev[0] + ";0:" + parts + ";00ff00;" + this.brightness + "#");
                            if (maxLed < parts * 2)
                            {
                                this.arduino.AddCommand(dev[0] + ";" + parts + ":" + maxLed + ";ffff00;" + this.brightness + "#");
                            }
                            else
                            {
                                this.arduino.AddCommand(dev[0] + ";" + parts + ":" + parts * 2 + ";ffff00;" + this.brightness + "#");
                                this.arduino.AddCommand(dev[0] + ";" + parts * 2 + ":" + maxLed + ";ff0000;" + this.brightness + "#");
                            }
                        }
                    }
                }
                this.arduino.AddCommand(devCommitList);
                Thread.Sleep(25);
            }
        }

        private string percentToColor(float value)
        {
            string color;
            if (value < 30)
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

        private decimal getData()
        {
            int ret = BassWasapi.BASS_WASAPI_GetData(this.fft, (int)BASSData.BASS_DATA_FFT2048); //get channel fft data
            if (ret < -1) return 0m;

            Int32 level = BassWasapi.BASS_WASAPI_GetLevel();
            if (level == this.lastLevel && level != 0) this.hanctr++;

            float left = Utils.LowWord32(level);

            //Required, because some programs hang the output. If the output hangs for a 75ms
            //this piece of code re initializes the output so it doesn't make a gliched sound for long.
            if (this.hanctr > 3)
            {
                this.hanctr = 0;
                Free();
                Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                this.initialized = false;
                this.Init();
            }

            // convert to usable percentages
            decimal data = ((decimal)left / 10000 * 100);

            return data;
        }

        // WASAPI callback, required for continuous recording
        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }

        //cleanup
        public void Free()
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();
        }
    }
}
