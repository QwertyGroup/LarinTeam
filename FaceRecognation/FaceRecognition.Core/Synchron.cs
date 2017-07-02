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
        }

        private string AuthSecret;
        private string BasePath;
        private IFirebaseConfig Config;
        private FirebaseClient Client;
        public Dictionary<Guid, string> Data;

        //Downloads all Data from FireBase and saves it to List<Face>
        public Dictionary<Guid, string> GetData()
        {
            try
            {
                FirebaseResponse response = Client.Get("Faces");
                var Data = response.ResultAs<Dictionary<Guid, string>>();
                return Data ?? new Dictionary<Guid, string>();
            }
            catch
            {
                return new Dictionary<Guid, string>();
            }
        }

        public async Task<Dictionary<Guid, string>> GetFaces()
        {
            try
            {
                FirebaseResponse response = await Client.GetAsync("Faces");
                var Data = response.ResultAs<Dictionary<Guid, string>>();
                return Data ?? new Dictionary<Guid, string>();
            }
            catch
            {
                return new Dictionary<Guid, string>();
            }
        }

        public async Task<List<Image>> GetFacesByList(List<Guid> ids)
        {
            List<Image> images = new List<Image>();
            foreach (var id in ids)
            {
                images.Add(await GetFace(id));
            }
            return images;
        }

        public async Task AddPeople(List<Person> newPeople)
        {
            foreach (var person in newPeople)
            {
                foreach (var face in person.Faces)
                {
                    await AddFace(face.MicrosoftId, face.BaseImage);
                }
            }
        }

        public async Task AddFace(Guid microsoftId, string img)
        {
            SetResponse response = await Client.SetAsync($"Faces/{microsoftId}", img);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Data.Add(microsoftId, img);
            }
        }

        public async Task DeleteFace(Guid microsoftId)
        {
            if (!Data.Keys.Contains(microsoftId))
            {
                throw new Exception("You're tryin' to delete id that doesn't exist");
            }
            FirebaseResponse response = await Client.DeleteAsync($"Faces/{microsoftId}");
            Data.Remove(microsoftId);
        }

        public async Task<Image> GetFace(Guid microsoftId)
        {
            FirebaseResponse response = await Client.GetAsync($"Faces/{microsoftId}");
            return new FaceImage(response.ResultAs<string>()).Image;
        }

        public async Task Test()
        {
            await AddFace(new Guid(12, 13, 12, 12, 12, 12, 228, 12, 12, 12, 12), "kek");
        }
    }
}
