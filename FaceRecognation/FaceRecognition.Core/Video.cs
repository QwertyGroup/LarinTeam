using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace FaceRecognition.Core
{
	public class Video
	{
		private Video() { }
		private static Lazy<Video> _videoInstance = new Lazy<Video>(() => new Video());
		public static Video VideoInstance { get { return _videoInstance.Value; } }

		private string _path;
		event EventHandler<bool> OnPathChanged;
		public string Path
		{
			get { return _path; }
			set
			{
				_path = value;
				OnPathChanged?.Invoke(this, _path != string.Empty);
			}
		}

		public async Task<List<List<Image>>> ExtractFaces()
		{
			if (Path == string.Empty) throw new Exception("Video path is empty");
			var people = new List<List<Image>>();

			var extractedFaces = await VideoManager.VManagerInstance.GetFacesFromVideo(Path);
			MessageManager.MsgManagerInstance.ReportProgress();
			foreach (var eface in extractedFaces)
				people.Add(eface.Value);
			return people;
		}

		public async Task<List<Person>> ExtractFacesA()
		{
			if (Path == string.Empty) throw new Exception("Video path is empty");
			var people = new List<Person>();
			var extractedFaces = await VideoManager.VManagerInstance.GetFacesFromVideo(Path);
			foreach (var eface in extractedFaces)
				people.Add(new Person(eface.Value));
			return people;
		}
	}
}
