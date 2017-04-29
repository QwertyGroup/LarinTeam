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

namespace FaceRecognation._1._0
{
    public class Synchron
    {
        //Singleton
        public static readonly Synchron Instance = new Synchron();


        private Synchron()
        {
            AuthSecret = getSecretCode();
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
        private int LastId;
        private List<Face> Data;

        private string getSecretCode()
        {
            try
            {
                var fileName = "Keys.keys";
                if (File.Exists(fileName))
                {
                    return File.ReadAllLines(fileName).First();
                }
                else
                {
                    throw new Exception("File with keys does not exist");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return "Null key";
            }
        }
        private List<Face> getData()
        {
            FirebaseResponse response = Client.Get("Faces");
            return response.ResultAs<List<Face>>();
        }
        private int getLastId()
        {
            return Data.Count;
        }

        private async Task<Face> getFace(int id)
        {
            FirebaseResponse response = await Client.GetAsync($"Faces/{id}");
            return response.ResultAs<Face>();
        }

        private async Task<Face> setFace(Face face)
        {
            SetResponse response = await Client.SetAsync($"Faces/{LastId}", face);
            LastId++;
            Data.Add(face);
            Face result = response.ResultAs<Face>();
            return result;
        }
    }
    public class Face
    {
        List<string> FaceImages;
    }
}