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
    //Base class of Face. Needed to convert dataBase data to c# objects.
    public class PrimitivePerson
    {
        public List<string> _baseFaceImage;
        public int Id;

        public PrimitivePerson(List<string> BaseFaceImage)
        {
            _baseFaceImage = BaseFaceImage;
        }

        public PrimitivePerson() { }

        public Person GetPersonFromPrimitive()
        {
            Person person = new Person(this._baseFaceImage);
            person.Id = this.Id;
            return person;
        }
    }

    public class FaceImage
    {
        public string BaseImage;
        public Image Image;
        public Guid MicrosofId;

        public FaceImage(string BaseImage)
        {
            this.BaseImage = BaseImage;
            this.Image = BaseToImage(BaseImage);
        }

        public FaceImage(Image Image)
        {
            this.Image = Image;
            this.BaseImage = ImageToBase(Image);
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
            var stream = ImageProcessing.ImageProcessingInstance.ImageToStream(img, ImageFormat.Png);
            var Face = await FaceApiManager.FaceApiManagerInstance.DetectFace(stream);
            return Face.First().FaceId;
        }
    }

    //Complicated PrimitiveFace class. Can convert textBase image to Image and vice versa.
    public class Person
    {
        public List<FaceImage> Faces;
        public int Id = -1;
        public Guid MicrosoftPersonId;

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
            foreach(var face in Faces)
            {
                try
                {
                    face.MicrosofId = await FaceImage.ImageToMSID(face.Image);
                    await Task.Delay(5000);
                }
                catch
                {
                    Debug.WriteLine("Problem with convertin FaceImage to Microsoft.");
                }
            }
            Faces.RemoveAll(x => x.MicrosofId == null);
        }

        public PrimitivePerson GetPrimitive()
        {
            PrimitivePerson primitive = new PrimitivePerson(Faces.Select(x => x.BaseImage).ToList());
            primitive.Id = this.Id;
            return primitive;
        }
    }
}
