﻿using FireSharp;
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
			try
			{
				FirebaseResponse response = Client.Get("Faces");
				var PrimitiveData = response.ResultAs<List<PrimitiveFace>>();
				var Data = PrimitiveData.Select(x => x.getFaceFromPrimitive()).ToList();
				return Data == null ? new List<Face>() : Data;
			}
			catch
			{
				return new List<Face>();
			}
		}
		private int getLastId()
		{
			return Data.Count;
		}

		private void AddFace(Face face)
		{
			if (face.id == -1)
			{
				face.id = LastId;
				SetResponse response = Client.Set($"Faces/{LastId}", face.getPrimitiveFace());
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					LastId++;
					Data.Add(face);
				}
			}
			else
			{
				FirebaseResponse response = Client.Update($"Faces/{face.id}", face.getPrimitiveFace());
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					Data[face.id] = face;
				}
			}
		}

		public async Task<SyncronResult> checkAndThenAdd(Face face)
		{
			foreach (var otherFace in Data)
			{
				//float similarity = 0;
				var faceImage = face.FaceImage;
				var otherImage = otherFace.FaceImage;
				var similarity = await FaceApiManager.MSAPIManagerInstance.FindSimilar(faceImage, otherImage);
				if (similarity.Length == 0)
					continue;
				var confidence = similarity[0].Confidence;
				MessageManager.MsgManagerInstance.WriteMessage($"{face.id} and {otherFace.id} similarity - {confidence}");
				if (confidence > 0.4)
				{
					face.id = otherFace.id;
					AddFace(face);
					return new SyncronResult();
				}
			}
			AddFace(face);
			return new SyncronResult();
		}

		public async void Test()
		{
			Face Katya = new Face(ImageProcessing.ImageProcessingInstance.LoadImageFromFile("ResultFaces/0.png"));
			Face Mark = new Face(ImageProcessing.ImageProcessingInstance.LoadImageFromFile("ResultFaces/0.png"));
			AddFace(Mark);
			AddFace(Katya);
			var result = await checkAndThenAdd(Mark);
			var result1 = await checkAndThenAdd(Katya);
		}

	}

	public class SyncronResult
	{
	}
}
