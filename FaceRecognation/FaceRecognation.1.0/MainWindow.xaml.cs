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
		private List<System.Drawing.Image> _faces = new List<System.Drawing.Image>();
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
			var photo = System.Drawing.Image.FromFile(filePath);
			_faces.Add(photo);
			spTakenPhotos.Children.Add(GenerateImg(photo));
		}

		private System.Windows.Controls.Image GenerateImg(System.Drawing.Image photo)
		{
			return new System.Windows.Controls.Image
			{
				Source = _imgProcessing.ConvertImageToBitmapImage(photo),
				Height = 120,
				Width = 120,
				Stretch = Stretch.Uniform,
				Margin = new Thickness(7)
			};
		}

		private void cmdAddFace_Click(object sender, RoutedEventArgs e)
		{

		}

		private async void cmdDetectFace_Click(object sender, RoutedEventArgs e)
		{
			var res = new List<System.Drawing.Image>();
			foreach (var photo in _faces)
			{
				var faces = await _msapiManager.DetectFace(_imgProcessing.ImageToStream(photo));
				foreach (var face in faces)
				{
					var croppedFace = _imgProcessing.CropImage(photo, face.FaceRectangle);
					res.Add(croppedFace);
				}
			}
			_faces = res;
			spTakenPhotos.Children.Clear();
			foreach (var face in _faces)
			{
				spTakenPhotos.Children.Add(GenerateImg(face));
			}
		}

		private void cmdClearCache_Click(object sender, RoutedEventArgs e)
		{
			_imgProcessing.ClearCache();
			_faces = new List<System.Drawing.Image>();
			spTakenPhotos.Children.Clear();
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
			Debug.WriteLine("KEK");
			VideoManager.getFacesFromVideo("1.mp4");
		}
	}
}
