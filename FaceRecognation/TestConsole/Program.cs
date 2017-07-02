using FaceRecognition.Core;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestConsole
{
    class Program
    {
        //private static GroupManager _gm = GroupManager.GroupManagerInstance;
        static void Main(string[] args)
        {
            MessageManager.MsgManagerInstance.OnMessageSended += (s, e) => Console.WriteLine("Log >> " + e);
            Test();
            Console.ReadLine();
        }

        private async static void Test()
        {
            var path = "VectImgs/";
            var img1 = Image.FromFile($"{path}vector1.jpg");
            var faces = await FaceRecognition.Core.MicrosoftAPIs.ComparationAPI.Commands.
                 CommandsInstance.DetectFaceWithLandmarks(img1);
            var face = faces.First();

            var landmarks = face.FaceLandmarks;

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

            Console.ReadLine();
        }
    }
}
