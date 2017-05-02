using System.Threading.Tasks;
using System.Windows;

namespace FaceRecognation._1._0
{
	public partial class LogWindow : Window
	{
		public LogWindow()
		{
			InitializeComponent();
			Loaded += (s, e) => Clear();
		}

		public void AddMsg(string msg)
		{
			lbLog.Items.Add(msg);
		}

		public void Clear()
		{
			lbLog.Items.Clear();
			lbLog.Items.Add("Purest Log ever.");
		}
	}
}
