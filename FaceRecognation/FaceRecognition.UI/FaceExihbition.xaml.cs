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
        private List<Person> Data;

        public FaceExhibition()
		{
			InitializeComponent();
		}

		private async Task Exhibit()
		{
            await GetData();
            foreach (var person in Data)
				spGallery.Children.Add(new Painting(person));
		}

        private async Task GetData()
        {
            Data = await Core.MicrosoftAPIs.DataBaseAPI.PersonAPI.PersonAPIinstance.GetPersonList();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Exhibit();
        }
    }
}
