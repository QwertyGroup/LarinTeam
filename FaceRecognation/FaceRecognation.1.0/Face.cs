using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognation._1._0
{
    public class Face
    {
        public List<string> FaceImages;

        public bool Equals(Face Another)
        {
            return this.FaceImages == Another.FaceImages;
        }
    }
}
