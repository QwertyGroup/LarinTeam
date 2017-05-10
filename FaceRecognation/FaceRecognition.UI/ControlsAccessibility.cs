using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FaceRecognition.UI
{
	public partial class MainWindow : Window
	{
		public class ControlsAccessibility
		{
			private ControlsAccessibility() { }
			private static Lazy<ControlsAccessibility> _controlsAccessibilityInstance = new Lazy<ControlsAccessibility>(() => new ControlsAccessibility());
			public static ControlsAccessibility ControlsAccessibilityInstance { get { return _controlsAccessibilityInstance.Value; } }

			public MainWindow MainWindowInstance { get; set; }

			public Visibility ValidFaceButtonsVisibility
			{
				get { return MainWindowInstance.ValidateFaceBut.Visibility; }
				set { MainWindowInstance.ValidateFaceBut.Visibility = value; }
			}

			public bool IsDetectFacesButtonEnabled
			{
				get { return MainWindowInstance.cmdDetectFaces.IsEnabled; }
				set { MainWindowInstance.cmdDetectFaces.IsEnabled = value; }
			}
		}
	}
}
