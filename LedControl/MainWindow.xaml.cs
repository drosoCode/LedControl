using System;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;
using System.Windows.Media;
using System.Collections.Generic;
using System.Threading;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace LedControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow AppWindow;
        private Arduino arduino;
        private string primaryColor = "ff0000";
        private string secondaryColor = "ff0000";
        private List<Thread> threads = new List<Thread>() { };
        private Thread arduinoThread;

        private Dictionary<string, List<int>> devices = new Dictionary<string, List<int>>()
        {
            { "dev_fan1", new List<int>() { 5, 15, -1 } },
            { "dev_fan2", new List<int>() { 3, 15, -1 } },
            { "dev_fan3", new List<int>() { 7, 15, -1 } },
            { "dev_fan4", new List<int>() { 8, 15, -1 } },
            { "dev_fan5", new List<int>() { 9, 15, -1 } },
            { "dev_ram1", new List<int>() { 2, 15, -1 } },
            { "dev_ram2", new List<int>() { 6, 15, -1 } },
            { "dev_window", new List<int>() { 0, 80, -1 } },
            { "dev_all", new List<int>() { -1, 80, -1 } }
        };

        public MainWindow()
        {
            InitializeComponent();
            brightSlider.Value = 255;
            sleepSlider.Value = 1;
            this.arduino = new Arduino();
        }

        private void stopEffects()
        {
            foreach (Thread t in this.threads)
            {
                t.Abort();
            }
            foreach (KeyValuePair<string, List<int>> dev in this.devices)
            {
                this.devices[dev.Key][2] = -1;
            }
            this.threads.Clear();
        }

        private void BtnEnable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BtnEnable.IsChecked == true)
                {
                    comPorts.IsEnabled = false;
                    this.arduino.Connect((comPorts.Items[comPorts.SelectedIndex] as string));
                    //this.arduinoThread = new Thread(this.arduino.ThreadLoop);
                    //this.arduinoThread.Start();
                    BtnEnable.Content = "Disconnect";
                }
                else
                {
                    this.stopEffects();
                    if (this.arduinoThread != null)
                        this.arduinoThread.Abort();
                    comPorts.IsEnabled = true;
                    this.arduino.Disconnect();
                    BtnEnable.Content = "Connect";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void comPorts_DropDownOpened(object sender, EventArgs e)
        {
            comPorts.Items.Clear();
            var ports = SerialPort.GetPortNames();
            foreach (var port in ports) comPorts.Items.Add(port);
        }

        private string convertColor(string selected)
        {
            String color = "000000";
            switch (selected)
            {
                case "Custom":
                    color = CustomColor.SelectedColor.Value.ToString().Substring(3, 6);
                    break;
                case "Red":
                    color = "ff0000";
                    break;
                case "Orange":
                    color = "ff6600";
                    break;
                case "Yellow":
                    color = "ffff00";
                    break;
                case "Green":
                    color = "00ff00";
                    break;
                case "Blue":
                    color = "0000ff";
                    break;
                case "Cyan":
                    color = "00ffff";
                    break;
                case "Purple":
                    color = "8800ff";
                    break;
                case "White":
                    color = "ffffff";
                    break;
                case "All":
                    color = "all";
                    break;
            }
            return color;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SolidColorBrush colorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#000000"));
            if ((bool)PrimaryColorRadio.IsChecked)
            {
                this.primaryColor = this.convertColor(((ComboBoxItem)ComboColor.SelectedItem).Content.ToString());
                if (this.primaryColor != "all")
                    colorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#" + this.primaryColor));
                primaryBackground.Background = colorBrush;
            }
            else
            {
                this.secondaryColor = this.convertColor(((ComboBoxItem)ComboColor.SelectedItem).Content.ToString());
                if (this.secondaryColor != "all")
                    colorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#" + this.secondaryColor));
                secondaryBackground.Background = colorBrush;
            }

        }

        private void SetEffect_Click(object sender, RoutedEventArgs e)
        {
            String selected = ((ComboBoxItem)ComboMode.SelectedItem).Content.ToString();
            int brightness = (int)brightSlider.Value;
            int sleep = (int)Math.Round(sleepSlider.Value * 1000);

            Effects.Effect eff = null;
            switch (selected)
            {
                case "Music":
                    String dev = (DeviceBox.Items[DeviceBox.SelectedIndex] as string).Split(' ')[0];
                    eff = new Effects.Music(this.arduino, null, brightness, Convert.ToInt32(dev));
                    break;
                case "Static":
                    eff = new Effects.Static(this.arduino, this.primaryColor, brightness);
                    break;
                case "CPU Load":
                    eff = new Effects.Stats(this.arduino, null, brightness, sleep, "cpu");
                    break;
                case "GPU Load":
                    eff = new Effects.Stats(this.arduino, null, brightness, sleep, "gpuLoad");
                    break;
                case "GPU Temp":
                    eff = new Effects.Stats(this.arduino, null, brightness, sleep, "gpuTemp");
                    break;
                case "Rainbow":
                    eff = new Effects.Rainbow(this.arduino, null, brightness, sleep);
                    break;
                case "Breathe":
                    eff = new Effects.Breathe(this.arduino, this.primaryColor, brightness, sleep);
                    break;
                case "Blink":
                    eff = new Effects.Blink(this.arduino, this.primaryColor, brightness, sleep);
                    break;
                case "Ring":
                    eff = new Effects.Ring(this.arduino, this.primaryColor, brightness, sleep, this.secondaryColor);
                    break;
                case "OFF":
                    eff = new Effects.Static(this.arduino, "000000", 0);
                    break;
            }

            // add devices
            foreach (KeyValuePair<string, List<int>> dev in this.devices)
            {
                CheckBox cb = this.FindName(dev.Key) as CheckBox;
                if ((bool)cb.IsChecked)
                {
                    if (dev.Value[2] != -1 && dev.Value[2] < this.threads.Count)
                    {
                        if (this.threads[dev.Value[2]].IsAlive)
                            this.threads[dev.Value[2]].Abort();
                        this.threads.RemoveAt(dev.Value[2]);
                        dev.Value[2] = -1;
                    }
                    eff.addDevice(dev.Value[0], dev.Value[1]);
                }
            }

            if (selected != "OFF" && selected != "Static")
            {
                // create and add thread
                Thread t = new Thread(eff.ThreadLoop);
                this.threads.Add(t);
                int index = this.threads.Count - 1;
                // link devices to their thread
                foreach (KeyValuePair<string, List<int>> dev in this.devices)
                {
                    CheckBox cb = this.FindName(dev.Key) as CheckBox;
                    if ((bool)cb.IsChecked)
                    {
                        this.devices[dev.Key][2] = index;
                    }
                }
                // start the thread
                t.Start();
            }
            else
            {
                eff.ThreadLoop();
            }
        }

        private void brightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            brightnessLabel.Content = Math.Round(brightSlider.Value).ToString();
        }

        private void sleepSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sleepLabel.Content = Math.Round(sleepSlider.Value, 2).ToString();
        }

        private void DeviceBox_DropDownOpened(object sender, EventArgs e)
        {
            if (DeviceBox.Items.Count == 0)
            {
                bool result = false;
                for (int i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
                {
                    var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                    if (device.IsEnabled && device.IsLoopback)
                    {
                        DeviceBox.Items.Add(string.Format("{0} - {1}", i, device.name));
                    }
                }
                DeviceBox.SelectedIndex = 0;
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
                result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                if (!result) throw new Exception("Init Error");
            }
        }

        private void dev_all_Checked(object sender, RoutedEventArgs e)
        {
            foreach (KeyValuePair<string, List<int>> dev in this.devices)
            {
                if (dev.Key != "dev_all")
                {
                    CheckBox cb = this.FindName(dev.Key) as CheckBox;
                    cb.IsChecked = false;
                }
            }
        }
    }
}
