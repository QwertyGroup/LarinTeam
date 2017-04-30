using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Video;
using Microsoft.ProjectOxford.Video.Contract;
using System.IO;

namespace FaceRecognation._1._0
{
	public static class VideoManager
	{
		private static VideoServiceClient videoServiceClient = new VideoServiceClient(Synchron.getSecretCode());

		private async void GetOperation(string filePath)
		{
			Operation videoOperation; // MARK KLASSNIY PAREN
			using (var fs = new FileStream(@"C:\Videos\Sample.mp4", FileMode.Open))
			{
				videoOperation = await videoServiceClient.CreateOperationAsync(fs, OperationType.Stabilize);
			}
		}
	}
}
