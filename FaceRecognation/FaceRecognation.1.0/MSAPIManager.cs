using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

namespace FaceRecognation._1._0
{
	public class MSAPIManager
	{
		//Singleton
		private static MSAPIManager _mSAPIManagerInstance;

		public static MSAPIManager MSAPIManagerInstance
		{
			get
			{
				if (_mSAPIManagerInstance == null)
				{
					_mSAPIManagerInstance = new MSAPIManager();
				}
				return _mSAPIManagerInstance;
			}
		}

		private MSAPIManager()
		{
			_faceServiceClient = new FaceServiceClient(KeyManager.Instance.MsPhotoKey);
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
				var faces = await _faceServiceClient.DetectAsync(imageAsStream, true);
				if (faces.Length == 0) throw new Exception("FaceList is empty");
				return faces;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return new Microsoft.ProjectOxford.Face.Contract.Face[0];
			}
		}

		private class FaceIdAndRect
		{
			public Guid FaceId { get; set; }
			public FaceRectangle FaceRect { get; set; }
			private FaceIdAndRect() { }
			public FaceIdAndRect(Guid faceId, FaceRectangle faceRectangle)
			{
				FaceId = faceId;
				FaceRect = faceRectangle;
			}
		}

		private async Task<FaceIdAndRect[]> GetFaceRectangle(Stream imageAsStream)
		{
			var faces = await DetectFace(imageAsStream);
			var faceIdAndRectList = new List<FaceIdAndRect>();
			foreach (var face in faces)
				faceIdAndRectList.Add(new FaceIdAndRect(face.FaceId, face.FaceRectangle));
			return faceIdAndRectList.ToArray();
		}

		private async Task<SimilarPersistedFace[]> CheckForSimilarity(FaceIdAndRect faceIdAndRect, string faceListId)
		{
			try
			{
				var similarFaces = await _faceServiceClient.FindSimilarAsync(faceIdAndRect.FaceId, faceListId, FindSimilarMatchMode.matchFace);
				if (similarFaces.Length == 0) throw new Exception("There is no similar faces");
				return similarFaces;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return new SimilarPersistedFace[0];
			}
		}

		public async void ATL_ACIDHOUZE(string faceListId)
		{
			try
			{
				await _faceServiceClient.DeleteFaceListAsync(faceListId);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		private ImageProcessing _imgProcessing = ImageProcessing.ImageProcessingInstance;
		public async Task<SimilarPersistedFace[]> FindSimilar(Image original, Image[] candidates, bool areCropped = false)
		{
			var croppedCandidatesFaces = new List<Image>();
			var faceIdAndRectList = new List<FaceIdAndRect>();
			if (!areCropped)
			{// Cropping candidates if wasnt cropped before 
				foreach (var photo in candidates)
				{
					var faces = await GetFaceRectangle(_imgProcessing.ImageToStream(photo));
					if (faces.Length == 0) continue;
					foreach (var face in faces)
					{
						var croppedFace = _imgProcessing.CropImage(photo, face.FaceRect);
						faceIdAndRectList.Add(face);
						croppedCandidatesFaces.Add(croppedFace);
					}
				}
			}
		}
	}
}
