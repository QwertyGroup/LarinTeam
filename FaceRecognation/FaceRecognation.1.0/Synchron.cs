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

		private List<Face> getData()
		{
			FirebaseResponse response = Client.Get("Faces");
			List<Face> Data = response.ResultAs<List<Face>>();
			return Data == null ? new List<Face>() : Data;
		}
		private int getLastId()
		{
			return Data.Count;
		}

		public async Task<Face> AddFace(Face face)
		{
			SetResponse response = await Client.SetAsync($"Faces/{LastId}", face);
			LastId++;
			Data.Add(face);
			Face result = response.ResultAs<Face>();
			return result;
		}


	}

}
