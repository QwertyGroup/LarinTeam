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

namespace FaceRecognition.UI.Gallery
{
	public partial class FacePainting : UserControl
	{
		public bool IsDeleteButtonVisible
		{
			set
			{
				if (value) deleteFaceButt.Visibility = Visibility.Visible;
				else deleteFaceButt.Visibility = Visibility.Hidden;
			}
		}

		public FacePainting()
		{
			InitializeComponent();
		}

		public FacePainting(System.Drawing.Image Face)
		{
			InitializeComponent();
			canvas.Background = new ImageBrush(Core.ImageProcessing.ImageProcessingInstance.ConvertImageToBitmapImage(Face));
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
