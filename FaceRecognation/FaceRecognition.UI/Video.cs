using FaceRecognition.Core;
using FaceRecognition.UI.Galley;

using Microsoft.ProjectOxford.Face.Contract;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FaceRecognition.UI
{
	public class Video : DependencyObject
	{
		private string _path;
		public string Path
		{
			get { return _path; }
			set
			{
				_path = value;
				if (_path != string.Empty) VideoSelected = true;
				else VideoSelected = false;
			}
		}

		public bool VideoSelected
		{
			get { return (bool)GetValue(VideoSelectedProperty); }
			set { SetValue(VideoSelectedProperty, value); }
		}

		public Video(System.Windows.Controls.WrapPanel ImageValidatingPanel)
		{
			_imageValidatingPanel = ImageValidatingPanel;
		}

		public static readonly DependencyProperty VideoSelectedProperty =
			DependencyProperty.Register("VideoSelected", typeof(bool), typeof(Video));

		public Dictionary<int, GPerson> GPeople { get; set; } = new Dictionary<int, GPerson>();

		private Dictionary<int, List<System.Drawing.Image>> _extractedFaces;
		public Dictionary<int, List<System.Drawing.Image>> ExtractedFaces
		{
			get { return _extractedFaces; }
			set { _extractedFaces = value; if (_extractedFaces.Count != 0) AreFacesExtracted = true; else AreFacesExtracted = false; }
		}

		public bool AreFacesExtracted
		{
			get { return (bool)GetValue(AreFacesExtractedProperty); }
			set { SetValue(AreFacesExtractedProperty, value); }
		}
		public static readonly DependencyProperty AreFacesExtractedProperty =
			DependencyProperty.Register("AreFacesExtracted", typeof(bool), typeof(Video));


		public async Task ExtractFaces()
		{
			ExtractedFaces = await VideoManager.VManagerInstance.GetFacesFromVideo(Path);
			var curFaceCount = GPeople.Count;
			foreach (var exFace in ExtractedFaces)
				GPeople.Add(exFace.Key + curFaceCount, new GPerson { PersonLocalId = exFace.Key });
			_num = 0;
		}

		public Dictionary<int, List<System.Drawing.Image>> ValidFaces { get; set; } = new Dictionary<int, List<System.Drawing.Image>>();

		private System.Windows.Controls.WrapPanel _imageValidatingPanel;

		private int _selectedFaceCounter;
		private int _totalSelctedFaceCounter;
		private System.Windows.Controls.Border CreateImage(System.Drawing.Image img)
		{
			System.Windows.Controls.Border Border = new System.Windows.Controls.Border()
			{
				Width = 150,
				Height = 150,
				Margin = new Thickness(5),
				BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0))
			};
			System.Windows.Controls.Image Image = new System.Windows.Controls.Image
			{
				Source = ImageProcessing.ImageProcessingInstance.ConvertImageToBitmapImage(img),
				Width = 146,
				Height = 146,
				Stretch = System.Windows.Media.Stretch.Fill
			};
			Border.Child = Image;
			Border.MouseLeftButtonUp += (sender, e) =>
			{
				var brd = (System.Windows.Controls.Border)sender;
				if (brd.BorderThickness.Top == 0)
				{
					brd.BorderThickness = new Thickness(2);
					_selectedFaceCounter++;
					_totalSelctedFaceCounter++;
				}
				else
				{
					brd.BorderThickness = new Thickness(0);
					_selectedFaceCounter--;
					_totalSelctedFaceCounter--;
				}
			};
			_selectedFaceCounter = 0;
			return Border;
		}
		public int _num;
		public void LoadNextPerson()
		{
			_imageValidatingPanel.Children.Clear();
			if (ExtractedFaces.Count == 0)
			{
				MessageManager.MsgManagerInstance.WriteMessage("There are no faces on video.");
				return;
			}
			var currentPerson = ExtractedFaces[_num];
			foreach (var faceImage in currentPerson)
			{
				_imageValidatingPanel.Children.Add(CreateImage(faceImage));
			}
			_num++;
		}

		public void ValidateFaces()
		{
			LoadNextPerson();
		}

		private FaceApiManager _faceApiManager = FaceApiManager.FaceApiManagerInstance;
		public async Task AppendGroup(Button btn)
		{
			try
			{
				FillGP();

				btn.Content = "Identifying...";
				MessageManager.MsgManagerInstance.WriteMessage("Aggregating MS ids for comparing face request...");
				var listofGuidsToCompare = new List<Guid[]>();
				var rowGuidList = new List<Guid>();
				var personMatchGuid = new Dictionary<Guid, int>();
				await MakeTens(listofGuidsToCompare, rowGuidList, personMatchGuid);
				MessageManager.MsgManagerInstance.WriteMessage("Successfuly aggregated.");

				await TrainGroup();

				MessageManager.MsgManagerInstance.WriteMessage("Comparing new faces with archive...");
				List<IdentifyResult> result = await CompareTensWithArch(listofGuidsToCompare);

				var unrecognisedFacesGuids = result.Where(x => x.Candidates.Length == 0 || x.Candidates == null).ToList();
				if (unrecognisedFacesGuids.Count == 0)
				{
					MessageManager.MsgManagerInstance.WriteMessage("There is no new faces!");
					btn.Content = "Detect Faces";
					return;
				}

				var unrecognisedGPeople = await AddFacesToMSArchive(personMatchGuid, unrecognisedFacesGuids);
				foreach (var up in unrecognisedGPeople) // Danger zone
					GPeople.Add(GPeople.Count, up.Value);
				FileManager fm = new FileManagerJson();
				new FaceExhibition(unrecognisedGPeople).Show(); // Show new faces
				// Тут пишет Марк связку с БД и в ГПипл при создании нужно подгружать людей из БД
				MessageManager.MsgManagerInstance.WriteMessage("Comparing result recived!");
			}
			catch (Exception ex)
			{
				MessageManager.MsgManagerInstance.WriteMessage(ex.Message);
			}
			MessageManager.MsgManagerInstance.WriteMessage("Faces were processed!");
			btn.Content = "Detect Faces";
		}

		private async Task<Dictionary<int, GPerson>> AddFacesToMSArchive(Dictionary<Guid, int> personMatchGuid, List<IdentifyResult> unrecognisedFacesGuids)
		{
			var unrecognisedGPeople = new Dictionary<int, GPerson>();
			var addedPeopleLinking = new Dictionary<int, CreatePersonResult>();
			var currentPersonCounter = 0;
			var currentPerson = personMatchGuid[unrecognisedFacesGuids[0].FaceId]; // Может будет работать?)
			foreach (var unrecFace in unrecognisedFacesGuids)
			{
				try
				{
					var personId = personMatchGuid[unrecFace.FaceId];
					if (!addedPeopleLinking.ContainsKey(personId))
					{
						var res = await _faceApiManager.CreatePerson(personId.ToString());
						unrecognisedGPeople.Add(personId, new GPerson());
						MessageManager.MsgManagerInstance.WriteMessage($"Added {personId} to group.");
						addedPeopleLinking.Add(personId, res);
					}

					if (currentPerson == personId) currentPersonCounter++;
					else
					{
						currentPersonCounter = 1;
						currentPerson = personId;
					}
					await Task.Delay(TimeSpan.FromSeconds(5));
					var img = GPeople[personId].Faces[currentPersonCounter - 1].Img;
					unrecognisedGPeople[personId].Faces.Add(new GFace { Img = img });
					await _faceApiManager.AddPersonFace(addedPeopleLinking[personId],
						ImageProcessing.ImageProcessingInstance.ImageToStream(img));
					MessageManager.MsgManagerInstance.WriteMessage($"{currentPersonCounter}th face added to {personId} person.");
				}
				catch (Exception ex)
				{
					MessageManager.MsgManagerInstance.WriteMessage("Ex in AddFacesToMSArchive" + Environment.NewLine + ex.Message);
				}
			}
			return unrecognisedGPeople;
		}

		private async Task<List<IdentifyResult>> CompareTensWithArch(List<Guid[]> listofGuidsToCompare)
		{
			var result = new List<IdentifyResult>();
			try
			{
				foreach (var tens in listofGuidsToCompare)
					result.AddRange(await _faceApiManager.Identify(tens));
			}
			catch (Exception ex)
			{
				MessageManager.MsgManagerInstance.WriteMessage("Ex in CompareTensWithArch" + Environment.NewLine + ex.Message);
			}
			return result;
		}

		private async Task TrainGroup()
		{
			try
			{
				await _faceApiManager.TrainGroup();
				while (true)
				{
					var status = (await FaceApiManager.FaceApiManagerInstance.GetTrainStatus()).Status;
					if (status == Status.Succeeded)
					{
						break;
					}
					MessageManager.MsgManagerInstance.WriteMessage($"Status of training is {status}. Trying again...");
					await Task.Delay(15000);
				}
			}
			catch (Exception ex)
			{
				MessageManager.MsgManagerInstance.WriteMessage("Ex in TrainGroup" + Environment.NewLine + ex.Message);
			}
		}

		private async Task MakeTens(List<Guid[]> listofGuidsToCompare, List<Guid> rowGuidList, Dictionary<Guid, int> personMatchGuid)
		{
			try
			{
				foreach (var person in GPeople)
				{
					foreach (var face in person.Value.Faces)
					{
						var id = (await _faceApiManager.DetectFace(
							ImageProcessing.ImageProcessingInstance.ImageToStream(face.Img)))[0].FaceId;
						personMatchGuid.Add(id, person.Key);
						rowGuidList.Add(id);
					}
				}
				for (int i = 0; i < rowGuidList.Count; i += 10)
				{
					listofGuidsToCompare.Add(rowGuidList.GetRange(i, Math.Min(10, rowGuidList.Count - i)).ToArray());
				}
			}
			catch (Exception ex)
			{
				MessageManager.MsgManagerInstance.WriteMessage("Ex in MakeTens" + Environment.NewLine + ex.Message);
			}
		}

		private void FillGP()
		{
			try
			{
				foreach (var vFace in ValidFaces) // fill GPersons with valid faces
					GPeople[vFace.Key].Faces = vFace.Value.Select(x => new GFace { Img = x }).ToList();
				ValidFaces = new Dictionary<int, List<System.Drawing.Image>>();

				for (int i = 0; i < GPeople.Count; i++) // Remove people with unvalid faces
					if (GPeople[i].Faces.Count == 0) GPeople.Remove(i);
			}
			catch (Exception ex)
			{
				MessageManager.MsgManagerInstance.WriteMessage("Ex in FillGP" + Environment.NewLine + ex.Message);
			}
		}
	}
}
