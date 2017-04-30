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
	public class MSAPIManager
	{
		//Singleton
		public static readonly MSAPIManager MSAPIManagerInstance = new MSAPIManager();

		private MSAPIManager()
		{
			_faceServiceClient = new FaceServiceClient(GetMSKey());
		}

		private readonly IFaceServiceClient _faceServiceClient;

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
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
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
				return "No MS key found";
			}
		}

		private const string PERSON_GROUP_ID = "0x00";
		private const string GROUP_NAME = "RecognisedFaces";
		private List<Guid> _faceGuids = new List<Guid>();
		private async void TEST()
		{
			await _faceServiceClient.CreatePersonGroupAsync(PERSON_GROUP_ID, GROUP_NAME);
			var ires = await _faceServiceClient.IdentifyAsync(PERSON_GROUP_ID, _faceGuids.ToArray());
			//ires[1].Candidates
			//_faceServiceClient.AddFaceToFaceListAsync();
		}
	}
}
