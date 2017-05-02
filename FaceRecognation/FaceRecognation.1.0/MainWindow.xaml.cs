using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FaceRecognation._1._0
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			XTests test = new XTests();
			test.Run();
		}

		private MSAPIManager _msapiManager = MSAPIManager.MSAPIManagerInstance;
		private ImageProcessing _imgProcessing = ImageProcessing.ImageProcessingInstance;
		private string _videoPath;
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

			// video
			_videoPath = filePath;
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

		private class FacesSelectedCounter
		{
			public event EventHandler OnAllWindowsClosed;
			private int _closedCounter;
			private int _maxWindows;
			private FacesSelectedCounter() { }
			public FacesSelectedCounter(int maxWindows)
			{
				_maxWindows = maxWindows;
			}
			public void IncCounter()
			{
				_closedCounter++;
				if (_closedCounter == _maxWindows)
					OnAllWindowsClosed?.Invoke(this, new EventArgs());
			}
		}

		private async void cmdDetectFace_Click(object sender, RoutedEventArgs e)
		{
			(sender as Button).Content = "Detecting...";

			// Detecting for Videos
			var faces4eachPerson = await VideoManager.getFacesFromVideo(_videoPath);
			(sender as Button).Content = "Detected successfuly.";

			// Selecting One face from Five given
			var faces = new List<System.Drawing.Image>();
			var faceCounter = new FacesSelectedCounter(faces4eachPerson.Count);
			faceCounter.OnAllWindowsClosed += async (so, a) =>
			{
				spTakenPhotos.Children.Clear();
				foreach (var face in faces)
					spTakenPhotos.Children.Add(GenerateImg(face));
				(sender as Button).Content = "Comparing...";
				var compResult = await _msapiManager.FindSimilar(faces.First(), faces.ToArray());

				foreach (var r in compResult)
					lbCompResults.Items.Add($"ID: {r.PersistedFaceId}; Conf: {r.Confidence:f}");

				(sender as Button).Content = "Compared successfuly.";
				await Task.Delay(TimeSpan.FromSeconds(3));
				(sender as Button).Content = "Compare Faces";
			};
			foreach (var person in faces4eachPerson)
			{
				var winSf = new windowSelectFace(person.Value);
				winSf.OnFaceSelected += (s, args) =>
				{
					faces.Add(args.Face);
					faceCounter.IncCounter();
				};
				winSf.Show();
			}
		}

		private void cmdClearCache_Click(object sender, RoutedEventArgs e)
		{
			_imgProcessing.ClearCache();
			spTakenPhotos.Children.Clear();
			lbCompResults.Items.Clear();
		}
	}

	public class XTests
	{

		public void Run()
		{
			//Debug.WriteLine("KEK");
			//VideoManager.getFacesFromVideo("1.mp4");
		}
	}
}
