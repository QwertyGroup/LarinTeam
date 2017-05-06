using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using System.IO;
using System.Diagnostics;
using FireSharp.Response;
using Newtonsoft;
using Newtonsoft.Json;
using System.Drawing;
namespace FaceRecognation._1._0
{
	public class Synchron
	{
        //Singleton
        private static Synchron _instance;
		public static Synchron Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Synchron();
                }
                return _instance;
            }
        }


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
			Data = getData();
			LastId = getLastId();
		}

		private string AuthSecret;
		private string BasePath;
		private IFirebaseConfig Config;
		private FirebaseClient Client;
		private int LastId = 0;
		private List<Face> Data;

        public class RootObject
        {
            public List<Face> Faces { get; set; }
        }

        private List<Face> getData()
		{
			FirebaseResponse response = Client.Get("");
			List<Face> Data = response.ResultAs<RootObject>().Faces;
			return Data == null ? new List<Face>() : Data.ToList();
		}
		private int getLastId()
		{
			return Data.Count;
		}

		private void AddFace(Face face)
		{
            face.id = LastId;
			SetResponse response = Client.Set($"Faces/{LastId}", face);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                LastId++;
                Data.Add(face);
            }
		}

        private Face UpdateFace(Face face)
        {
            FirebaseResponse response = Client.Update($"Faces/{face.id}", face);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception();
            }
            Face result = response.ResultAs<Face>();
            return result;
            
        }

        public void checkAndThenAdd(Face face)
        {
            foreach(var otherFace in Data)
            {
                float similarity = 0;
                //similarity = MSAPIManager.MSAPIManagerInstance.FindSimilar();
                if (similarity > 0.4)
                {
                    Face result = UpdateFace(face);
                    return;         
                }
            }
            AddFace(face);
        }

        public void Test()
        {
            Face Kostya = new Face(ImageProcessing.ImageProcessingInstance.LoadImageFromFile("ResultFaces/0.png"));
            Face Dima = new Face(ImageProcessing.ImageProcessingInstance.LoadImageFromFile("ResultFaces/1.png"));
            AddFace(Kostya);
            AddFace(Dima);
        }

	}

}
