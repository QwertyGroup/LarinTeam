using FaceRecognition.Core;

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

namespace FaceRecognition.UI.Galley
{
	public partial class Painting : UserControl
	{
		private Painting()
		{
			InitializeComponent();
		}

		private Person _person;
		public Painting(Person person) : this()
		{
			_person = person;
			DisplayInfo();
			DisplayFaces();
		}

		private float _imgWidth = 200;
		private float _imgHeight = 200;
		private void DisplayFaces()
		{
			foreach (var face in _person.Faces)
				spFaceContainer.Children.Add(new Image
				{
					Source = ImageProcessing.ImageProcessingInstance.ConvertImageToBitmapImage(face.Image),
					Width = _imgWidth,
					Height = _imgHeight,
					Stretch = Stretch.Fill
				});
		}

		private void DisplayInfo()
		{
			var info = $"Face count:{_person.Faces.Count}";
			tbInfo.Text = info;
		}
	}
}
