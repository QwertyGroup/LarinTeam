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
    public interface IRecognitionProgress
    {
        ProgressBar PBar { get; }
        bool IsPBVisible { get; set; }

        double Progress { get; set; }
        double IncreaseProgressBy { set; }
        double MaxProgress { set; }

        string TStatus { get; set; }

        void IncBy1();
    }

    public partial class RecognitionProgress : UserControl, IRecognitionProgress
    {
        public ProgressBar PBar { get { return pbProgressBar; } }
        public bool IsPBVisible
        {
            get
            {
                return (pbProgressBar.Visibility == Visibility.Visible) ? true : false;
            }
            set
            {
                var vis = (value) ? Visibility.Visible : Visibility.Hidden;
                pbProgressBar.Visibility = tbStatusBlack.Visibility = tbStatusWhite.Visibility = vis;
            }
        }

        public double Progress
        {
            get { return pbProgressBar.Value; }
            set
            {
                pbProgressBar.Value = value;
            }
        }
        public double IncreaseProgressBy { set { pbProgressBar.Value += value; } }
        public double MaxProgress { set { pbProgressBar.Maximum = value; } }

        public string TStatus
        {
            get { return tbStatusBlack.Text; }
            set
            {
                tbStatusBlack.Text = value;
                tbStatusWhite.Text = value;
            }
        }

        private double _pbWidth
        {
            get { return pbProgressBar.ActualWidth; }
        }

        private double _tbWidth
        {
            get { return tbStatusBlack.ActualWidth; }
        }

        public RecognitionProgress()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdateText();
            pbProgressBar.ValueChanged += (s, e) => UpdateText();

            // For testing
            MouseLeftButtonUp += (s, e) => IncreaseProgressBy = 1;
            MouseRightButtonUp += (s, e) => IncreaseProgressBy = -1;
        }

        private void UpdateText()
        {
            var currentBarPosition = _pbWidth / pbProgressBar.Maximum * pbProgressBar.Value;
            if (currentBarPosition > (_pbWidth - _tbWidth) / 2 &&
                currentBarPosition < (_pbWidth + _tbWidth) / 2) // in text area range
            {
                var rbarpos = currentBarPosition - (_pbWidth - _tbWidth) / 2;
                var tpos = rbarpos / _tbWidth;
                transparentPointer.Offset = tpos;
                blackPointer.Offset = transparentPointer.Offset + 0.0001;
            }
            else if (currentBarPosition < (_pbWidth - _tbWidth) / 2) // before text area
            {
                transparentPointer.Offset = -0.0001;
                blackPointer.Offset = transparentPointer.Offset + 0.0001;
            }
            else if (currentBarPosition > (_pbWidth + _tbWidth) / 2) // after text area
            {
                transparentPointer.Offset = 1 - 0.0001;
                blackPointer.Offset = transparentPointer.Offset + 0.0001;
            }
        }

        public void IncBy1()
        {
            IncreaseProgressBy = 1;
        }
    }
}
