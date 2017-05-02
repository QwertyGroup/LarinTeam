using System.Windows;

namespace FaceRecognation._1._0
{
	public class MessageManager
	{
		private static MessageManager _msgManagerInstance;
		public static MessageManager MsgManagerInstance
		{
			get
			{
				if (_msgManagerInstance == null) _msgManagerInstance = new MessageManager();
				return _msgManagerInstance;
			}
		}

		private LogWindow _logWindow;
		private MessageManager()
		{
			_logWindow = new LogWindow();
			_logWindow.ShowInTaskbar = false;
			_logWindow.Owner = Application.Current.MainWindow;
			_logWindow.Show();
		}

		public void WriteMessage(string msg)
		{
			_logWindow.AddMsg(msg);
		}

		public void ClearLog()
		{
			_logWindow.Clear();
		}
	}
}
