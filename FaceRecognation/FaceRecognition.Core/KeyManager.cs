using System.IO;

namespace FaceRecognition.Core
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

        //Microsoft Face Api Key
        public string MsPhotoKey { get; private set; }
        
        //Microsoft Video Api Key
        public string MsVideoKey { get; private set; }

        //FireBase OAth Key
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
