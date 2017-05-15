using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaceRecognition.UI
{
	public partial class RecognitionProgress : UserControl
	{
		public ProgressBar PBar { get { return pbProgressBar; } }
		public double Progress { get { return pbProgressBar.Value; } set { pbProgressBar.Value = value; } }
		public double IncreaseProgressBy { set { pbProgressBar.Value += value; } }
		public string TStatus
		{
			get { return tbStatusBlack.Text; }
			set
			{
				tbStatusBlack.Text = value;
				tbStatusWhite.Text = value;
			}
		}
		public RecognitionProgress()
		{
			InitializeComponent();
		}
	}
}
