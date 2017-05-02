using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Video;
using Microsoft.ProjectOxford.Video.Contract;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing;
using MediaToolkit;
using MediaToolkit.Options;
using MediaToolkit.Model;

namespace FaceRecognation._1._0
{
	public class VideoManager
	{
        //Singleton
        private VideoManager _instance;
        public VideoManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VideoManager();
                }
                return _instance;
            }
        }
        private VideoManager()
        {
            videoServiceClient = new VideoServiceClient(KeyManager.Instance.MsVideoKey);
            videoServiceClient.Timeout = TimeSpan.FromMinutes(15);
        }


        private VideoServiceClient videoServiceClient;
        //Количество тиков в секунду, майкрософт не хочет использовать тики, как в шарпе.
        private static double TimeScale;
		private static int VideoWidth;
		private static int VideoHeight;

		private async Task<FaceDetectionResult> getFaceDetectionAsync(string filePath)
		{
            //Создаем новую операцию в Каунтер Страйке
            Operation videoOperation;

            //Интересный факт - к слову стрим сложно придумать рифмочку.
            //Стрим = Обострим = Местерим = Обматерим, не так уж и трудно
            using (var fs = new FileStream(filePath, FileMode.Open))
			{
				videoOperation = await videoServiceClient.CreateOperationAsync(fs, new FaceDetectionOperationSettings());
			}
			OperationResult operationResult;
            //Очевидный цикл.
            while (true)
			{
				operationResult = await videoServiceClient.GetOperationResultAsync(videoOperation);
				if (operationResult.Status == OperationStatus.Succeeded || operationResult.Status == OperationStatus.Failed)
				{
					break;
				}
                //Экономим количество запросов.
                await Task.Delay(5000);
			}
			var faceDetectionTrackingResultJsonString = operationResult.ProcessingResult;
			var faceDetecionTracking = JsonConvert.DeserializeObject<FaceDetectionResult>(faceDetectionTrackingResultJsonString);

            TimeScale = faceDetecionTracking.Timescale;
			return faceDetecionTracking;
		}

		private Dictionary<int, List<CoolEvent>> getCoolEvents(FaceDetectionResult faceDetectionTracking)
		{
            //Почему майкрософт в свою программу не добавили эту строку? Зачем мне фрагменты без событий?
            var Fragments = faceDetectionTracking.Fragments.Where(x => x.Events != null).ToArray();
			var idDict = getDictionary(Fragments);
			return idDict;
		}

		private void getFrame(string path, double startTime, int id)
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

		private Dictionary<int, List<CoolEvent>> getDictionary(Fragment<FaceEvent>[] fragments)
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
                        faceEvent.rec.Height = Convert.ToInt32(VideoHeight * face.Height);
						faceEvent.rec.Width = Convert.ToInt32(VideoWidth * face.Width);
						faceEvent.rec.Left = Convert.ToInt32(VideoWidth * face.X);
						faceEvent.rec.Top = Convert.ToInt32(VideoHeight * face.Y);

						if (faceEvent.rec.Height + faceEvent.rec.Top > VideoHeight)
							faceEvent.rec.Height = VideoHeight - faceEvent.rec.Top;
						if (faceEvent.rec.Width + faceEvent.rec.Left > VideoWidth)
							faceEvent.rec.Width = VideoWidth - faceEvent.rec.Left;

						if (!dic.Keys.Contains(faceEvent.Id))
							dic[faceEvent.Id] = new List<CoolEvent>();
						dic[faceEvent.Id].Add(faceEvent);
					}
				}
			}
			return dic;
		}

        //Получаем разрешение видосика
		private void setVideoResol(string path)
		{
            //Это просто, ебать, говноМагия говнокода. Одним камушком два кувшинчика ёбнул.
            //Беру, во-первых, разрешение (Выебать твою маман, ухухух). Во-вторых, создаю скриншот первого фрейма видоса.
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
                VideoWidth = testImage.Width;
                //ВидеоВысота = тестКартинка.Высота.
                VideoHeight = testImage.Height;
                //Это была миниатюра о том, как Американцы воспринимают си шарп, и программування в целом.
            }
        }

		public async Task<Dictionary<int, List<Image>>> getFacesFromVideo(string path)
		{
            //Получаем разрешение
			setVideoResol(path);

            //Отправляем видосик
            FaceDetectionResult faceDetectionResult = await getFaceDetectionAsync(path);

            //Радуемся ответу, как будто это ответ от Кибернетики про олимпиаду.
            Debug.WriteLine("Got FDR!!!!)))");

            //Получаем список крутыхСобытий на каждого человека
            Dictionary<int, List<CoolEvent>> FaceIds = getCoolEvents(faceDetectionResult);
			Dictionary<int, List<Image>> resultImages = new Dictionary<int, List<Image>>();

			//Choose 1 first and 4 random CoolEvent - Гы, инглишь завтра просто первой парой, нужно попрактиковаться
			FaceIds = chooseFive(FaceIds);

            //Тут все очевидно
            foreach (int id in FaceIds.Keys)
			{
				resultImages[id] = new List<Image>();
				foreach (var curEvent in FaceIds[id])
				{
					var startTimeMili = curEvent.startTime / TimeScale * 1000;
					getFrame(path, startTimeMili, id);

					var img = ImageProcessing.ImageProcessingInstance.LoadImageFromFile($@"TempData/{id}.{(long)startTimeMili}.png");
					img = ImageProcessing.ImageProcessingInstance.CropImage(img, curEvent.rec);
					//ImageProcessing.ImageProcessingInstance.SaveImageToFile($@"TempData/{id}.{(long)startTimeMili}Face", img, System.Drawing.Imaging.ImageFormat.Png);
					resultImages[id].Add(img);
				}
			}
			return resultImages;
		}

		private Dictionary<int, List<CoolEvent>> chooseFive(Dictionary<int, List<CoolEvent>> faceIds)
		{
			var coolIds = new Dictionary<int, List<CoolEvent>>();

            //Я знаю, ты любишь шалить, вот тебе немного рандома, он поласкает твою простату.
            Random rnd = new Random();

			foreach (var id in faceIds.Keys)
			{
				coolIds[id] = new List<CoolEvent>();
				if (faceIds[id].Count > 4)
				{
					//First лет ми тейк э селфи ТУТУТУТУТ
					coolIds[id].Add(faceIds[id].FirstOrDefault());

					//Я знаю, ты люишь комбинаторику, посчитай, какая вероятность совпадения хотя бы двух фоток при 100 евентах.
					for (int i = 0; i < 4; i++)
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
