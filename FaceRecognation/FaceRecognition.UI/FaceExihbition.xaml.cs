using FaceRecognition.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace FaceRecognition.UI.Galley
{
	public partial class FaceExhibition : Window
	{
		public List<Person> Data;
		private int FaceCount;
		public FaceExhibition()
		{
			InitializeComponent();
		}

		private async Task Exhibit()
		{
			spGallery.Children.Clear();
			await GetData();
			foreach (var person in Data)
				spGallery.Children.Add(new Painting(person));
		}

		private async Task GetData()
		{
			Data = await Core.MicrosoftAPIs.DataBaseAPI.PersonAPI.PersonAPIinstance.GetPersonList(ProgressBar);
			foreach (var person in Data)
				FaceCount += person.Faces.Count;
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			infoLabel.Content = "Total People: - . Total Faces: - .";

			FaceCount = 0;
			var btn = (Button)sender;
			btn.IsEnabled = false;
			ProgressBar.Visibility = Visibility.Visible;
			await Exhibit();
			ProgressBar.Value = 0;
			ProgressBar.Visibility = Visibility.Hidden;
			btn.IsEnabled = true;

			infoLabel.Content = $"Total People: {Data.Count}. Total Faces: {FaceCount}.";
		}

		public void UpdateInfo()
		{
			FaceCount = 0;
			foreach (var person in Data)
				FaceCount += person.Faces.Count;
			infoLabel.Content = $"Total People: {Data.Count}. Total Faces: {FaceCount}.";
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			FaceCount = 0;
			updatebut.IsEnabled = false;
			ProgressBar.Visibility = Visibility.Visible;
			await Exhibit();
			ProgressBar.Value = 0;
			ProgressBar.Visibility = Visibility.Hidden;
			updatebut.IsEnabled = true;
			infoLabel.Content = $"Total People: {Data.Count}. Total Faces: {FaceCount}.";
		}

		private void button_Click_1(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine($"{Data.Count}");
		}
	}
}
