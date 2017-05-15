using FaceRecognition.Core;

using System.Collections.Generic;
using System.Windows;

namespace FaceRecognition.UI.Galley
{
	public partial class ReportGallery : Window
	{
		private ReportGallery()
		{
			InitializeComponent();
		}

		private List<Person> _newPeople;
		private List<Person> _existedPeople;
		public ReportGallery(List<Person> newPeople, List<Person> existedPeople) : this()
		{
			_newPeople = newPeople;
			_existedPeople = existedPeople;
			Loaded += (s, e) => Exhibit();
		}

		public void Exhibit()
		{
			foreach (var person in _newPeople)
				spNew.Children.Add(new Painting(person));
			foreach (var person in _existedPeople)
				spExisted.Children.Add(new Painting(person));
		}
	}
}
