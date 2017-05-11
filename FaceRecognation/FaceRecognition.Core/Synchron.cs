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
		public List<Person> Data;

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

		private async Task AddFace(Person person)
		{
            //face._id == -1 means that this face is new and should be added to the end of Data Base.
			if (person.Id == -1)
			{
				person.Id = LastId;
				SetResponse response = await Client.SetAsync($"People/{LastId}", person.GetPrimitive());
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
       
		public async Task Test()
		{
            //Person Katya = new Person(new List<Image> {
            //             ImageProcessing.ImageProcessingInstance.LoadImageFromFile("ResultFaces/0.png"),
            //             ImageProcessing.ImageProcessingInstance.LoadImageFromFile("ResultFaces/1.png") });
            foreach (var person in Data)
            {
                await person.GetMicrosoftData();
            }
            Debug.WriteLine("Got MS Data");

        }
        public async Task SendKnownPeople(List<Person> KnownPeople)
        {
            Data = KnownPeople;
            for (int i = 0; i < KnownPeople.Count; i++)
            {
                await Instance.AddFace(KnownPeople[i]);
            }
        }

	}

	public class SyncronResult
	{
	}
}
