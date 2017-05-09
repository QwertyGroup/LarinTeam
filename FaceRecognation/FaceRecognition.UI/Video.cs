using FaceRecognition.Core;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

		private Dictionary<int, List<System.Drawing.Image>> _extractedFaces;
		public Dictionary<int, List<System.Drawing.Image>> ExtractedFaces
		{
			get { return _extractedFaces; }
			set { _extractedFaces = value; if (_extractedFaces.Count != 0) AreFacesExtracted = true; else AreFacesExtracted = false; }
		}

		public bool AreFacesExtracted
		{
			get { return (bool)GetValue(AreFacesExtractedProperty); }
			set { SetValue(AreFacesExtractedProperty, value); }
		}
		public static readonly DependencyProperty AreFacesExtractedProperty =
			DependencyProperty.Register("AreFacesExtracted", typeof(bool), typeof(Video));


		public async Task ExtractFaces()
		{
			ExtractedFaces = await VideoManager.VManagerInstance.GetFacesFromVideo(Path);
			var curFaceCount = GPersons.Count;
			foreach (var exFace in ExtractedFaces)
				GPersons.Add(exFace.Key + curFaceCount, new GPerson { PersonLocalId = exFace.Key });
			_num = 0;
		}

		public Dictionary<int, List<System.Drawing.Image>> ValidFaces { get; set; } = new Dictionary<int, List<System.Drawing.Image>>();

		private System.Windows.Controls.WrapPanel _imageValidatingPanel;

		private int _selectedFaceCounter;
		private int _totalSelctedFaceCounter;
		private System.Windows.Controls.Border CreateImage(System.Drawing.Image img)
		{
			System.Windows.Controls.Border Border = new System.Windows.Controls.Border()
			{
				Width = 150,
				Height = 150,
				Margin = new Thickness(5),
				BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0))
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
			};
			_selectedFaceCounter = 0;
			return Border;
		}
		public int _num;
		public void LoadNextPerson()
		{
			_imageValidatingPanel.Children.Clear();
			var currentPerson = ExtractedFaces[_num];
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
		public async Task AppendGroup(Button btn)
		{
			try
			{
				foreach (var vFace in ValidFaces) // fill GPersons with valid faces
					GPersons[vFace.Key].Faces = vFace.Value.Select(x => new GFace { Img = x }).ToList();

				for (int i = 0; i < GPersons.Count; i++) // Remove people with unvalid faces
					if (GPersons[i].Faces.Count == 0) GPersons.Remove(i);

				btn.Content = "Identifying...";
				MessageManager.MsgManagerInstance.WriteMessage("Aggregating MS ids for comparing face request...");
				var listofGuidsToCompare = new List<Guid[]>();
				var counter = 0;
				var tenfaces = new Guid[10];
				foreach (var person in GPersons)
					foreach (var face in person.Value.Faces)
					{
						if (counter == 10)
						{
							listofGuidsToCompare.Add(tenfaces);
							counter = 0;
							tenfaces = new Guid[10];
						}
						tenfaces[counter] = (await _faceApiManager.DetectFace(
							ImageProcessing.ImageProcessingInstance.ImageToStream(face.Img)))[0].FaceId;
						counter++;
					}
				MessageManager.MsgManagerInstance.WriteMessage("Successfuly aggregated.");
				await _faceApiManager.TrainGroup();
				MessageManager.MsgManagerInstance.WriteMessage("Comparing new faces with archive...");
				var result = new List<IdentifyResult>();
				foreach (var tens in listofGuidsToCompare)
					result.AddRange(await _faceApiManager.Identify(tens));

				MessageManager.MsgManagerInstance.WriteMessage("Comparing result recived!");
			}
			catch (Exception ex)
			{
				MessageManager.MsgManagerInstance.WriteMessage(ex.Message);
			}
		}
	}
}
