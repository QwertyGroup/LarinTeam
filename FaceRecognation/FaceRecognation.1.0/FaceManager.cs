using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace FaceRecognation._1._0
{
	class FaceManager
	{
		private readonly IFaceServiceClient _faceServiceClient;

		private FaceManager()
		{
			_faceServiceClient = new FaceServiceClient(GetMSKey());
		}

		private static FaceManager _fmInstance;
		public static FaceManager FaceManagerInstance
		{
			get
			{
				if (_fmInstance == null)
					_fmInstance = new FaceManager();
				return _fmInstance;
			}
		}

		private async Task<FaceRectangle[]> GetFaceRect(string filePath)
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

		private string GetMSKey()
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

		private void TEST()
		{
			var personGroupId = "0x00";
			var faceGuids = new List<Guid>();
			_faceServiceClient.CreatePersonGroupAsync(personGroupId, "RecognisedFaces");
			var ires = _faceServiceClient.IdentifyAsync(personGroupId, faceGuids.ToArray());
			//_faceServiceClient.AddFaceToFaceListAsync();
		}
	}
}
