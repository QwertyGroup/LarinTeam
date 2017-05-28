﻿using FaceRecognition.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TestCCTV
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _dt = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _dt.Tick += (o, a) => Timer_Tick();
                _dt.Interval = TimeSpan.FromMilliseconds(slDelay.Value);
            };
        }

        private async void Timer_Tick()
        {
            string url = "http://69.193.149.90:82/jpg/1/image.jpg";
            var req = (HttpWebRequest)WebRequest.Create(url);
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
            _dt.Interval = TimeSpan.FromMilliseconds(e.NewValue);
        }
    }
}