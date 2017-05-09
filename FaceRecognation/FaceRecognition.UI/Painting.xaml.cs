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

		private GPerson _person;
		public Painting(GPerson person) : this()
		{
			_person = person;
			DisplayInfo();
			DisplayFaces();
		}

		private void DisplayFaces()
		{
			foreach (var face in _person.Faces)
				spFaceContainer.Children.Add(new Image
				{
					Source = ImageProcessing.ImageProcessingInstance.ConvertImageToBitmapImage(face.Img),
					Width = face.Img.Width,
					Height = face.Img.Height,
					Stretch = Stretch.Fill
				});
		}

		private void DisplayInfo()
		{
			var info =
				$"local id: {_person.PersonLocalId}{Environment.NewLine}Name: Alisa{Environment.NewLine}Face count:{_person.Faces.Count}";
			tbInfo.Text = info;
		}
	}
}
