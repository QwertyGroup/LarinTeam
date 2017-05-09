﻿using System;
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
    //Base class of Face. Needed to convert dataBase data to c# objects.
	public class PrimitiveFace
	{
        public string _baseFaceImage;
        public int _id;

        public PrimitiveFace(string BaseFaceImage)
        {
			_baseFaceImage = BaseFaceImage;
        }

        public PrimitiveFace() { }

        public Face GetFaceFromPrimitive()
        {
            Face face = new Face(this._baseFaceImage);
            face._id = this._id;
            return face;
        }
	}
    //Complicated PrimitiveFace class. Can convert textBase image to Image and vice versa.
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
            this._id = -1;
            BaseFaceImage = ImageToBase(FaceImage);
        }

        public Face(string BaseFaceImage) : base()
        {
            this.BaseFaceImage = BaseFaceImage;
            this._id = -1;
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
        //Converting this Face to primitive.
        public PrimitiveFace GetPrimitiveFace()
        {
            PrimitiveFace primitiveFace = new PrimitiveFace(this.BaseFaceImage);
            primitiveFace._id = this._id;
            return primitiveFace;
        }
    }
}
