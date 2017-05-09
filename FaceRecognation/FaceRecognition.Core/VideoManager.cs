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
		//Количество тиков в секунду, майкрософт не хочет использовать тики, как в шарпе.
		private static double _timeScale;
		private static int _videoWidth;
		private static int _videoHeight;

		private async Task<FaceDetectionResult> GetFaceDetectionAsync(string filePath)
		{
			//Создаем новую операцию в Каунтер Страйке
			Operation videoOperation;

			//Интересный факт - к слову стрим сложно придумать рифмочку.
			//Стрим = Обострим = Местерим = Обматерим, не так уж и трудно
			using (var fs = new FileStream(filePath, FileMode.Open))
			{
				videoOperation = await _videoServiceClient.CreateOperationAsync(fs, new FaceDetectionOperationSettings());
			}
			OperationResult operationResult;
			//Очевидный цикл.
			while (true)
			{
				MessageManager.MsgManagerInstance.WriteMessage("Getting operation result...");
				operationResult = await _videoServiceClient.GetOperationResultAsync(videoOperation);
				if (operationResult.Status == OperationStatus.Succeeded || operationResult.Status == OperationStatus.Failed)
				{
					break;
				}
				MessageManager.MsgManagerInstance.WriteMessage($"Status is {operationResult.Status}. Trying again...");
				//Экономим количество запросов.
				await Task.Delay(30000);
			}
			var faceDetectionTrackingResultJsonString = operationResult.ProcessingResult;
			var faceDetecionTracking = JsonConvert.DeserializeObject<FaceDetectionResult>(faceDetectionTrackingResultJsonString);

			_timeScale = faceDetecionTracking.Timescale;
			return faceDetecionTracking;
		}

		private Dictionary<int, List<CoolEvent>> GetCoolEvents(FaceDetectionResult faceDetectionTracking)
		{
			//Почему майкрософт в свою программу не добавили эту строку? Зачем мне фрагменты без событий?
			var Fragments = faceDetectionTracking.Fragments.Where(x => x.Events != null).ToArray();
			var idDict = GetDictionary(Fragments);
			return idDict;
		}

		private void GetFrame(string path, double startTime, int id)
		{
			//Создаем папочку, если её нет. Нужно же максимально засрать папку с екзешником.
			//Если что, удалять эту папку программа не будет.
			if (!Directory.Exists("TempData"))
				Directory.CreateDirectory("TempData");

			//Магия сторонних библиотек.
			var inputFile = new MediaFile() { Filename = path };
			var outputFile = new MediaFile() { Filename = $@"TempData/{id}.{(long)startTime}.png" };

			//Если написать юзинг русскими буквами, получиться гыштп, почему-то я упорно читаю тут слово гыштюпок.
			using (var engine = new Engine())
			{
				//Метадата, приятно, что сторонняя библиотека не забывает делать отсылочки на прекрасный сериал Флеш.
				engine.GetMetadata(inputFile);

				//Вырезать фрейм из видоса на таком-то времени (Семью сунитов тоже вырезает, но неявно)
				var options = new ConversionOptions() { Seek = TimeSpan.FromMilliseconds(startTime) };
				//Thumbnail Thu-Thu-Thumbnail Thumbnail Thu-Thu-Thumbnail - новый трек драконов
				engine.GetThumbnail(inputFile, outputFile, options);
			}
		}

		//Почему этот класс круче чем ты?
		class CoolEvent
		{
			//Программеры си шарпа умные люди, вот и додумай сам прикол про ФейсРектангл и прямоугольноголовых.
			public FaceRectangle rec = new FaceRectangle();
			//public veryLong LengthOfMark'sDick = Distance.FromLightYears(Distance.Constants.Dohuya);
			public long startTime;
			public int Id;
		}

		private Dictionary<int, List<CoolEvent>> GetDictionary(Fragment<FaceEvent>[] fragments)
		{
			//Ну давай же, добавь букву "к" к названию, будет так смешно))0)
			Dictionary<int, List<CoolEvent>> dic = new Dictionary<int, List<CoolEvent>>();

			foreach (var fragment in fragments)
			{
				var startTime = fragment.Start;
				var interval = fragment.Interval;

				for (int momentId = 0; momentId < fragment.Events.Length; momentId++)
				{
					//Этот цикл просматривает все моменты в интервале 3 сотых секунды или тип того,
					//Но почему-то он так и не нашел момента, где ты пишешь сразу нормальный код.
					long time = startTime + momentId * (long)interval;
					foreach (var face in fragment.Events[momentId])
					{
						CoolEvent faceEvent = new CoolEvent
						{
							Id = face.Id,
							startTime = time
						};

						//Равно такой диагональю, это потому что кол-во букв в словах уменьшается по одной. Красиво.
						faceEvent.rec.Height = Convert.ToInt32(_videoHeight * face.Height);
						faceEvent.rec.Width = Convert.ToInt32(_videoWidth * face.Width);
						faceEvent.rec.Left = Convert.ToInt32(_videoWidth * face.X);
						if (faceEvent.rec.Left < 0)
							faceEvent.rec.Left = 0;

						faceEvent.rec.Top = Convert.ToInt32(_videoHeight * face.Y);
						if (faceEvent.rec.Top < 0)
							faceEvent.rec.Top = 0;

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

		//Получаем разрешение видосика
		private void SetVideoResol(string path)
		{
			//Это просто, ебать, говноМагия говнокода. Одним камушком два кувшинчика ёбнул.
			//Беру, во-первых, разрешение (Выебать твою маман, ухухух). Во-вторых, создаю скриншот первого фрейма видоса.
			if (!Directory.Exists("TempData"))
				Directory.CreateDirectory("TempData");
			MediaFile inputFile = new MediaFile() { Filename = path };
			MediaFile testFile = new MediaFile() { Filename = "TempData/RandomScreen.png" };
			using (Engine eng = new Engine())
			{
				//Эта строка она как ты, юный читатель, вдали от крутых строк, которые тусуються вместе
				eng.GetMetadata(inputFile);

				//Не смей удалять эти пустые строки, чтобы заговнить шутку выше.

				var options = new ConversionOptions() { Seek = TimeSpan.FromSeconds(0) };
				eng.GetThumbnail(inputFile, testFile, options);
				Image testImage = ImageProcessing.ImageProcessingInstance.LoadImageFromFile("TempData/RandomScreen.png");
				//ВидеоШирина = тестКартинка.Ширина.
				_videoWidth = testImage.Width;
				//ВидеоВысота = тестКартинка.Высота.
				_videoHeight = testImage.Height;
				//Это была миниатюра о том, как Американцы воспринимают си шарп, и программування в целом.
			}
		}

		public async Task<Dictionary<int, List<Image>>> GetFacesFromVideo(string path)
		{
			//Получаем разрешение
			SetVideoResol(path);

			//Отправляем видосик
			FaceDetectionResult faceDetectionResult = await GetFaceDetectionAsync(path);

			//Радуемся ответу, как будто это ответ от Кибернетики про олимпиаду.
			MessageManager.MsgManagerInstance.WriteMessage("Got Face Detection Result!!!!)))");

			//Получаем список крутыхСобытий на каждого человека
			Dictionary<int, List<CoolEvent>> FaceIds = GetCoolEvents(faceDetectionResult);
			Dictionary<int, List<Image>> resultImages = new Dictionary<int, List<Image>>();

			//Choose 1 first and 4 random CoolEvent - Гы, инглишь завтра просто первой парой, нужно попрактиковаться
			FaceIds = ChooseFive(FaceIds);

			//Тут все очевидно
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

						MessageManager.MsgManagerInstance.WriteMessage(" ");
					}
				}
			}
			return resultImages;
		}

		private Dictionary<int, List<CoolEvent>> ChooseFive(Dictionary<int, List<CoolEvent>> faceIds)
		{
			var coolIds = new Dictionary<int, List<CoolEvent>>();

			//Я знаю, ты любишь шалить, вот тебе немного рандома, он поласкает твою простату.
			Random rnd = new Random();

			foreach (var id in faceIds.Keys)
			{
				coolIds[id] = new List<CoolEvent>();
				if (faceIds[id].Count > 5)
				{
					//First лет ми тейк э селфи ТУТУТУТУТ
					coolIds[id].Add(faceIds[id].FirstOrDefault());

					//Я знаю, ты люишь комбинаторику, посчитай, какая вероятность совпадения хотя бы двух фоток при 100 евентах.
					for (int i = 0; i < 5; i++)
					{
						var randomId = rnd.Next(1, faceIds[id].Count - 1);
						coolIds[id].Add(faceIds[id][randomId]);
					}
				}
				else
				{
					//Опять строка-отброс. Её даже одну изолировали, никого не напоминает?
					coolIds[id].AddRange(faceIds[id]);
				}
			}

			return coolIds;
		}
	}
}
