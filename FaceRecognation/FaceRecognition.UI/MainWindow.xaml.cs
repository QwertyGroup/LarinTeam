using FaceRecognition.Core;
using FaceRecognition.UI.Galley;

using System;
using System.Collections.Generic;
using System.IO;
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

namespace FaceRecognition.UI
{
	public partial class MainWindow : Window
	{
		private Core.Video _video = Core.Video.VideoInstance;
		private List<List<System.Drawing.Image>> _extractedUnchosenPeoplesFaces;
		private List<Person> _extractedPeople = new List<Person>();
		private MessageManager _msgManager = MessageManager.MsgManagerInstance;

		public MainWindow()
		{
			InitializeComponent();
			ClearCache();
			SetProgressBarParameters();
			OnValidatingEnded += async (s, a) => await CompWithArchive();
			_msgManager.OnMessageSended += onMessageSended;
		}

		private void onMessageSended(object sender, string e)
		{
			spLog.Children.Add(new TextBlock
			{
				Style = FindResource("DefaultTextStyle") as Style,
				FontSize = 11,
				Margin = new Thickness(1),
				TextWrapping = TextWrapping.Wrap,
				HorizontalAlignment = HorizontalAlignment.Left,
				Text = ">> " + e + Environment.NewLine
			});
			(spLog.Parent as ScrollViewer).ScrollToEnd();
		}

		private void BrowseVideo_Click(object sender, RoutedEventArgs e)
		{
			var fileDialog = new Microsoft.Win32.OpenFileDialog();
			fileDialog.Filter = "MP4 Video(*.mp4) | *.mp4";
			bool? result = fileDialog.ShowDialog(this);
			if (!(bool)result)
			{
				_msgManager.WriteMessage("Video selection aborted");
				return;
			}

			_video.Path = fileDialog.FileName;

			(sender as Button).Content = "Video selected";
			_msgManager.WriteMessage("Video selected" + Environment.NewLine + _video.Path);
		}

		private int numberOfPeopleToLoad;

		private void LoadNextPersonForSelection()
		{
			ImageValidatingPanel.Children.Clear();
			ThisIsNotBut.Visibility = Visibility.Visible;
			ValidateFaceBut.Visibility = Visibility.Visible;
			var person = _extractedUnchosenPeoplesFaces[numberOfPeopleToLoad - 1];
			foreach (var face in person)
			{
				ImageValidatingPanel.Children.Add(CreateImage(face));
			}
			numberOfPeopleToLoad--;
		}

		private IRecognitionProgress _progressBar;
		private async void DetectFaces_Click(object sender, RoutedEventArgs e)
		{
			cmdBrowseVideo.Content = "Browse Video";
			cmdBrowseVideo.IsEnabled = false;
			var btn = (Button)sender;
			btn.Content = "Extracting faces...";
			btn.IsEnabled = false;
			_msgManager.WriteMessage("Extracting faces...");
			_progressBar.IsPBVisible = true;
			_msgManager.SetNewProgressStatus("Extracting...");
			await Core.MicrosoftAPIs.DataBaseAPI.GroupAPI.GroupAPIinstance.Train();
			_msgManager.ReportProgress();
			_extractedUnchosenPeoplesFaces = await _video.ExtractFaces();
			_msgManager.ReportProgress();
			numberOfPeopleToLoad = _extractedUnchosenPeoplesFaces.Count;

			_msgManager.SetNewProgressStatus("Validating...");
			cmdDetectFaces.Content = "Validating...";
			_msgManager.WriteMessage("Validating started.");

			_extractedPeople = new List<Person>();
			LoadNextPersonForSelection();
		}

		private void ClearCache()
		{
			ImageProcessing.ImageProcessingInstance.ClearCache();
			if (Directory.Exists("TempData"))
			{
				Directory.Delete("TempData", true);
				_msgManager.WriteMessage("Cache cleared");
			}
		}

