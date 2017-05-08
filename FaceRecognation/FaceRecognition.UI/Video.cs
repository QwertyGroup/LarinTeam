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

		public static readonly DependencyProperty VideoSelectedProperty =
			DependencyProperty.Register("VideoSelected", typeof(bool), typeof(Video));

		private Dictionary<int, List<Image>> _extractedFaces;
		public async Task ExtractFaces()
		{
			_extractedFaces = await VideoManager.VManagerInstance.getFacesFromVideo(Path);
		}

		public Dictionary<int, List<Image>> ValidFaces { get; set; }
		public void ValidateFaces()
		{

		}
	}
}
