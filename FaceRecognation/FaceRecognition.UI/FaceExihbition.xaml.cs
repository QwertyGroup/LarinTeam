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
using System.Windows.Shapes;

namespace FaceRecognition.UI.Galley
{
	public partial class FaceExhibition : Window
	{
		private FaceExhibition()
		{
			InitializeComponent();
		}

		private Dictionary<int, GPerson> _persons;
		private double _galleryWidth;
		private double _galleryHeight;
		public FaceExhibition(Dictionary<int, GPerson> persons) : this()
		{
			_persons = persons;
			_galleryHeight = spGallery.Height;
			_galleryWidth = spGallery.Width;
			Exhitbit();
		}

		private void Exhitbit()
		{
			foreach (var person in _persons)
				spGallery.Children.Add(new Painting(person.Value));
		}
	}
}
