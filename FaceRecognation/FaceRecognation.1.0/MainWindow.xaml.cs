using System;
using System.Collections.Generic;
using System.Linq;
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
using Newtonsoft.Json;
using Microsoft.ProjectOxford.Common;
using Microsoft.ProjectOxford.Face;
using System.IO;
using System.Diagnostics;

namespace FaceRecognation._1._0
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			//new XTests().Run();
		}
	}

	public class XTests
	{
		Face Mark = new Face();

		public void Run()
		{
			System.Drawing.Image img = System.Drawing.Image.FromFile("6.png");
			Mark.BaseImages.Add(Face.ImageToBase(img, System.Drawing.Imaging.ImageFormat.Png));
			img = Face.BaseToImage(Mark.BaseImages[0]);
			Debug.WriteLine(img.Size);
			img.Save("kek.png", System.Drawing.Imaging.ImageFormat.Png);
		}
	}
}
