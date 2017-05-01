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
		private ImageProcessing _imgProcessing = ImageProcessing.GetImageProcessingInstance;
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

			var sysdrwImage = System.Drawing.Image.FromFile(filePath);
			imgPhoto.Source = _imgProcessing.ConvertImageToBitmapImage(sysdrwImage);

			var faces = await _msapiManager.DetectFace(_imgProcessing.ImageToStream(sysdrwImage));

			imgPhoto.Source = _imgProcessing.ConvertImageToBitmapImage(_imgProcessing.CropImage(sysdrwImage, faces[0].FaceRectangle));
		}

		private void cmdAddFace_Click(object sender, RoutedEventArgs e)
		{

		}

		private void cmdFindSimilar_Click(object sender, RoutedEventArgs e)
		{

		}

		private void cmdCreateFaceList_Click(object sender, RoutedEventArgs e)
		{
			_imgProcessing.ClearCache();
		}
	}

	public class XTests
	{

		public void Run()
		{
			Debug.WriteLine("KEK");
			VideoManager.getFacesFromVideo("1.mp4");
		}
	}
}
