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

		private List<Person> _people;
		public FaceExhibition(List<Person> persons) : this()
		{
			_people = persons;
			Exhitbit();
		}

		private void Exhitbit()
		{
			foreach (var person in _people)
				spGallery.Children.Add(new Painting(person));
		}
	}
}
