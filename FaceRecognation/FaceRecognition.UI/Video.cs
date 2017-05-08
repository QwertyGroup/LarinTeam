using FaceRecognition.Core;

using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

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
            this.ImageValidatingPanel = ImageValidatingPanel;
        }

		public static readonly DependencyProperty VideoSelectedProperty =
			DependencyProperty.Register("VideoSelected", typeof(bool), typeof(Video));

		public Dictionary<int, List<Image>> _extractedFaces;
		public async Task ExtractFaces()
		{
			_extractedFaces = await VideoManager.VManagerInstance.getFacesFromVideo(Path);
		}

		public Dictionary<int, List<Image>> ValidFaces { get; set; }

        private System.Windows.Controls.WrapPanel ImageValidatingPanel;

        private System.Windows.Controls.Border createImage(Image img)
        {
            System.Windows.Controls.Border Border = new System.Windows.Controls.Border()
            {
                Width = 150,
                Height = 150,
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0))
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
                    brd.BorderThickness = new Thickness(2);
                else
                    brd.BorderThickness = new Thickness(0);
                MessageManager.MsgManagerInstance.writeMessage(brd.BorderThickness.Top.ToString());
            };
            return Border;
        }
        public int _num;
        public void loadNextPerson()
        {
            ImageValidatingPanel.Children.Clear();
            var currentPerson = _extractedFaces[_num];
            foreach(var faceImage in currentPerson)
            {
                ImageValidatingPanel.Children.Add(createImage(faceImage));
            }
            _num++;
        }

        public void ValidateFaces()
		{
            loadNextPerson();
		}
	}
}
