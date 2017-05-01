using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public static class VideoManager
    {
        private static VideoServiceClient videoServiceClient = new VideoServiceClient(/*MSAPIManager.MSAPIManagerInstance.GetMSKey()*/"80c8c2aa437c4a30a76825d80efb2910 ");
        private static async Task<FaceDetectionResult> getFaceDetectionAsync(string filePath)
        {
            Operation videoOperation;
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                videoOperation = await videoServiceClient.CreateOperationAsync(fs, new FaceDetectionOperationSettings());
            }
            OperationResult operationResult;
            while (true)
            {
                operationResult = await videoServiceClient.GetOperationResultAsync(videoOperation);
                if (operationResult.Status == OperationStatus.Succeeded || operationResult.Status == OperationStatus.Failed)
                {
                    break;
                }

                Task.Delay(30000).Wait();
            }
            var faceDetectionTrackingResultJsonString = operationResult.ProcessingResult;
            var faceDetecionTracking = JsonConvert.DeserializeObject<FaceDetectionResult>(faceDetectionTrackingResultJsonString);
            getFacesCoordinates(faceDetecionTracking);
            return faceDetecionTracking;
        }
        private static Dictionary<int, Fragment<FaceEvent>> getFacesCoordinates(FaceDetectionResult faceDetectionTracking)
        {
            List<Image> resultFacec = new List<Image>();

            List<int> IDs = faceDetectionTracking.FacesDetected.Select(x => x.FaceId).ToList();

            var Fragments = faceDetectionTracking.Fragments.Where(x => x.Events != null).ToArray();

            var idDict = getIdDict(Fragments);
            return idDict;
        }

        public static List<Image> getFacesFromCoords(Dictionary<int, Fragment<FaceEvent>> faceCoordinates)
        {
            List<Image> CroppedFaces = new List<Image>();

           
            return CroppedFaces;
        }

        private static void getFrame(string path, long startTime, int id)
        {
            if (!Directory.Exists("TempData"))
                Directory.CreateDirectory("TempData");

            var inputFile = new MediaFile() { Filename = path };
            var outputFile = new MediaFile() { Filename = $@"TempData/{id}.png" };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);
                var options = new ConversionOptions() { Seek = TimeSpan.FromTicks(startTime) };
                engine.GetThumbnail(inputFile, outputFile, options);
            }
        }

        private static Dictionary<int, Fragment<FaceEvent>> getIdDict(Fragment<FaceEvent>[] fragments)
        {
            Dictionary<int, List<Fragment<FaceEvent>>> IdDict = new Dictionary<int, List<Fragment<FaceEvent>>>();
            foreach(var fragment in fragments)
            {
                for (int i = 0; i < fragment.Events.Length; i++)
                {
                    for (int j = 0; j < fragment.Events[i].Length; j++)
                    {
                        var curId = fragment.Events[i][j].Id;
                        if (!IdDict.Keys.Contains(curId))
                        {
                            IdDict[curId] = new List<Fragment<FaceEvent>>();
                        }
                        IdDict[curId].Add(fragment);
                    }
                }
            }
            var coolIdDict = new Dictionary<int, Fragment<FaceEvent>>();
            foreach (var key in IdDict.Keys)
            {
                coolIdDict[key] = IdDict[key][IdDict[key].Count / 2];
            }
            return coolIdDict;
        }

        public static async void getFacesFromVideo(string path)
        {
            FaceDetectionResult faceDetectionResult = await getFaceDetectionAsync(path);
            Dictionary<int, Fragment<FaceEvent>> FaceIds = getFacesCoordinates(faceDetectionResult);

            foreach (int id in FaceIds.Keys)
            {
                var curFragment = FaceIds[id];
                var startTime = curFragment.Start;
                getFrame(path, startTime, id);
            }
        }
    }
}
