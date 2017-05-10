using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

using Newtonsoft;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognition.Core
{
	public class Synchron
	{
		//Singleton
		private static Lazy<Synchron> _syncInstance = new Lazy<Synchron>(() => new Synchron());
		public static Synchron Instance { get { return _syncInstance.Value; } }
		private Synchron()
		{
			AuthSecret = KeyManager.Instance.FireBaseKey;
			BasePath = "https://recognise-faces.firebaseio.com/";
			Config = new FirebaseConfig
			{
				AuthSecret = this.AuthSecret,
				BasePath = this.BasePath
			};
			Client = new FirebaseClient(Config);
			Data = GetData();
			LastId = GetLastId();
		}

		private string AuthSecret;
		private string BasePath;
		private IFirebaseConfig Config;
		private FirebaseClient Client;
		private int LastId = 0;
		private List<Person> Data;

        //Downloads all Data from FireBase and saves it to List<Face>
		private List<Person> GetData()
		{
			try
			{
				FirebaseResponse response = Client.Get("People");
				var PrimitiveData = response.ResultAs<List<PrimitivePerson>>();
				var Data = PrimitiveData.Select(x => x.GetPersonFromPrimitive()).ToList();
				return Data == null ? new List<Person>() : Data;
			}
			catch
			{
				return new List<Person>();
			}
		}
		private int GetLastId()
		{
			return Data.Count;
		}

		private void AddFace(Person person)
		{
            //face._id == -1 means that this face is new and should be added to the end of Data Base.
			if (person.Id == -1)
			{
				person.Id = LastId;
				SetResponse response = Client.Set($"People/{LastId}", person.GetPrimitive());
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					LastId++;
					Data.Add(person);
				}
			}
			else
			{
				FirebaseResponse response = Client.Update($"People/{person.Id}", person.GetPrimitive());
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					Data[person.Id] = person;
				}
			}
		}
        /*
		public async Task<SyncronResult> CheckAndThenAdd(Face face)
		{
			foreach (var otherFace in Data)
			{
				//float similarity = 0;
				var faceImage = face.FaceImage;
				var otherImage = otherFace.FaceImage;
				var similarity = await FaceApiManager.FaceApiManagerInstance.FindSimilar(faceImage, otherImage);
				if (similarity.Length == 0)
					continue;
				var confidence = similarity[0].Confidence;
				MessageManager.MsgManagerInstance.WriteMessage($"{face._id} and {otherFace._id} similarity - {confidence}");
				if (confidence > 0.4)
				{
					face._id = otherFace._id;
					AddFace(face);
					return new SyncronResult();
				}
			}
			AddFace(face);
			return new SyncronResult();
		}
        */
		public void Test()
		{
			Person Katya = new Person(new List<Image> {
                ImageProcessing.ImageProcessingInstance.LoadImageFromFile("ResultFaces/0.png"),
                ImageProcessing.ImageProcessingInstance.LoadImageFromFile("ResultFaces/1.png") });
			AddFace(Katya);
		}

	}

	public class SyncronResult
	{
	}
}
