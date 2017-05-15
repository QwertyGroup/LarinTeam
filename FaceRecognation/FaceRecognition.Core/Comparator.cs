using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognition.Core
{
	public class Comparator
	{
		// Clear group
		// Add known people
		// Train
		// Compare detected with archive (firebase)

		private Comparator() { }
		private static Lazy<Comparator> _comparatorInstance = new Lazy<Comparator>(() => new Comparator());
		public static Comparator ComparatorInstance { get { return _comparatorInstance.Value; } }

		MessageManager _msgManager = MessageManager.MsgManagerInstance;
		//FaceApiManager _faceApiManager = FaceApiManager.FaceApiManagerInstance;
		//ImageProcessing _imgProcessing = ImageProcessing.ImageProcessingInstance;



		public async Task<Tuple<List<Person>, List<Person>>> SendDetectedPeopleToCompare(List<Person> videoPeople)
		{
			var newPeople = new List<Person>();
			var existedPeople = new List<Person>();
			//knownPeople = await AddKnownPeopleToGroup(knownPeople); // Зачем сейчас? Мы ее уже не чистим/ Добавлять если пустая
			for (int i = 0; i < videoPeople.Count; i++)
			{
				var person = videoPeople[i];
				//Sending photos to Microsoft and getting Id of each one
				await person.GetMicrosoftData();
				//person.Faces = person.Faces.Where(x => x.MicrosoftId != Guid.Empty).ToList();
				//Getting list of this faces
				var personFacesIds = person.Faces.Select(x => x.MicrosoftId).ToArray();
				//Sening list of faces and getting the result
				var iresult = await MicrosoftAPIs.ComparationAPI.Commands.CommandsInstance.Compare(personFacesIds);
				var isnew = false;

				iresult = iresult.Where(x => x.Candidates.Length != 0).ToList();
				isnew = iresult.Count == 0;

				if (isnew)
				{
					_msgManager.WriteMessage("New person.");
					newPeople.Add(person);
				}
				else
				{
					_msgManager.WriteMessage("Existed person.");
					existedPeople.Add(person);
				}
			}
			return Tuple.Create(existedPeople, newPeople);
		}
	}
}
