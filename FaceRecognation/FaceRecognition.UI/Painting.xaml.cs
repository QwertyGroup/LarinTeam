using FaceRecognition.Core;
using FaceRecognition.UI.Gallery;
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

		//private float _imgWidth = 200;
		//private float _imgHeight = 200;
		public bool IsDeleteButtonVisible { get; set; }
		private void DisplayFaces()
		{
			foreach (var face in _person.Faces)
			{
				FacePanel.Children.Add(new FacePainting(face.Image)
				{
					Width = 120,
					Height = 120,
					Margin = new Thickness(5),
					IsDeleteButtonVisible = IsDeleteButtonVisible
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

		private void deleteFaceButt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var tb = (TextBlock)sender;
			tb.Foreground = new SolidColorBrush(Color.FromRgb(189, 0, 0));
		}
	}
}
