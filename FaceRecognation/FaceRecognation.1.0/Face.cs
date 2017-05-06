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
        private Image _faceImage;
        private Image FaceImage
        {
            get
            {
                if (_faceImage == null)
                {
                    if (_baseFaceImage == null)
                    {
                        throw new Exception("FaceImage and BaseFaceImage are nulls");
                    }

                    _faceImage = BaseToImage(BaseFaceImage);
                }
                return _faceImage;
            }
            set
            {
                _faceImage = value;
            }
        }
        private string _baseFaceImage;
        public string BaseFaceImage
        {
            get
            {
                if (_baseFaceImage == null)
                {
                    if (_faceImage == null)
                    {
                        throw new Exception("FaceImage and BaseFaceImage are nulls");
                    }

                    _baseFaceImage = ImageToBase(FaceImage);
                }
                return _baseFaceImage;
            }
            private set
            {
                _baseFaceImage = value;
            }
        }
        public int id;

		public Face(Image FaceImage)
		{
            this.FaceImage = FaceImage;
            BaseFaceImage = ImageToBase(FaceImage);
		}

        public Face(string BaseFaceImage)
        {
            this.BaseFaceImage = BaseFaceImage;
            FaceImage = BaseToImage(BaseFaceImage);
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

	}
}
