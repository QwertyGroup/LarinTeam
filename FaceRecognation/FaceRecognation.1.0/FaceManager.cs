using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face;
using System.IO;
using System.Diagnostics;

namespace FaceRecognation._1._0
{
	class FaceManager
	{
		private readonly IFaceServiceClient _faceServiceClient;

		public FaceManager()
		{
			_faceServiceClient = new FaceServiceClient(GetMSKey());
		}

		public async Task<FaceRectangle[]> GetFaceRect(string filePath)
		{
			return await UploadAndDetectFaces(filePath);
		}

		private async Task<FaceRectangle[]> UploadAndDetectFaces(string imageFilePath)
		{
			try
			{
				using (Stream imageFileStream = File.OpenRead(imageFilePath))
				{
					var faces = await _faceServiceClient.DetectAsync(imageFileStream);
					var faceRects = faces.Select(face => face.FaceRectangle);
					return faceRects.ToArray();
				}
			}
			catch (Exception)
			{
				return new FaceRectangle[0];
			}
		}

		protected string GetMSKey()
		{
			try
			{
				var filename = "Keys.keys";
				if (!File.Exists(filename)) throw new Exception("File with keys does not exist((9(");
				return File.ReadLines(filename).ToArray()[1];
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return "no key";
			}
		}
	}
}
