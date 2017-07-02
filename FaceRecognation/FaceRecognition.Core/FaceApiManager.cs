using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FaceRecognition.Core.MicrosoftAPIs
{
    namespace ComparationAPI
    {
        public class Commands
        {
            private Commands() { }
            private static Lazy<Commands> _commandsInstance = new Lazy<Commands>(() => new Commands());
            public static Commands CommandsInstance { get { return _commandsInstance.Value; } }

            FaceApiManager _faceApiManager = FaceApiManager.FaceApiManagerInstance;

            public async Task<List<IdentifyResult>> Compare(List<Guid> faceIds)
            {
                return await Compare(faceIds.ToArray());
            }
            public async Task<List<IdentifyResult>> Compare(Guid[] faceIds)
            {
                return await _faceApiManager.Identify(faceIds);
            }

            public async Task<Face[]> DetectFace(Image image)
            {
                return await _faceApiManager.DetectFace(ImageProcessing.ImageProcessingInstance.ImageToStream(image, ImageFormat.Png));
            }

            public async Task<List<Face>> DetectFaceWithLandmarks(Image image)
            {
                return (await _faceApiManager.DetectFace(ImageProcessing.ImageProcessingInstance.ImageToStream(image, ImageFormat.Png), true)).ToList();
            }
        }
    }

    namespace DataBaseAPI
    {
        public class PersonAPI
        {
            private PersonAPI() { }
            private static Lazy<PersonAPI> _personAPIinstance = new Lazy<PersonAPI>(() => new PersonAPI());
            public static PersonAPI PersonAPIinstance { get { return _personAPIinstance.Value; } }

            FaceApiManager _faceApiManager = FaceApiManager.FaceApiManagerInstance;
            public async Task<Person> AddPerson()
            {
                var msid = (await _faceApiManager.CreatePerson()).PersonId;
                return new Person(msid);
            }
            public async Task<Person> AddPerson(List<Image> faces)
            {
                var msid = (await _faceApiManager.CreatePerson()).PersonId;
                var person = new Person(msid);
                var faceImgs = new List<FaceImage>();
                foreach (var face in faces)
                {
                    var faceImg = new FaceImage(face)
                    {
                        MicrosoftId = (await AddFaceToPerson(msid, face)).PersistedFaceId
                    };
                    faceImgs.Add(faceImg);
                }
                person.Faces = faceImgs;
                return person;
            }

            public async Task DeletePerson(Guid personId)
            {
                await _faceApiManager.DeletePerson(personId);
            }

            public async Task DeletePerson(Person person)
            {
                if (person.MicrosoftPersonId == null)
                {
                    throw new Exception("Person has no Microsoft GUID");
                }

                await DeletePerson(person.MicrosoftPersonId);
            }

            public async Task<AddPersistedFaceResult> AddFaceToPerson(Guid personId, Image faceImg)
            {
                return await _faceApiManager.AddPersonFace(personId, ImageProcessing.ImageProcessingInstance.ImageToStream(faceImg));
            }

            public async Task<Person> GetPerson(Guid personId)
            {
                var groupPerson = await _faceApiManager.GetPersonFromGroup(personId);
                var person = new Person(groupPerson.PersonId)
                {
                    Faces = await DownloadPersonFaces(groupPerson)
                };
                return person;
            }

            private async Task<List<FaceImage>> DownloadPersonFaces(Microsoft.ProjectOxford.Face.Contract.Person groupPerson)
            {
                var imgs = new List<FaceImage>();
                var fimages = new List<FaceImage>();

                foreach (var faceId in groupPerson.PersistedFaceIds)
                {
                    var fm = new FaceImage(await Synchron.Instance.GetFace(faceId))
                    {
                        MicrosoftId = faceId
                    };
                    imgs.Add(fm);
                }

                //foreach (var face in groupPerson.PersistedFaceIds.Select(async x =>
                //{
                //	var fm = new FaceImage(await Synchron.Instance.GetFace(x));
                //	fm.MicrosoftId = x;
                //	return fm;
                //}).ToList())
                //	imgs.Add(await face);
                return imgs;
            }

            public async Task<List<Person>> GetPersonList(System.Windows.Controls.ProgressBar progressBar)
            {
                var plist = await _faceApiManager.GetPersonsFromGroup();
                var result = new List<Person>();
                foreach (var groupPerson in plist)
                {
                    try
                    {
                        var person = new Person(groupPerson.PersonId)
                        {
                            Faces = await DownloadPersonFaces(groupPerson)
                        };
                        result.Add(person);
                    }
                    catch { }
                    progressBar.Value += 100 / plist.Count();
                }
                return result;
            }

            public async Task DeleteFace(Guid personId, Guid faceId)
            {
                await _faceApiManager.DeleteFace(personId, faceId);
            }

            public async Task<List<Person>> GetPersonList()
            {
                var plist = await _faceApiManager.GetPersonsFromGroup();
                var result = new List<Person>();
                foreach (var groupPerson in plist)
                {
                    try
                    {
                        var person = new Person(groupPerson.PersonId)
                        {
                            Faces = await DownloadPersonFaces(groupPerson)
                        };
                        result.Add(person);
                    }
                    catch { }
                }
                return result;
            }
        }

        public class GroupAPI
        {
            private GroupAPI() { }
            private static Lazy<GroupAPI> _groupAPIinstance = new Lazy<GroupAPI>(() => new GroupAPI());
            public static GroupAPI GroupAPIinstance { get { return _groupAPIinstance.Value; } }

            FaceApiManager _faceApiManager = FaceApiManager.FaceApiManagerInstance;
            MessageManager _msgManager = MessageManager.MsgManagerInstance;
            ImageProcessing _imgProcessing = ImageProcessing.ImageProcessingInstance;

            public async Task CreateGroup()
            {
                await _faceApiManager.CreatePersonGroup();
            }

            public async Task DeleteGroup()
            {
                await _faceApiManager.DeleteGroup();
            }

            public async Task Train()
            {
                try
                {
                    await _faceApiManager.TrainGroup();
                    while (true)
                    {
                        var status = (await FaceApiManager.FaceApiManagerInstance.GetTrainStatus()).Status;
                        if (status == Microsoft.ProjectOxford.Face.Contract.Status.Succeeded)
                        {
                            MessageManager.MsgManagerInstance.WriteMessage("Training finished.");
                            break;
                        }
                        MessageManager.MsgManagerInstance.WriteMessage($"Status of training is {status}. Trying again...");
                        await Task.Delay(15000);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Ex in TrainGroup" + Environment.NewLine + ex.Message);
                }
            }

            public async Task Clear()
            {
                try
                {
                    await _faceApiManager.DeleteGroup();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                await _faceApiManager.CreatePersonGroup();
                _msgManager.WriteMessage("Group Created");
                var orsensId = await _faceApiManager.CreatePerson("Orsen");
                await _faceApiManager.AddPersonFace(orsensId,
                    _imgProcessing.ImageToStream(_imgProcessing.LoadImageFromFile("Orsen.jpg")));
                _msgManager.WriteMessage("Face added.");
            }

            public async Task<List<Person>> AddPeople(List<Person> newPeople)
            {
                for (int i = 0; i < newPeople.Count(); i++)
                {
                    newPeople[i].MicrosoftPersonId = (await _faceApiManager.CreatePerson(i.ToString())).PersonId;
                    for (int j = 0; j < newPeople[i].Faces.Count; j++)
                    {
                        newPeople[i].Faces[j].MicrosoftId = (
                            await _faceApiManager.AddPersonFace(newPeople[i].MicrosoftPersonId,
                            _imgProcessing.ImageToStream(newPeople[i].Faces[j].Image))).PersistedFaceId;
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
                await Train();
                return newPeople;
            }

            public async Task<int> Count()
            {
                return await _faceApiManager.GetGroupCount();
            }
        }
    }

    internal class FaceApiManager
    {
        //Singleton
        private static Lazy<FaceApiManager> _faceApiManager = new Lazy<FaceApiManager>(() => new FaceApiManager());
        public static FaceApiManager FaceApiManagerInstance { get { return _faceApiManager.Value; } }

        private FaceApiManager()
        {
            _faceServiceClient = new FaceServiceClient(KeyManager.Instance.MsPhotoKey);
        }

        private readonly IFaceServiceClient _faceServiceClient;
        private MessageManager _msgManager = MessageManager.MsgManagerInstance;

        private async void CreateFaceList(string faceListId, string faceListName)
        {
            try
            {
                await _faceServiceClient.CreateFaceListAsync(faceListId, faceListName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task<AddPersistedFaceResult> AddFaceToFaceList(string faceListId, Stream imageAsStream)
        {
            try
            {
                var faceResult = await _faceServiceClient.AddFaceToFaceListAsync(faceListId, imageAsStream);
                if (faceResult == null) throw new Exception("AddPersistedFaceResult is null");
                return faceResult;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new AddPersistedFaceResult();
            }
        }

        public async Task<Microsoft.ProjectOxford.Face.Contract.Face[]> DetectFace(Stream imageAsStream, bool landmarks = false)
        {
            try
            {
                var faces = await _faceServiceClient.DetectAsync(imageAsStream, returnFaceId: true, returnFaceLandmarks: landmarks);
                if (faces.Length == 0) throw new Exception("FaceList is empty");
                return faces;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new Microsoft.ProjectOxford.Face.Contract.Face[0];
            }
        }

        private async Task<MSFace[]> GetFaceRectangle(Stream imageAsStream)
        {
            var faces = await DetectFace(imageAsStream);
            var MSFaceList = new List<MSFace>();
            foreach (var face in faces)
                MSFaceList.Add(new MSFace(null, face.FaceId, face.FaceRectangle));
            return MSFaceList.ToArray();
        }

        private async Task<SimilarPersistedFace[]> CheckForSimilarity(MSFace aMSFace, string faceListId)
        {
            try
            {
                var similarFaces = await _faceServiceClient.FindSimilarAsync(aMSFace.Id,
                    faceListId, FindSimilarMatchMode.matchFace);
                if (similarFaces.Length == 0) throw new Exception("There is no similar faces");
                return similarFaces;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new SimilarPersistedFace[0];
            }
        }

        private async void DeleteFaceList(string faceListId)
        {
            try
            {
                await _faceServiceClient.DeleteFaceListAsync(faceListId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private class MSFace
        {
            public Image Face { get; private set; }
            public Guid Id { get; private set; }
            public FaceRectangle Rect { get; private set; }

            private MSFace() { }
            public MSFace(Image face, Guid id, FaceRectangle rect)
            {
                Face = face;
                Id = id;
                Rect = rect;
            }
        }

        private ImageProcessing _imgProcessing = ImageProcessing.ImageProcessingInstance;

        public async Task<SimilarPersistedFace[]> FindSimilar(Image original, Image candidate,
            bool areCropped = false)
        {
            return await FindSimilar(original, new Image[] { candidate }, areCropped);
        }

        public async Task<SimilarPersistedFace[]> FindSimilar(Image original, Image[] candidates,
            bool areCropped = false)
        {
            var facelistId = "atl_acidhouze";
            // Detecting original face
            var dd = await GetDetectionData(original);
            if (dd == null) throw new Exception("No face on original img.");
            var orgnl = dd.First();

            var cnds = new List<MSFace>();
            if (!areCropped)
            {
                // Cropping candidates if werent cropped before 
                foreach (var photo in candidates)
                {
                    dd = await GetDetectionData(photo);
                    if (dd == null) continue;
                    cnds.AddRange(dd);
                }
                if (cnds.Count == 0) throw new Exception("No candidates faces");
            }
            else
                // Just adding cropped candidates 
                foreach (var c in candidates)
                    cnds.Add(new MSFace(c, Guid.Empty, new FaceRectangle
                    {
                        Top = 0,
                        Left = 0,
                        Width = c.Width,
                        Height = c.Height
                    }));

            DeleteFaceList(facelistId);

            CreateFaceList(facelistId, "Limb");
            foreach (var pers in cnds)
                await AddFaceToFaceList(facelistId, _imgProcessing.ImageToStream(pers.Face));

            var compResult = await CheckForSimilarity(new MSFace(null, orgnl.Id, orgnl.Rect), facelistId);
            return compResult;
        }

        private async Task<List<MSFace>> GetDetectionData(Image img)
        {
            var result = new List<MSFace>();
            var faces = await GetFaceRectangle(_imgProcessing.ImageToStream(img));
            if (faces.Length == 0) return null;
            foreach (var face in faces)
            {
                var croppedFace = _imgProcessing.CropImage(img, face.Rect);
                result.Add(new MSFace(croppedFace, face.Id, face.Rect));
            }
            return result;
        }

        private string _personGroupId = "acquaintances";
        public async Task CreatePersonGroup()
        {
            await CreatePersonGroup(_personGroupId);
        }
        public async Task CreatePersonGroup(string customGroupId)
        {
            _msgManager.WriteMessage($"Creating group: {customGroupId}");
            await _faceServiceClient.CreatePersonGroupAsync(customGroupId, "BFS");
        }

        public async Task<CreatePersonResult> CreatePerson()
        {
            return await CreatePerson("Jackie Chan");
        }
        public async Task<CreatePersonResult> CreatePerson(string personName)
        {
            return await CreatePerson(_personGroupId, personName);
        }
        public async Task<CreatePersonResult> CreatePerson(string customGroupId, string personName)
        {
            _msgManager.WriteMessage($"Creating person: {personName}");
            return await _faceServiceClient.CreatePersonAsync(customGroupId, personName);
        }

        public async Task DeletePerson(Guid personId)
        {
            await DeletePerson(_personGroupId, personId);
        }
        public async Task DeletePerson(string customGroupId, Guid personId)
        {
            await _faceServiceClient.DeletePersonAsync(customGroupId, personId);
        }

        public async Task<AddPersistedFaceResult> AddPersonFace(Guid personId, Stream faceImg)
        {
            return await AddPersonFace(_personGroupId, personId, faceImg);
        }
        public async Task<AddPersistedFaceResult> AddPersonFace(CreatePersonResult personId, Stream faceImg)
        {
            return await AddPersonFace(_personGroupId, personId, faceImg);
        }
        public async Task<AddPersistedFaceResult> AddPersonFace(string customGroupId, CreatePersonResult personId, Stream faceImg)
        {
            return await AddPersonFace(customGroupId, personId.PersonId, faceImg);
        }
        public async Task<AddPersistedFaceResult> AddPersonFace(string customGroupId, Guid personId, Stream faceImg)
        {
            _msgManager.WriteMessage("Adding face to person");
            return await _faceServiceClient.AddPersonFaceAsync(customGroupId, personId, faceImg);
        }

        public async Task TrainGroup()
        {
            await TrainGroup(_personGroupId);
        }
        public async Task TrainGroup(string customGroupId)
        {
            _msgManager.WriteMessage("Group training had started");
            await _faceServiceClient.TrainPersonGroupAsync(customGroupId);
        }

        public async Task<TrainingStatus> GetTrainStatus()
        {
            return await GetTrainStatus(_personGroupId);
        }
        public async Task<TrainingStatus> GetTrainStatus(string customGroupId)
        {
            _msgManager.WriteMessage("Getting training status...");
            return await _faceServiceClient.GetPersonGroupTrainingStatusAsync(customGroupId);
        }

        public async Task DeleteGroup()
        {
            await DeleteGroup(_personGroupId);
        }
        public async Task DeleteGroup(string customGroupId)
        {
            _msgManager.WriteMessage("Deleting person group...");
            await _faceServiceClient.DeletePersonGroupAsync(customGroupId);
        }

        public async Task<List<IdentifyResult>> Identify(AddPersistedFaceResult[] faceIds)
        {
            return await Identify(_personGroupId, faceIds);
        }
        public async Task<List<IdentifyResult>> Identify(string customGroupId, AddPersistedFaceResult[] faceIds)
        {
            var ids = faceIds.Select(x => x.PersistedFaceId).ToArray();
            return await Identify(customGroupId, ids);
        }
        public async Task<List<IdentifyResult>> Identify(Guid[] faceIds)
        {
            return await Identify(_personGroupId, faceIds);
        }
        public async Task<List<IdentifyResult>> Identify(string customGroupId, Guid[] faceIds)
        {
            return (await _faceServiceClient.IdentifyAsync(customGroupId, faceIds)).ToList();
        }

        public async Task<Microsoft.ProjectOxford.Face.Contract.Person[]> GetPersonsFromGroup()
        {
            return await _faceServiceClient.GetPersonsAsync(_personGroupId);
        }

        public async Task<Microsoft.ProjectOxford.Face.Contract.Person> GetPersonFromGroup(Guid personId)
        {
            return await _faceServiceClient.GetPersonAsync(_personGroupId, personId);
        }

        public async Task<int> GetGroupCount()
        {
            return (await _faceServiceClient.GetPersonsAsync(_personGroupId)).Count();
        }

        public async Task DeleteFace(Guid personId, Guid faceId)
        {
            await _faceServiceClient.DeletePersonFaceAsync(_personGroupId, personId, faceId);
        }
    }
}
