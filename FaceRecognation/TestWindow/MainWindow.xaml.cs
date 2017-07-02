using FaceRecognition.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

namespace TestWindow
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            cmdStart.Click += CmdStart_Click;

            VectTest();
        }

        private void VectTest()
        {
            var v1 = new Vector3(2, 1, 2);
            var v2 = new Vector3(3, 0, 1);
            var v3 = Vector3.Cross(v1, v2);
        }

        private async void CmdStart_Click(object sender, RoutedEventArgs e)
        {
            var faces = await FaceRecognition.Core.MicrosoftAPIs.ComparationAPI.Commands.
                 CommandsInstance.DetectFaceWithLandmarks(face);
            var vface = faces.First();

            var landmarks = vface.FaceLandmarks;

            var upperLipBottom = landmarks.UpperLipBottom;
            var underLipTop = landmarks.UnderLipTop;

            var centerOfMouth = new System.Windows.Point(
                (upperLipBottom.X + underLipTop.X) / 2,
                (upperLipBottom.Y + underLipTop.Y) / 2);

            var eyeLeftInner = landmarks.EyeLeftInner;
            var eyeRightInner = landmarks.EyeRightInner;

            var centerOfTwoEyes = new System.Windows.Point(
                (eyeLeftInner.X + eyeRightInner.X) / 2,
                (eyeLeftInner.Y + eyeRightInner.Y) / 2);

            Vector faceDirection = new Vector(
                centerOfTwoEyes.X - centerOfMouth.X,
                centerOfTwoEyes.Y - centerOfMouth.Y);

            // Kekstvo
            var width = face.Width;
            var height = face.Height;
            var size = face.Size;
            var hr = face.HorizontalResolution;
            var vr = face.VerticalResolution;

            //// points = pixels * 72 / g.DpiX;
            ////var pixelWidth = (int)(graphics.DpiX / 72.0);
            ////var pixelHeight = (int)(graphics.DpiY / 72.0);

            ////line.X1 = landmarks.NoseTip.X * 72 / graphics.DpiX; // / width * faceimage.ActualWidth;
            ////line.Y1 = landmarks.NoseTip.Y * 72 / graphics.DpiY; // / height * faceimage.ActualHeight;
            //var wk = faceimage.ActualWidth / width;
            //var hk = faceimage.ActualHeight / height;
            //line.X1 = landmarks.NoseTip.X * wk;
            //line.Y1 = landmarks.NoseTip.Y * hk;

            ////line.X2 = line.X1 + faceDirection.X * 72 / graphics.DpiX;
            ////line.Y2 = line.Y1 + faceDirection.Y * 72 / graphics.DpiY;

            //line.X2 = line.X1 + faceDirection.X * wk;
            //line.Y2 = line.Y1 + faceDirection.Y * hk;

        }

        private System.Drawing.Image face;
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var path = "VectImgs/";
            face = System.Drawing.Image.FromFile($"{path}vector1.jpg");
            faceimage.Source = ImageProcessing.ImageProcessingInstance.ConvertImageToBitmapImage(face);
        }


    }
}
