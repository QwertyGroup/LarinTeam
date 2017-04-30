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

namespace FaceRecognation._1._0
{
    public static class VideoManager
    {
        private static VideoServiceClient videoServiceClient = new VideoServiceClient(/*MSAPIManager.MSAPIManagerInstance.GetMSKey()*/"80c8c2aa437c4a30a76825d80efb2910 ");

        public static async void getOperationAsync(string filePath)
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
            Debug.WriteLine("END!");
        }
    }
}
