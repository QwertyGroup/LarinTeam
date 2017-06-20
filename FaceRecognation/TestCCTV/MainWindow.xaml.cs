using FaceRecognition.Core;

using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using static TestCCTV.UILinking;

namespace TestCCTV
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _dt = new DispatcherTimer();
        private CameraSettings _currentCamera;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _dt.Tick += (o, a) => Timer_Tick();
                SetDelay(SliderDelayConverter(5000));
                _currentCamera = cbIp.SelectedValue as CameraSettings;
            };
        }

        private async void Timer_Tick()
        {
            var req = (HttpWebRequest)WebRequest.Create(_currentCamera.GetImageUrl());
            var resp = await req.GetResponseAsync();
            var stream = resp.GetResponseStream();
            var img = System.Drawing.Image.FromStream(stream);
            imgCameraStream.Source = ImageProcessing.ImageProcessingInstance.ConvertImageToBitmapImage(img);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            _dt.Start();
            cmdStart.IsEnabled = false;
            cmdStop.IsEnabled = true;
            slDelay.IsEnabled = false;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _dt.Stop();
            cmdStart.IsEnabled = true;
            cmdStop.IsEnabled = false;
            slDelay.IsEnabled = true;
        }

        private void SlDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
                SetDelay(SliderDelayConverter(e.NewValue));
        }

        private void SetDelay((double Delay, string Tip) cr)
        {
            _dt.Interval = TimeSpan.FromMilliseconds(cr.Delay);
            tbDelay.Text = cr.Tip;
        }

        private List<double> _delayStates = new List<double>
        {
            1d/60,
            1d/30,
            1d/20,
            1d/15,
            1d/10,
            1d/5,
            1,
            2,
            3,
            5,
            10,
            30,
            60,
            120,
            300,
            600,
            1800
        };
        private (double Delay, string Tip) SliderDelayConverter(double value)
        {
            var min = slDelay.Minimum;
            var max = slDelay.Maximum - min;
            value -= min;

            var delay = 2d;
            var tip = $"Default: {delay}s.";

            var selctor = (int)Math.Floor(value / max * (_delayStates.Count - 1));
            delay = _delayStates[selctor];
            tip = (delay > 1) ? (delay > 60) ? $"{(int)(delay / 60)}min delay." :
                $"{delay}s delay." : $"{1 / delay:0} fps.";

            return (delay * 1000, tip);
        }

        private void CbIp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentCamera = (sender as ComboBox).SelectedValue as CameraSettings;
        }
    }

    internal class UILinking : DependencyObject
    {
        public class CameraSettings
        {
            public enum CameraManufacturer { Dlink, Axis, Bosh }

            public string Ip { get; private set; }
            public CameraManufacturer Manufacturer { get; set; }

            private CameraSettings() { }
            public CameraSettings(string ip, CameraManufacturer manufacturer)
            {
                Ip = ip;
                Manufacturer = manufacturer;
            }

            public override string ToString()
            {
                return $"Ip: {Ip}; Manufacturer: {Manufacturer}.";
            }

            public string GetImageUrl()
            {
                var url = string.Empty;
                switch (Manufacturer)
                {
                    case CameraManufacturer.Dlink:
                        url = $"http://{Ip}/Image.jpg";
                        break;
                    case CameraManufacturer.Axis:
                        url = $"http://{Ip}/jpg/1/image.jpg";
                        break;
                    case CameraManufacturer.Bosh:
                        url = $"http://{Ip}/snap.jpg";
                        break;
                    default:
                        break;
                }
                return url;
            }
        }

        public UILinking()
        {
            Ips = new List<CameraSettings>
           {
               new CameraSettings ("81.149.56.38:8083", CameraSettings.CameraManufacturer.Axis),
               new CameraSettings ("69.193.149.90:82", CameraSettings.CameraManufacturer.Axis),
               new CameraSettings ("200.89.115.210", CameraSettings.CameraManufacturer.Dlink),
               new CameraSettings ("89.113.5.135", CameraSettings.CameraManufacturer.Bosh),
               new CameraSettings ("79.58.129.44:8083", CameraSettings.CameraManufacturer.Axis)
           };
        }

        public List<CameraSettings> Ips
        {
            get { return (List<CameraSettings>)GetValue(IpsProperty); }
            set { SetValue(IpsProperty, value); }
        }

        public static readonly DependencyProperty IpsProperty =
            DependencyProperty.Register("Ips", typeof(List<CameraSettings>), typeof(UILinking), new PropertyMetadata(new List<CameraSettings>()));
    }
}
