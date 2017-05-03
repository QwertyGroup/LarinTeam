using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FaceRecognation._1._0
{
	public partial class LogWindow : Window
	{
		public LogWindow()
		{
			InitializeComponent();

			Loaded += (s, e) =>
			{
				Clear();
				//Topmost = false;
				//var ow = Owner as MainWindow;
				//ow.Topmost = true;
				//ow.Focus();
				//ow.Topmost = false;
			};
		}

		public void AddMsg(string msg)
		{
			lbLog.Items.Add(msg);
			if (lbLog != null)
			{
				var border = (Border)VisualTreeHelper.GetChild(lbLog, 0);
				var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
				scrollViewer.ScrollToBottom();
			}
		}

		public void Clear()
		{
			lbLog.Items.Clear();
			lbLog.Items.Add("Purest Log ever.");
		}
	}
}
