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



		public async Task<List<Person>> SendDetectedPeopleToCompare(List<Person> videoPeople)
		{
			var newPeople = new List<Person>();
			//knownPeople = await AddKnownPeopleToGroup(knownPeople); // Зачем сейчас? Мы ее уже не чистим/ Добавлять если пустая
			for (int i = 0; i < videoPeople.Count; i++)
			{
				var person = videoPeople[i];
				await person.GetMicrosoftData();
				var personFacesIds = person.Faces.Select(x => x.MicrosofId).ToArray();
				var iresult = await MicrosoftAPIs.ComparationAPI.Commands.CommandsInstance.Identify(personFacesIds);
				var isnew = false;

				iresult = iresult.Where(x => x.Candidates.Length != 0).ToList();
				isnew = iresult.Count == 0;

				if (isnew)
				{
					_msgManager.WriteMessage("New person.");
					//knownPeople.Add(person); // Зачем? Хотя пусть будет. Хотя нет. Спросить у Марка
					newPeople.Add(person);
				}
				else
				{
					//var candidate = iresult.First().Candidates.First();
					//foreach (var kp in knownPeople)
					//	if (kp.MicrosoftPersonId == candidate.PersonId)
					//	{
					//		//kp.Faces.Add(ca)
					//		_msgManager.WriteMessage("Existed person.");
					//		break;
					//	}
				}
			}
			return newPeople;
		}
	}
}
