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

		private readonly IFaceServiceClient _faceServiceClient;

		private async void CreateFaceList(string faceListId, string faceListName)
		{
			try
			{
				await _faceServiceClient.CreateFaceListAsync(faceListId, faceListName);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		private async Task<AddPersistedFaceResult> AddFaceToFaceList(string faceListId, Stream imageAsStream)
		{
			try
			{
				var faceResult = await _faceServiceClient.AddFaceToFaceListAsync(faceListId, imageAsStream);
				if (faceResult == null) throw new Exception("AddPersistedFaceResult is null");
				return faceResult;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return new AddPersistedFaceResult();
			}
		}

		private async Task<Microsoft.ProjectOxford.Face.Contract.Face[]> DetectFace(Stream imageAsStream)
		{
			try
			{
				var faces = await _faceServiceClient.DetectAsync(imageAsStream);
				if (faces.Length == 0) throw new Exception("FaceList is empty");
				return faces;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return new Microsoft.ProjectOxford.Face.Contract.Face[0];
			}
		}

		public class FaceIdAndRect
		{
			public Guid FaceId { get; set; }
			public FaceRectangle FaceRect { get; set; }
			private FaceIdAndRect() { }
			public FaceIdAndRect(Guid faceId, FaceRectangle faceRectangle)
			{
				FaceId = FaceId;
				FaceRect = faceRectangle;
			}
		}

		private async Task<FaceIdAndRect[]> GetFaceRectangle(Stream imageAsStream)
		{
			var faces = await DetectFace(imageAsStream);
			var faceIdAndRectList = new List<FaceIdAndRect>();
			foreach (var face in await DetectFace(imageAsStream))
				faceIdAndRectList.Add(new FaceIdAndRect(face.FaceId, face.FaceRectangle));
			return faceIdAndRectList.ToArray();
		}

		private async Task<SimilarPersistedFace[]> CheckForSimilarity(FaceIdAndRect faceIdAndRect, string faceListId)
		{
			try
			{
				var similarFaces = await _faceServiceClient.FindSimilarAsync(faceIdAndRect.FaceId, faceListId);
				if (similarFaces.Length == 0) throw new Exception("There is no similar faces");
				return similarFaces;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return new SimilarPersistedFace[0];
			}
		}
	}
}
