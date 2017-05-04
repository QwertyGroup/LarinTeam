using System.Windows;

namespace FaceRecognation._1._0
{
	public class MessageManager
	{
		private static MessageManager _msgManagerInstance;
        public MainWindow mainWindow;
		public static MessageManager MsgManagerInstance
		{
			get
			{
				if (_msgManagerInstance == null) _msgManagerInstance = new MessageManager();
				return _msgManagerInstance;
			}
		}

		private MessageManager()
		{
		}

		public void WriteMessage(string msg)
		{
            mainWindow.lbLog.Items.Add(msg);
		}

		public void ClearLog()
		{
			mainWindow.lbLog.Items.Clear();
		}
	}
}
