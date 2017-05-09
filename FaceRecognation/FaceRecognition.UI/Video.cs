using FaceRecognition.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FaceRecognition.UI
{
	public class Video : DependencyObject
	{
		private string _path;
		public string Path
		{
			get { return _path; }
			set
			{
				_path = value;
				if (_path != string.Empty) VideoSelected = true;
				else VideoSelected = false;
			}
		}

		public bool VideoSelected
		{
			get { return (bool)GetValue(VideoSelectedProperty); }
			set { SetValue(VideoSelectedProperty, value); }
		}

		public Video(System.Windows.Controls.WrapPanel ImageValidatingPanel)
		{
			_imageValidatingPanel = ImageValidatingPanel;
		}

		public static readonly DependencyProperty VideoSelectedProperty =
			DependencyProperty.Register("VideoSelected", typeof(bool), typeof(Video));

		public Dictionary<int, GPerson> GPersons { get; set; } = new Dictionary<int, GPerson>();
		public Dictionary<int, List<Image>> _extractedFaces;
		public async Task ExtractFaces()
		{
			_extractedFaces = await VideoManager.VManagerInstance.GetFacesFromVideo(Path);
			foreach (var exFace in _extractedFaces)
				GPersons.Add(exFace.Key, new GPerson { PersonLocalId = exFace.Key });
		}

		public Dictionary<int, List<Image>> ValidFaces { get; set; } = new Dictionary<int, List<Image>>();

		private System.Windows.Controls.WrapPanel _imageValidatingPanel;

		private int _selectedFaceCounter;
		private int _totalSelctedFaceCounter;
		private System.Windows.Controls.Border CreateImage(Image img)
		{
			System.Windows.Controls.Border Border = new System.Windows.Controls.Border()
			{
				Width = 150,
				Height = 150,
				BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0))
			};
			System.Windows.Controls.Image Image = new System.Windows.Controls.Image
			{
				Source = ImageProcessing.ImageProcessingInstance.ConvertImageToBitmapImage(img),
				Width = 146,
				Height = 146,
				Stretch = System.Windows.Media.Stretch.Fill
			};
			Border.Child = Image;
			Border.MouseLeftButtonUp += (sender, e) =>
			{
				var brd = (System.Windows.Controls.Border)sender;
				if (brd.BorderThickness.Top == 0)
				{
					brd.BorderThickness = new Thickness(2);
					_selectedFaceCounter++;
					_totalSelctedFaceCounter++;
				}
				else
				{
					brd.BorderThickness = new Thickness(0);
					_selectedFaceCounter--;
					_totalSelctedFaceCounter--;
				}
				MessageManager.MsgManagerInstance.WriteMessage(
					$"{_selectedFaceCounter} faces selected.{Environment.NewLine}Total: {_totalSelctedFaceCounter}");
			};
			_selectedFaceCounter = 0;
			return Border;
		}
		public int _num;
		public void LoadNextPerson()
		{
			_imageValidatingPanel.Children.Clear();
			var currentPerson = _extractedFaces[_num];
			foreach (var faceImage in currentPerson)
			{
				_imageValidatingPanel.Children.Add(CreateImage(faceImage));
			}
			_num++;
		}

		public void ValidateFaces()
		{
			LoadNextPerson();
		}

		private FaceApiManager _faceApiManager = FaceApiManager.FaceApiManagerInstance;
		public async Task AppendGroup()
		{
			foreach (var vFace in ValidFaces) // fill GPersons with valid faces
				GPersons[vFace.Key].Faces = vFace.Value.Select(x => new GFace { Img = x }).ToList();

			for (int i = 0; i < GPersons.Count; i++) // Remove people with unvalid faces
				if (GPersons[i].Faces.Count == 0) GPersons.Remove(i);

			//await _faceApiManager.A

		}
	}
}