		private Border CreateImage(System.Drawing.Image img)
		{
			Border Border = new Border()
			{
				Width = 150,
				Height = 150,
				Margin = new Thickness(5),
				BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0))
			};
			Image Image = new Image
			{
				Source = ImageProcessing.ImageProcessingInstance.ConvertImageToBitmapImage(img),
				Width = 146,
				Height = 146,
				Stretch = Stretch.Fill
			};
			Border.Child = Image;
			Border.MouseLeftButtonUp += (sender, e) =>
			{
				var brd = (Border)sender;
				if (brd.BorderThickness.Top == 0)
				{
					brd.BorderThickness = new Thickness(2);
				}
				else
				{
					brd.BorderThickness = new Thickness(0);
				}
			};
			return Border;
		}

		private void Validate_Click(object sender, RoutedEventArgs e)
		{
			List<System.Drawing.Image> resultFacesOfPeople = new List<System.Drawing.Image>();
			foreach (var border in ImageValidatingPanel.Children)
			{
				var brd = (Border)border;
				if (brd.BorderThickness.Bottom == 2)
				{
					var img = ImageProcessing.ImageProcessingInstance.ConvertBitmapImageToImage(
						(BitmapImage)((Image)brd.Child).Source);
					resultFacesOfPeople.Add(img);
				}
			}
			_extractedPeople.Add(new Person(resultFacesOfPeople));
			if (numberOfPeopleToLoad == 0)
			{
				EndValidating();
				OnValidatingEnded?.Invoke(this, new EventArgs());
				return;
			}
			LoadNextPersonForSelection();
		}

		private event EventHandler OnValidatingEnded;
		private void EndValidating()
		{
			ImageValidatingPanel.Children.Clear();

			ThisIsNotBut.Visibility = Visibility.Hidden;
			cmdBrowseVideo.Content = "Browse Video";
			ValidateFaceBut.Visibility = Visibility.Hidden;
		}

		private void ExhibitFaceArchive_Click(object sender, RoutedEventArgs e)
		{
			new FaceExhibition().Show();
		}

		private void ThisIsNotBut_Click(object sender, RoutedEventArgs e)
		{
			if (numberOfPeopleToLoad == 0)
			{
				EndValidating();
				OnValidatingEnded?.Invoke(this, new EventArgs());
				return;
			}
			LoadNextPersonForSelection();
		}

		private async Task CompWithArchive()
		{
			_msgManager.ReportProgress();
			cmdDetectFaces.Content = "Comparing...";
			_msgManager.WriteMessage("Comparing started.");
			_msgManager.SetNewProgressStatus("Comparing...");

			var tuple = await Comparator.ComparatorInstance.SendDetectedPeopleToCompare(_extractedPeople);
			var existedPeople = tuple.Item1;
			var newPeople = tuple.Item2;
			_msgManager.ReportProgress();
			newPeople = await Core.MicrosoftAPIs.DataBaseAPI.GroupAPI.GroupAPIinstance.AddPeople(newPeople);
			_msgManager.ReportProgress();
			await Synchron.Instance.AddPeople(newPeople);
			_msgManager.ReportProgress();

			cmdDetectFaces.Content = "Detect Faces";
			cmdDetectFaces.IsEnabled = true;
			cmdBrowseVideo.IsEnabled = true;
			_msgManager.WriteMessage("Aggregating comparation report.");
			_msgManager.SetNewProgressStatus("Aggregating report.");

			new ReportGallery(newPeople, existedPeople).Show();

			_msgManager.ReportProgress();
			_msgManager.SetNewProgressStatus("Face recognition");
		}

		private void SetProgressBarParameters()
		{
			_progressBar = pbProgress;
			_progressBar.MaxProgress = 8;
			_progressBar.Progress = 0;
			_progressBar.IsPBVisible = false;
			_progressBar.TStatus = "Face recognition";
			_msgManager.OnProgressMade += (s, e) =>
			{
				_progressBar.IncBy1();
				if (e != string.Empty)
					_progressBar.TStatus = e;
			};
			_msgManager.OnNewProgressStatus += (s, e) => _progressBar.TStatus = e;
		}
	}
}
