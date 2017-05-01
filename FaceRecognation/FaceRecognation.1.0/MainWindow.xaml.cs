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
using System.Drawing;

namespace FaceRecognation._1._0
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			new XTests().Run();
		}

		private MSAPIManager _msapiManager = MSAPIManager.MSAPIManagerInstance;
		private async void cmdTakePhoto_Click(object sender, RoutedEventArgs e)
		{
			var openDlg = new Microsoft.Win32.OpenFileDialog();

			openDlg.Filter = "JPEG Image(*.jpg)|*.jpg|PNG Image(*.png)|*.png";
			bool? result = openDlg.ShowDialog(this);

			if (!(bool)result)
			{
				return;
			}

			string filePath = openDlg.FileName;

			Uri fileUri = new Uri(filePath);
			BitmapImage bitmapSource = new BitmapImage();

			bitmapSource.BeginInit();
			bitmapSource.CacheOption = BitmapCacheOption.None;
			bitmapSource.UriSource = fileUri;
			bitmapSource.EndInit();

			imgPhoto.Source = bitmapSource;

			using (Stream fstream = File.OpenRead(filePath))
			{
				var faces = await _msapiManager.DetectFace(fstream);
				var rect = faces[0].FaceRectangle;
				//var rect = new Microsoft.ProjectOxford.Common.Rectangle { Left = 100, Top = 50, Width = 300, Height = 200 };
				var croppedBm = new CroppedBitmap(bitmapSource, new Int32Rect(rect.Left, rect.Top, rect.Width, rect.Height));
				imgPhoto.Source = croppedBm;
			}


		}

		private void cmdAddFace_Click(object sender, RoutedEventArgs e)
		{

		}

		private void cmdFindSimilar_Click(object sender, RoutedEventArgs e)
		{

		}

		private void cmdCreateFaceList_Click(object sender, RoutedEventArgs e)
		{

		}
	}

	public class XTests
	{

		public void Run()
		{
			//Debug.WriteLine("KEK");
			//VideoManager.getOperationAsync("1.mp4");
		}
	}
}
