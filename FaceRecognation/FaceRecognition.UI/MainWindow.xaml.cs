using FaceRecognition.Core;

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
        private Video _video;
        
		private MessageManager _msgManager = MessageManager.MsgManagerInstance;

        private int peopleLoaded = 0;

		public MainWindow()
		{
			InitializeComponent();
            _video = new Video(ImageValidatingPanel);
			Loaded += (s, e) => DataContext = _video;
			_msgManager.OnMessageSended += (s, e) =>
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
			};
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

		private async void DetectFaces_Click(object sender, RoutedEventArgs e)
		{
			var bnt = (Button)sender;
			bnt.Content = "Extracting faces...";
			_msgManager.WriteMessage("Extracting faces...");
			await _video.ExtractFaces();
			bnt.Content = "Extracted";
			_msgManager.WriteMessage("Faces were successfuly extracted.");
			await Task.Delay(500);
			bnt.Content = "Validating faces...";
			//_msgManager.WriteMessage("Seleced 0 faces of 6.");
			// ДАЛЬШЕ ВЫБИРАЕМ ЛИЦА И ... Это пишет Марк
			_video.ValidateFaces();
		}

		private void ClearCache_Click(object sender, RoutedEventArgs e)
		{
			ImageProcessing.ImageProcessingInstance.ClearCache();
			if (Directory.Exists("TempData"))
			{
				Directory.Delete("TempData", true);
			}
			_msgManager.WriteMessage("Cache cleared");
		}
	}
}
