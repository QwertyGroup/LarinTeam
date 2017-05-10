using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FaceRecognition.Core
{
	public class FaceApiManager
	{
		//Singleton
		private static Lazy<FaceApiManager> _faceApiManager = new Lazy<FaceApiManager>(() => new FaceApiManager());
		public static FaceApiManager FaceApiManagerInstance { get { return _faceApiManager.Value; } }

		private FaceApiManager()
		{
			_faceServiceClient = new FaceServiceClient(KeyManager.Instance.MsPhotoKey);
		}

		private readonly IFaceServiceClient _faceServiceClient;
		private MessageManager _msgManager = MessageManager.MsgManagerInstance;

		private async void CreateFaceList(string faceListId, string faceListName)
		{
			try
			{
				await _faceServiceClient.CreateFaceListAsync(faceListId, faceListName);
			}
			catch (Exception ex)
			{
				MessageManager.MsgManagerInstance.WriteMessage(ex.Message);
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
				MessageManager.MsgManagerInstance.WriteMessage(ex.Message);
				return new AddPersistedFaceResult();
			}
		}

		public async Task<Microsoft.ProjectOxford.Face.Contract.Face[]> DetectFace(Stream imageAsStream)
		{
            Debug.WriteLine("Kek");
			try
			{
				var faces = await _faceServiceClient.DetectAsync(imageAsStream, true);
				if (faces.Length == 0) throw new Exception("FaceList is empty");
				return faces;
			}
			catch (Exception ex)
			{
				MessageManager.MsgManagerInstance.WriteMessage(ex.Message);
				return new Microsoft.ProjectOxford.Face.Contract.Face[0];
			}
		}

		private async Task<MSFace[]> GetFaceRectangle(Stream imageAsStream)
		{
			var faces = await DetectFace(imageAsStream);
			var MSFaceList = new List<MSFace>();
			foreach (var face in faces)
				MSFaceList.Add(new MSFace(null, face.FaceId, face.FaceRectangle));
			return MSFaceList.ToArray();
		}

		private async Task<SimilarPersistedFace[]> CheckForSimilarity(MSFace aMSFace, string faceListId)
		{
			try
			{
				var similarFaces = await _faceServiceClient.FindSimilarAsync(aMSFace.Id,
					faceListId, FindSimilarMatchMode.matchFace);
				if (similarFaces.Length == 0) throw new Exception("There is no similar faces");
				return similarFaces;
			}
			catch (Exception ex)
			{
				MessageManager.MsgManagerInstance.WriteMessage(ex.Message);
				return new SimilarPersistedFace[0];
			}
		}

		private async void DeleteFaceList(string faceListId)
		{
			try
			{
				await _faceServiceClient.DeleteFaceListAsync(faceListId);
			}
			catch (Exception ex)
			{
				MessageManager.MsgManagerInstance.WriteMessage(ex.Message);
			}
		}

		private class MSFace
		{
			public Image Face { get; private set; }
			public Guid Id { get; private set; }
			public FaceRectangle Rect { get; private set; }

			private MSFace() { }
			public MSFace(Image face, Guid id, FaceRectangle rect)
			{
				Face = face;
				Id = id;
				Rect = rect;
			}
		}

		private ImageProcessing _imgProcessing = ImageProcessing.ImageProcessingInstance;

		public async Task<SimilarPersistedFace[]> FindSimilar(Image original, Image candidate,
			bool areCropped = false)
		{
			return await FindSimilar(original, new Image[] { candidate }, areCropped);
		}

		public async Task<SimilarPersistedFace[]> FindSimilar(Image original, Image[] candidates,
			bool areCropped = false)
		{
			var facelistId = "atl_acidhouze";
			// Detecting original face
			var dd = await GetDetectionData(original);
			if (dd == null) throw new Exception("No face on original img.");
			var orgnl = dd.First();

			var cnds = new List<MSFace>();
			if (!areCropped)
			{
				// Cropping candidates if werent cropped before 
				foreach (var photo in candidates)
				{
					dd = await GetDetectionData(photo);
					if (dd == null) continue;
					cnds.AddRange(dd);
				}
				if (cnds.Count == 0) throw new Exception("No candidates faces");
			}
			else
				// Just adding cropped candidates 
				foreach (var c in candidates)
					cnds.Add(new MSFace(c, Guid.Empty, new FaceRectangle
					{
						Top = 0,
						Left = 0,
						Width = c.Width,
						Height = c.Height
					}));

			DeleteFaceList(facelistId);

			CreateFaceList(facelistId, "Limb");
			foreach (var pers in cnds)
				await AddFaceToFaceList(facelistId, _imgProcessing.ImageToStream(pers.Face));

			var compResult = await CheckForSimilarity(new MSFace(null, orgnl.Id, orgnl.Rect), facelistId);
			return compResult;
		}

		private async Task<List<MSFace>> GetDetectionData(Image img)
		{
			var result = new List<MSFace>();
			var faces = await GetFaceRectangle(_imgProcessing.ImageToStream(img));
			if (faces.Length == 0) return null;
			foreach (var face in faces)
			{
				var croppedFace = _imgProcessing.CropImage(img, face.Rect);
				result.Add(new MSFace(croppedFace, face.Id, face.Rect));
			}
			return result;
		}

		private string _personGroupId = "acquaintances";
		public async Task CreatePersonGroup()
		{
			await CreatePersonGroup(_personGroupId);
		}
		public async Task CreatePersonGroup(string customGroupId)
		{
			_msgManager.WriteMessage($"Creating group: {customGroupId}");
			await _faceServiceClient.CreatePersonGroupAsync(customGroupId, "BFS");
		}

		public async Task<CreatePersonResult> CreatePerson(string personName)
		{
			return await CreatePerson(_personGroupId, personName);
		}
		public async Task<CreatePersonResult> CreatePerson(string customGroupId, string personName)
		{
			_msgManager.WriteMessage($"Creating person: {personName}");
			return await _faceServiceClient.CreatePersonAsync(customGroupId, personName);
		}

		public async Task<AddPersistedFaceResult> AddPersonFace(CreatePersonResult personId, Stream faceImg)
		{
			return await AddPersonFace(_personGroupId, personId, faceImg);
		}
		public async Task<AddPersistedFaceResult> AddPersonFace(string customGroupId, CreatePersonResult personId, Stream faceImg)
		{
			_msgManager.WriteMessage("Adding face to person");
			return await _faceServiceClient.AddPersonFaceAsync(customGroupId, personId.PersonId, faceImg);
		}

		public async Task TrainGroup()
		{
			await TrainGroup(_personGroupId);
		}
		public async Task TrainGroup(string customGroupId)
		{
			_msgManager.WriteMessage("Group training had started");
			await _faceServiceClient.TrainPersonGroupAsync(customGroupId);
		}

		public async Task<TrainingStatus> GetTrainStatus()
		{
			return await GetTrainStatus(_personGroupId);
		}
		public async Task<TrainingStatus> GetTrainStatus(string customGroupId)
		{
			_msgManager.WriteMessage("Getting training status...");
			return await _faceServiceClient.GetPersonGroupTrainingStatusAsync(customGroupId);
		}

		public async Task DeleteGroup()
		{
			await DeleteGroup(_personGroupId);
		}
		public async Task DeleteGroup(string customGroupId)
		{
			_msgManager.WriteMessage("Deleting person group...");
			await _faceServiceClient.DeletePersonGroupAsync(customGroupId);
		}

		public async Task<List<IdentifyResult>> Identify(AddPersistedFaceResult[] faceIds)
		{
			return await Identify(_personGroupId, faceIds);
		}
		public async Task<List<IdentifyResult>> Identify(string customGroupId, AddPersistedFaceResult[] faceIds)
		{
			var ids = faceIds.Select(x => x.PersistedFaceId).ToArray();
			return await Identify(customGroupId, ids);
		}
		public async Task<List<IdentifyResult>> Identify(Guid[] faceIds)
		{
			return await Identify(_personGroupId, faceIds);
		}
		public async Task<List<IdentifyResult>> Identify(string customGroupId, Guid[] faceIds)
		{
			return (await _faceServiceClient.IdentifyAsync(customGroupId, faceIds)).ToList();
		}

	}
}
