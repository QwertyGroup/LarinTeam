using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FaceRecognation._1._0
{
	public partial class windowSelectFace : Window
	{
		public windowSelectFace()
		{
			InitializeComponent();
		}

		private List<System.Drawing.Image> _faces;
		private ImageProcessing _imgProcessing = ImageProcessing.ImageProcessingInstance;
		public windowSelectFace(List<System.Drawing.Image> faces) : this()
		{
			_faces = faces;
			Loaded += (s, e) => DisplayFaces();
		}

		private void DisplayFaces()
		{
			foreach (var face in _faces)
			{
				wpFaces.Children.Add(GenerateFace(face));
			}
		}

		public event EventHandler<OnFaceSelectedEventArgs> OnFaceSelected;
		public class OnFaceSelectedEventArgs : EventArgs
		{
			public System.Drawing.Image Face { get; private set; }
			private OnFaceSelectedEventArgs() { }
			public OnFaceSelectedEventArgs(System.Windows.Controls.Image img)
			{
				Face = ImageProcessing.ImageProcessingInstance.ConvertBitmapImageToImage((BitmapImage)img.Source);
			}

		}
		private System.Windows.Controls.Image GenerateFace(System.Drawing.Image face)
		{
			var f = new System.Windows.Controls.Image
			{
				Source = _imgProcessing.ConvertImageToBitmapImage(face),
				Margin = new Thickness(6),
				Width = 200,
				Height = 200
			};
			f.MouseUp += (s, e) =>
			{
				OnFaceSelected?.Invoke(this, new OnFaceSelectedEventArgs(s as System.Windows.Controls.Image));
				Close();
			};
			return f;
		}
	}
}
