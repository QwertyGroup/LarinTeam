using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;

namespace FaceRecognition.Core
{
	public class FaceImage
	{
		public string BaseImage;
		public Image Image;
		public Guid MicrosoftId;

		public FaceImage(string BaseImage, Guid MicrosoftId)
		{
			this.BaseImage = BaseImage;
			this.MicrosoftId = MicrosoftId;
		}

		public FaceImage(Image Image)
		{
			this.Image = Image;
			this.BaseImage = ImageToBase(Image);
			MicrosoftId = new Guid();
		}

		public FaceImage(string BaseImage)
		{
			this.BaseImage = BaseImage;
			this.Image = BaseToImage(BaseImage);
			MicrosoftId = new Guid();
		}

		private static string ImageToBase(Image image)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				image.Save(ms, ImageFormat.Png);
				byte[] imagebytes = ms.ToArray();
				string base64String = Convert.ToBase64String(imagebytes);
				return base64String;
			}
		}

		private static Image BaseToImage(string base64String)
		{
			byte[] imageBytes = Convert.FromBase64String(base64String);
			using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
			{
				ms.Write(imageBytes, 0, imageBytes.Length);
				Image image = Image.FromStream(ms, true);
				return image;
			}
		}

		public static async Task<Guid> ImageToMSID(Image img)
		{
			var Face = await MicrosoftAPIs.ComparationAPI.Commands.CommandsInstance.DetectFace(img);
			return Face.First().FaceId;
		}
	}

	//Complicated PrimitiveFace class. Can convert textBase image to Image and vice versa.
	public class Person
	{

		public List<FaceImage> Faces;
		public Guid MicrosoftPersonId;

		public Person(Guid msId)
		{
			MicrosoftPersonId = msId;
		}

		public Person(List<string> BaseFaces)
		{
			Faces = BaseFaces.Select(x => new FaceImage(x)).ToList();
		}

		public Person(List<Image> ImageFaces)
		{
			Faces = ImageFaces.Select(x => new FaceImage(x)).ToList();
		}

		public async Task GetMicrosoftData()
		{
			foreach (var face in Faces)
			{
				try
				{
					face.MicrosoftId = await FaceImage.ImageToMSID(face.Image);
					if (face.MicrosoftId == Guid.Empty)
						throw new Exception("Guid is empty.");
					await Task.Delay(6000);
				}
				catch
				{
					Debug.WriteLine("Problem with converting FaceImage to Microsoft.");
				}
			}
			Faces.RemoveAll(x => x.MicrosoftId == null || x.MicrosoftId == Guid.Empty);
		}
	}
}
