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
	public class PrimitiveFace
	{
        public string BaseFaceImage;
        public int id;

        public PrimitiveFace(string BaseFaceImage)
        {
            this.BaseFaceImage = BaseFaceImage;
        }

        public PrimitiveFace() { }

        public Face getFaceFromPrimitive()
        {
            Face face = new Face(this.BaseFaceImage);
            face.id = this.id;
            return face;
        }
	}

    public class Face : PrimitiveFace
    {
        private Image _faceImage;
        public Image FaceImage
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
        public Face(Image FaceImage) : base()
        {
            this.FaceImage = FaceImage;
            this.id = -1;
            BaseFaceImage = ImageToBase(FaceImage);
        }

        public Face(string BaseFaceImage) : base()
        {
            this.BaseFaceImage = BaseFaceImage;
            this.id = -1;
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
        public PrimitiveFace getPrimitiveFace()
        {
            PrimitiveFace primitiveFace = new PrimitiveFace(this.BaseFaceImage);
            primitiveFace.id = this.id;
            return primitiveFace;
        }
    }
}
