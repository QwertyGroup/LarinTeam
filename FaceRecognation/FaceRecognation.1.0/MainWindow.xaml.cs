using FaceRecognition.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FaceRecognation._1._0
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			MessageManager.MsgManagerInstance.mainWindow = this;
			Loaded += (s, e) => MessageManager.MsgManagerInstance.WriteMessage("Program started");

			FaceRecognition.Core.MessageManager.MsgManagerInstance.OnMessageSended +=
				(s, e) => MessageManager.MsgManagerInstance.WriteMessage(e);
			//Synchron.Instance.Test();
		}

		private FaceApiManager _msapiManager = FaceApiManager.FaceApiManagerInstance;
		private ImageProcessing _imgProcessing = ImageProcessing.ImageProcessingInstance;
		private string _videoPath;
		private Dictionary<int, List<System.Drawing.Image>> unchosenPhotos;
		private int LoadedPeople = 0;
		private List<System.Drawing.Image> Faces = new List<System.Drawing.Image>();
		private void cmdTakePhoto_Click(object sender, RoutedEventArgs e)
		{
			var openDlg = new Microsoft.Win32.OpenFileDialog();
			openDlg.Filter = "MP4 Video(*.mp4) | *.mp4"; //"JPEG Image(*.jpg)|*.jpg|PNG Image(*.png)|*.png|MP4 Video(*.mp4)|*.mp4";
			bool? result = openDlg.ShowDialog(this);

			if (!(bool)result)
			{
				return;
			}

			string filePath = openDlg.FileName;
			cmdDetectFace.IsEnabled = true;

			// video
			_videoPath = filePath;
			(sender as Button).Content = "Video selected";
			MessageManager.MsgManagerInstance.WriteMessage("Video selected");
			MessageManager.MsgManagerInstance.WriteMessage(_videoPath);
		}

		private System.Windows.Controls.Image GenerateImg(System.Drawing.Image photo)
		{
			Image img = new System.Windows.Controls.Image
			{
				Source = _imgProcessing.ConvertImageToBitmapImage(photo),
				Height = 200,
				Width = 200,
				Stretch = Stretch.Uniform,
				Margin = new Thickness(7),
			};
			img.MouseLeftButtonUp += (sender, e) =>
			{
				var tempImg = (Image)sender;
				MessageManager.MsgManagerInstance.WriteMessage("Selected 1 face");
				Faces.Add(ImageProcessing.ImageProcessingInstance.ConvertBitmapImageToImage((BitmapImage)img.Source));
				if (LoadedPeople < unchosenPhotos.Keys.Count)
				{
					fillUndetectedFaces();
					return;
				}
				if (LoadedPeople == unchosenPhotos.Keys.Count)
				{
					//fillUndetectedFaces();
					DetectFacesPanel.Children.Clear();
					notFaceButton.Visibility = Visibility.Collapsed;
					cmdTakePhoto.Content = "Browse Video";
					cmdDetectFace.Content = "Detect Faces";
					cmdDetectFace.IsEnabled = false;
					SavePhotos();
				}
			};
			return img;
		}

		private void SavePhotos()
		{
			if (!Directory.Exists("ResultFaces"))
				Directory.CreateDirectory("ResultFaces");
			foreach (var path in Directory.GetFiles("ResultFaces"))
			{
				File.Delete(path);
			}
			for (int i = 0; i < Faces.Count; i++)
			{
				ImageProcessing.ImageProcessingInstance.SaveImageToFile($"ResultFaces/{i}", Faces[i], System.Drawing.Imaging.ImageFormat.Png);
				Faces[i].Dispose();
			}
			MessageManager.MsgManagerInstance.WriteMessage("Saved all faces to ResultFaces");
		}

		private void fillUndetectedFaces()
		{
			notFaceButton.Visibility = Visibility.Visible;
			DetectFacesPanel.Children.Clear();
			var currentPerson = unchosenPhotos[unchosenPhotos.Keys.ElementAt(LoadedPeople)];
			foreach (var person in currentPerson)
			{
				DetectFacesPanel.Children.Add(GenerateImg(person));
			}
			LoadedPeople++;
		}
		private async void cmdDetectFace_Click(object sender, RoutedEventArgs e)
		{
			LoadedPeople = 0;
			Faces.Clear();
			var btn = sender as Button;
			btn.Content = "Detecting...";
			MessageManager.MsgManagerInstance.WriteMessage(btn.Content.ToString());

			// Detecting for Videos
			unchosenPhotos = await VideoManager.VManagerInstance.GetFacesFromVideo(_videoPath);

			btn.Content = "Detected successfuly.";
			MessageManager.MsgManagerInstance.WriteMessage(btn.Content.ToString());
			// Selecting One face from Five given
			var faces = new List<System.Drawing.Image>();
			if (unchosenPhotos.Count > 1)
			{
				MessageManager.MsgManagerInstance.WriteMessage($"Found {unchosenPhotos.Count} people");
			}
			else
			{
				MessageManager.MsgManagerInstance.WriteMessage($"Found {unchosenPhotos.Count} person");
			}
			fillUndetectedFaces();
		}

		private void cmdClearCache_Click(object sender, RoutedEventArgs e)
		{
			_imgProcessing.ClearCache();
			if (Directory.Exists("TempData"))
			{
				Directory.Delete("TempData", true);
			}
			//spTakenPhotos.Children.Clear();
			lbCompResults.Items.Clear();
			MessageManager.MsgManagerInstance.WriteMessage("Cache cleared");
		}

		private void notFaceButton_Click(object sender, RoutedEventArgs e)
		{
			if (LoadedPeople < unchosenPhotos.Keys.Count)
			{
				fillUndetectedFaces();
				return;
			}
			if (LoadedPeople == unchosenPhotos.Keys.Count)
			{
				DetectFacesPanel.Children.Clear();
				notFaceButton.Visibility = Visibility.Collapsed;
				cmdTakePhoto.Content = "Browse Video";
				cmdDetectFace.Content = "Detect Faces";
				cmdDetectFace.IsEnabled = false;
				SavePhotos();
			}
		}
	}
}
