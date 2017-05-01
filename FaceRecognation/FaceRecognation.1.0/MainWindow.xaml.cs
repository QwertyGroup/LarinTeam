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
using System.Threading;

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
		private ImageProcessing _imgProcessing = ImageProcessing.ImageProcessingInstance;
		private List<System.Drawing.Image> _faces = new List<System.Drawing.Image>();
		private void cmdTakePhoto_Click(object sender, RoutedEventArgs e)
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

		private List<Guid> _persistedIds = new List<Guid>();
		private async void cmdAddFace_Click(object sender, RoutedEventArgs e)
		{
			foreach (var face in _faces)
			{
				var pres = await _msapiManager.AddFaceToFaceList(_facelistId, _imgProcessing.ImageToStream(face));
				_persistedIds.Add(pres.PersistedFaceId);
			}
		}

		private List<MSAPIManager.FaceIdAndRect> _faceIdAndRectList = new List<MSAPIManager.FaceIdAndRect>();
		private async void cmdDetectFace_Click(object sender, RoutedEventArgs e)
		{
			(sender as Button).Content = "Detecting...";
			var res = new List<System.Drawing.Image>();
			foreach (var photo in _faces)
			{
				var faces = await _msapiManager.GetFaceRectangle(_imgProcessing.ImageToStream(photo));
				foreach (var face in faces)
				{
					var croppedFace = _imgProcessing.CropImage(photo, face.FaceRect);
					_faceIdAndRectList.Add(face);
					res.Add(croppedFace);
				}
			}
			_faces = res;
			spTakenPhotos.Children.Clear();
			foreach (var face in _faces)
			{
				spTakenPhotos.Children.Add(GenerateImg(face));
			}
			(sender as Button).Content = "Detected successfuly.";
			await Task.Delay(TimeSpan.FromSeconds(3));
			(sender as Button).Content = "Detect Faces";
		}

		private void cmdClearCache_Click(object sender, RoutedEventArgs e)
		{
			_imgProcessing.ClearCache();
			_faces = new List<System.Drawing.Image>();
			spTakenPhotos.Children.Clear();
		}

		private async void cmdFindSimilar_Click(object sender, RoutedEventArgs e)
		{
			if (_faces.Count < 2) return;
			var compResult = await _msapiManager.CheckForSimilarity(_faceIdAndRectList.First(), _facelistId);
			foreach (var r in compResult)
				lbCompResults.Items.Add($"ID: {r.PersistedFaceId}; Conf: {r.Confidence:f}");
		}

		private string _facelistId = "facelist0";
		private void cmdCreateFaceList_Click(object sender, RoutedEventArgs e)
		{
			_msapiManager.CreateFaceList(_facelistId, "Lace list 0");
		}
	}

	public class XTests
	{

		public void Run()
		{
			Debug.WriteLine("KEK");
			//VideoManager.getFacesFromVideo("1.mp4");
		}
	}
}
