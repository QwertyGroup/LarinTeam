using System.IO;

namespace FaceRecognation._1._0
{
	public class KeyManager
    {
        //Singleton
        private static KeyManager _instance;
        public static KeyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new KeyManager();
                }
                return _instance;
            }
        }

        public string MsPhotoKey { get; private set; }
        public string MsVideoKey { get; private set; }
        public string FireBaseKey { get; private set; }

        private KeyManager()
        {
            var keys = File.ReadAllLines("Keys.keys");
            FireBaseKey = keys[0];
            MsPhotoKey = keys[1];
            MsVideoKey = keys.Length == 3 ? keys[2] : null;
        }
    }
}
