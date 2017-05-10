using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognition.Core
{
	public class GroupManager
	{
		private GroupManager() { }
		private static Lazy<GroupManager> _groupManagerInstance = new Lazy<GroupManager>(() => new GroupManager());
		public static GroupManager GroupManagerInstance { get { return _groupManagerInstance.Value; } }

		FaceApiManager _faceApiManager = FaceApiManager.FaceApiManagerInstance;
		MessageManager _msgManager = MessageManager.MsgManagerInstance;
		ImageProcessing _imgProcessing = ImageProcessing.ImageProcessingInstance;

		public async Task Clear()
		{
			try
			{
				await _faceApiManager.DeleteGroup();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
			await _faceApiManager.CreatePersonGroup();
			_msgManager.WriteMessage("Group Created");
			var orsensId = await _faceApiManager.CreatePerson("Orsen");
			await _faceApiManager.AddPersonFace(orsensId,
				_imgProcessing.ImageToStream(_imgProcessing.LoadImageFromFile("Orsen.jpg")));
			_msgManager.WriteMessage("Face added.");
		}

		public async Task Train()
		{
			try
			{
				await _faceApiManager.TrainGroup();
				while (true)
				{
					var status = (await FaceApiManager.FaceApiManagerInstance.GetTrainStatus()).Status;
					if (status == Microsoft.ProjectOxford.Face.Contract.Status.Succeeded)
					{
						MessageManager.MsgManagerInstance.WriteMessage("Training finished.");
						break;
					}
					MessageManager.MsgManagerInstance.WriteMessage($"Status of training is {status}. Trying again...");
					await Task.Delay(15000);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Ex in TrainGroup" + Environment.NewLine + ex.Message);
			}
		}
	}
}
