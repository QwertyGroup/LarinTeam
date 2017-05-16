using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;

using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Video;
using Microsoft.ProjectOxford.Video.Contract;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FaceRecognition.Core
{
	public class VideoManager
	{
		//Singleton
		private static Lazy<VideoManager> _vmInstance = new Lazy<VideoManager>(() => new VideoManager());
		public static VideoManager VManagerInstance { get { return _vmInstance.Value; } }

		private VideoManager()
		{
			_videoServiceClient = new VideoServiceClient(KeyManager.Instance.MsVideoKey);
			_videoServiceClient.Timeout = TimeSpan.FromMinutes(15);
		}


		private VideoServiceClient _videoServiceClient;
        //Microsoft's ticks per second
		private static double _timeScale;

		private static int _videoWidth;
		private static int _videoHeight;

        //Getting Face Detection Result - JSON object with face data
		private async Task<FaceDetectionResult> GetFaceDetectionAsync(string filePath)
		{
			Operation videoOperation;

			using (var fs = new FileStream(filePath, FileMode.Open))
			{
				videoOperation = await _videoServiceClient.CreateOperationAsync(fs, new FaceDetectionOperationSettings());
			}
			OperationResult operationResult;
            //Checking status of operation every 30 sec untill it's succeeded
			while (true)
			{
				MessageManager.MsgManagerInstance.WriteMessage("Getting operation result...");
				operationResult = await _videoServiceClient.GetOperationResultAsync(videoOperation);
				if (operationResult.Status == OperationStatus.Succeeded || operationResult.Status == OperationStatus.Failed)
				{
					break;
				}
				MessageManager.MsgManagerInstance.WriteMessage($"Status is {operationResult.Status}. Trying again...");
				await Task.Delay(30000);
			}
            //Getting JSON string
			var faceDetectionTrackingResultJsonString = operationResult.ProcessingResult;
            //Converting JSON to C# object
			var faceDetecionTracking = JsonConvert.DeserializeObject<FaceDetectionResult>(faceDetectionTrackingResultJsonString);
            //Getting timeScale from object
			_timeScale = faceDetecionTracking.Timescale;
			return faceDetecionTracking;
		}

		private Dictionary<int, List<CoolEvent>> GetCoolEvents(FaceDetectionResult faceDetectionTracking)
		{
            //Only fragments where Events are not null
			var Fragments = faceDetectionTracking.Fragments.Where(x => x.Events != null).ToArray();


			var idDict = GetDictionary(Fragments);
			return idDict;
		}

		private void GetFrame(string path, double startTime, int id)
		{
			if (!Directory.Exists("TempData"))
				Directory.CreateDirectory("TempData");
            var inputFile = new MediaFile() { Filename = path };
			var outputFile = new MediaFile() { Filename = $@"TempData/{id}.{(long)startTime}.png" };

			using (var engine = new Engine())
			{
                
				engine.GetMetadata(inputFile);

				var options = new ConversionOptions() { Seek = TimeSpan.FromMilliseconds(startTime) };
				engine.GetThumbnail(inputFile, outputFile, options);
			}
		}

		class CoolEvent
		{
			//Coordinates of face
			public FaceRectangle rec = new FaceRectangle();
			//StartTime of this moment
			public long startTime;
            //Person Id
			public int Id;
		}

		private Dictionary<int, List<CoolEvent>> GetDictionary(Fragment<FaceEvent>[] fragments)
		{
			Dictionary<int, List<CoolEvent>> dic = new Dictionary<int, List<CoolEvent>>();

			foreach (var fragment in fragments)
			{
				var startTime = fragment.Start;
				var interval = fragment.Interval;

				for (int momentId = 0; momentId < fragment.Events.Length; momentId++)
				{
					//Time of current fragment
					long time = startTime + momentId * (long)interval;
					foreach (var face in fragment.Events[momentId])
					{
						CoolEvent faceEvent = new CoolEvent
						{
							Id = face.Id,
							startTime = time
						};

						//Microsoft returns coordinates within range [0; 1], where 0 = 0, 1 = full width
						faceEvent.rec.Height = Convert.ToInt32(_videoHeight * face.Height);
						faceEvent.rec.Width = Convert.ToInt32(_videoWidth * face.Width);
						faceEvent.rec.Left = Convert.ToInt32(_videoWidth * face.X);
                        //Sometimes it returns negative numbers
						if (faceEvent.rec.Left < 0)
							faceEvent.rec.Left = 0;

						faceEvent.rec.Top = Convert.ToInt32(_videoHeight * face.Y);
						if (faceEvent.rec.Top < 0)
							faceEvent.rec.Top = 0;

                        //Sometimes top right corner or bottom left corner outside the coordinates
						if (faceEvent.rec.Height + faceEvent.rec.Top > _videoHeight)
							faceEvent.rec.Height = _videoHeight - faceEvent.rec.Top;
						if (faceEvent.rec.Width + faceEvent.rec.Left > _videoWidth)
							faceEvent.rec.Width = _videoWidth - faceEvent.rec.Left;

						if (!dic.Keys.Contains(faceEvent.Id))
							dic[faceEvent.Id] = new List<CoolEvent>();
						dic[faceEvent.Id].Add(faceEvent);
					}
				}
			}
			return dic;
		}

        //Getting video resolution (Width and Height)
		private void SetVideoResol(string path)
		{
			if (!Directory.Exists("TempData"))
				Directory.CreateDirectory("TempData");
			MediaFile inputFile = new MediaFile() { Filename = path };
			MediaFile testFile = new MediaFile() { Filename = "TempData/RandomScreen.png" };
			using (Engine eng = new Engine())
			{
				eng.GetMetadata(inputFile);
				var options = new ConversionOptions() { Seek = TimeSpan.FromSeconds(0) };
				eng.GetThumbnail(inputFile, testFile, options);
				Image testImage = ImageProcessing.ImageProcessingInstance.LoadImageFromFile("TempData/RandomScreen.png");
				_videoWidth = testImage.Width;
				_videoHeight = testImage.Height;
			}
		}

		public async Task<Dictionary<int, List<Image>>> GetFacesFromVideo(string path)
		{
			SetVideoResol(path);
			FaceDetectionResult faceDetectionResult = await GetFaceDetectionAsync(path);
			MessageManager.MsgManagerInstance.ReportProgress();
			MessageManager.MsgManagerInstance.WriteMessage("Got Face Detection Result!!!!)))");
			Dictionary<int, List<CoolEvent>> FaceIds = GetCoolEvents(faceDetectionResult);
			Dictionary<int, List<Image>> resultImages = new Dictionary<int, List<Image>>();

			//Choose 1 first and 5 random CoolEvents
			FaceIds = ChooseFive(FaceIds);

			//Cropping faces of each person. id - unique person's number
			foreach (int id in FaceIds.Keys)
			{
				resultImages[id] = new List<Image>();
				foreach (var curEvent in FaceIds[id])
				{
					try
					{
						var startTimeMili = curEvent.startTime / _timeScale * 1000;
						GetFrame(path, startTimeMili, id);

						var img = ImageProcessing.ImageProcessingInstance.LoadImageFromFile($@"TempData/{id}.{(long)startTimeMili}.png");
						img = ImageProcessing.ImageProcessingInstance.CropImage(img, curEvent.rec);
						//ImageProcessing.ImageProcessingInstance.SaveImageToFile($@"TempData/{id}.{(long)startTimeMili}Face", img, System.Drawing.Imaging.ImageFormat.Png);
						resultImages[id].Add(img);
					}
					catch
					{
						MessageManager.MsgManagerInstance.WriteMessage("Error with cropping");
					}
				}
			}
			MessageManager.MsgManagerInstance.ReportProgress();
			return resultImages;
		}

		private Dictionary<int, List<CoolEvent>> ChooseFive(Dictionary<int, List<CoolEvent>> faceIds)
		{
			var coolIds = new Dictionary<int, List<CoolEvent>>();
			Random rnd = new Random();
			foreach (var id in faceIds.Keys)
			{
				coolIds[id] = new List<CoolEvent>();
				if (faceIds[id].Count > 5)
				{
					coolIds[id].Add(faceIds[id].FirstOrDefault());

					for (int i = 0; i < 5; i++)
					{
						var randomId = rnd.Next(1, faceIds[id].Count - 1);
						coolIds[id].Add(faceIds[id][randomId]);
					}
				}
				else
				{
					coolIds[id].AddRange(faceIds[id]);
				}
			}

			return coolIds;
		}
	}
}
