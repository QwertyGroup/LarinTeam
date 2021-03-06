﻿using FaceRecognition.Core;
using FaceRecognition.UI.Gallery;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaceRecognition.UI.Galley
{
	public partial class Painting : UserControl
	{
		private Painting()
		{
			InitializeComponent();
		}

		public Person _person;
		public Painting(Person person, bool showDeleteBnt = true) : this()
		{
			_person = person;
			IsDeleteButtonVisible = showDeleteBnt;
			deleteFaceButt.Visibility = (IsDeleteButtonVisible) ? Visibility.Visible : Visibility.Hidden;
			DisplayInfo();
			DisplayFaces();
		}

		//private float _imgWidth = 200;
		//private float _imgHeight = 200;
		public bool IsDeleteButtonVisible { get; set; }
		private void DisplayFaces()
		{
			foreach (var face in _person.Faces)
			{
				FacePanel.Children.Add(new Image()
				{
					Source = ImageProcessing.ImageProcessingInstance.ConvertImageToBitmapImage(face.Image),
					Width = 120,
					Height = 120,
					Margin = new Thickness(5),
				});
			}

		}

		private void DisplayInfo()
		{
			var info = $"Person Id: {_person.MicrosoftPersonId}";
			tbInfo.Text = info;
		}

		private void deleteFaceButt_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var tb = (TextBlock)sender;
			tb.Foreground = new SolidColorBrush(Color.FromRgb(170, 0, 0));
		}

		private async void deleteFaceButt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var tb = (TextBlock)sender;
			tb.Foreground = new SolidColorBrush(Color.FromRgb(189, 0, 0));
			await Core.MicrosoftAPIs.DataBaseAPI.PersonAPI.PersonAPIinstance.DeletePerson(_person);
			foreach (var face in _person.Faces.Select(x => x.MicrosoftId))
				await Synchron.Instance.DeleteFace(face);
			((FaceExhibition)((Grid)((ScrollViewer)((StackPanel)Parent).Parent).Parent).Parent).Data.Remove(_person);
			((FaceExhibition)((Grid)((ScrollViewer)((StackPanel)Parent).Parent).Parent).Parent).UpdateInfo();
			((StackPanel)Parent).Children.Remove(this);
		}
	}
}
