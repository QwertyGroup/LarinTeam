using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;

namespace FaceRecognation._1._0
{
	public class Face
	{
		private List<Image> FaceImages = new List<Image>();
		public List<string> BaseImages = new List<string>();

		public Face()
		{
		}

		public static string ImageToBase(Image image, ImageFormat format)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				image.Save(ms, format);
				byte[] imagebytes = ms.ToArray();
				string base64String = Convert.ToBase64String(imagebytes);
				return base64String;
			}
		}

		public static Image BaseToImage(string base64String)
		{
			byte[] imageBytes = Convert.FromBase64String(base64String);
			using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
			{
				ms.Write(imageBytes, 0, imageBytes.Length);
				Image image = Image.FromStream(ms, true);
				return image;
			}
		}

	}
}
