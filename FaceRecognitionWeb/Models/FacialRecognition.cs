using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace FaceRecognitionWeb.Models
{
    public class FacialRecognition
    {
        // Replace the first parameter with your valid subscription key.        
        // Replace or verify the region in the second parameter.        
        // You must use the same region in your REST API call as you used to obtain your subscription keys.
        // For example, if you obtained your subscription keys from the westcentralus region, replace
        // "westus" in the URI below with "westcentralus".
        private IFaceServiceClient FaceServiceClient { get; } = new FaceServiceClient("key", "https://westus.api.cognitive.microsoft.com/face/v1.0");

        public async Task<Face[]> UploadAndDetectFacesStream(string imageBase64)
        {
            try
            {
                var converted = imageBase64.Replace('-', '+');
                converted = converted.Replace('_', '/');
                converted = converted.Replace("data:image/png;base64,", string.Empty);
                converted = converted.Replace("data:image/jpeg;base64,", string.Empty);                
                var stringBytes = Convert.FromBase64String(converted);
                                                    
                // The list of Face attributes to return.
                var faceAttributes = new[]
                {
                    FaceAttributeType.Gender,
                    FaceAttributeType.Age,
                    FaceAttributeType.Smile,
                    FaceAttributeType.Emotion,
                    FaceAttributeType.Glasses,
                    FaceAttributeType.Hair
                };

                // Call the Face API to detect the face(s)                    
                using (var imageFileStream = new MemoryStream(stringBytes))
                {
                    var faces = await FaceServiceClient.DetectAsync(imageFileStream, true, false, faceAttributes);
                    return faces;
                }                
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                Console.WriteLine(f.ErrorMessage, f.ErrorCode);
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                Console.WriteLine(e.Message, "Error");
                return new Face[0];
            }
        }
        public async Task<Face[]> UploadAndDetectFacesFromImageFile(string imageFilePath)
        {
            // The list of Face attributes to return.
            var faceAttributes = new[]
            {
                FaceAttributeType.Gender,
                FaceAttributeType.Age,
                FaceAttributeType.Smile,
                FaceAttributeType.Emotion,
                FaceAttributeType.Glasses,
                FaceAttributeType.Hair
            };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await FaceServiceClient.DetectAsync(imageFileStream, true, false, faceAttributes);
                    return faces;
                }
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                Console.WriteLine(f.ErrorMessage, f.ErrorCode);
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                Console.WriteLine(e.Message, "Error");
                return new Face[0];
            }
        }

        private string FaceDescription(Face face)
        {
            var sb = new StringBuilder();

            sb.Append("Face: ");

            // Add the gender, age, and smile.
            sb.Append(face.FaceAttributes.Gender);
            sb.Append(", ");
            sb.Append(face.FaceAttributes.Age);
            sb.Append(", ");
            sb.Append($"smile {face.FaceAttributes.Smile * 100:F1}%, ");

            // Add the emotions. Display all emotions over 10%.
            sb.Append("Emotion: ");
            var emotionScores = face.FaceAttributes.Emotion;
            if (emotionScores.Anger >= 0.1f) sb.Append($"anger {emotionScores.Anger * 100:F1}%, ");
            if (emotionScores.Contempt >= 0.1f) sb.Append($"contempt {emotionScores.Contempt * 100:F1}%, ");
            if (emotionScores.Disgust >= 0.1f) sb.Append($"disgust {emotionScores.Disgust * 100:F1}%, ");
            if (emotionScores.Fear >= 0.1f) sb.Append($"fear {emotionScores.Fear * 100:F1}%, ");
            if (emotionScores.Happiness >= 0.1f) sb.Append($"happiness {emotionScores.Happiness * 100:F1}%, ");
            if (emotionScores.Neutral >= 0.1f) sb.Append($"neutral {emotionScores.Neutral * 100:F1}%, ");
            if (emotionScores.Sadness >= 0.1f) sb.Append($"sadness {emotionScores.Sadness * 100:F1}%, ");
            if (emotionScores.Surprise >= 0.1f) sb.Append($"surprise {emotionScores.Surprise * 100:F1}%, ");

            // Add glasses.
            sb.Append(face.FaceAttributes.Glasses);
            sb.Append(", ");

            // Add hair.
            sb.Append("Hair: ");

            // Display baldness confidence if over 1%.
            if (face.FaceAttributes.Hair.Bald >= 0.01f)
                sb.Append($"bald {face.FaceAttributes.Hair.Bald * 100:F1}% ");

            // Display all hair color attributes over 10%.
            var hairColors = face.FaceAttributes.Hair.HairColor;
            foreach (var hairColor in hairColors)
            {
                if (!(hairColor.Confidence >= 0.1f)) continue;
                sb.Append(hairColor.Color);
                sb.Append($" {hairColor.Confidence * 100:F1}% ");
            }

            // Return the built string.
            return sb.ToString();
        }

        public async Task<bool> CreatePersonGroup(string personGroupId, string name)
        {
            try
            {
                await FaceServiceClient.CreatePersonGroupAsync(personGroupId, name);
                return true;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                return false;
            }
        }

        public async Task<CreatePersonResult> CreatePerson(string name, string personGroupId)
        {
            var employee = await FaceServiceClient.CreatePersonAsync(personGroupId, name);
            return employee;
        }
      
        private async void TrainPersonGroup(string personGroupId)
        {
            await FaceServiceClient.TrainPersonGroupAsync(personGroupId);
        }

        public async Task<List<Person>> IdentifyPersonInImage(string personGroupId, string imageBase64)
        {            
            try
            {
                var converted = imageBase64.Replace('-', '+');
                converted = converted.Replace('_', '/');
                converted = converted.Replace("data:image/png;base64,", String.Empty);
                converted = converted.Replace("data:image/jpeg;base64,", String.Empty);
                var stringBytes = Convert.FromBase64String(converted);
                using (Stream s = new MemoryStream(stringBytes))
                {
                    var faces = await FaceServiceClient.DetectAsync(s);
                    var faceIds = faces.Select(face => face.FaceId).ToArray();
                    var persons = new List<Person>();

                    var results = await FaceServiceClient.IdentifyAsync(personGroupId, faceIds);
                    foreach (var identifyResult in results)
                    {                        
                        if (identifyResult.Candidates.Length == 0)
                        {
                            Console.WriteLine("I'm sorry - we have not been able to verify your identity.");
                        }
                        else
                        {
                            // Get top 1 among all candidates returned
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await FaceServiceClient.GetPersonAsync(personGroupId, candidateId);                            
                            persons.Add(person);
                        }
                    }
                    return persons;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsPersonGroupTrained(string personGroupId)
        {
            var trainingStatus = await FaceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);
            return trainingStatus.Status != Status.Running;
        }

        public async Task<IEnumerable<Person>> GetPersonsInPersonGroup(string personGroupId)
        {
            var persons = await FaceServiceClient.ListPersonsAsync(personGroupId);
            return persons;            
        }

        public async void DeletePerson(string personId, string personGroupId)
        {
            try
            {
                var guid = Guid.Parse(personId);
                await FaceServiceClient.DeletePersonAsync(personGroupId, guid);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<PersonFace> GetPersistedFacesForPerson(string personId, string personGroupId, string persisitedFaceId)
        {
            var guid = Guid.Parse(personId);
            var guid2 = Guid.Parse(persisitedFaceId);
            var persistedFaces = await FaceServiceClient.GetPersonFaceAsync(personGroupId, guid, guid2);
            return persistedFaces;
        }

        public async Task<Person> GetPersonByName(string name, string personGroupId)
        {
            var persons = await FaceServiceClient.ListPersonsAsync(personGroupId);
            var person = persons.FirstOrDefault(x => x.Name == name);
            return person;
        }

        public async Task<AddPersistedFaceResult>AddFaceToPerson(string personId, string personGroupId, string imageBase64)
        {
            var converted = imageBase64.Replace('-', '+');
            converted = converted.Replace('_', '/');
            converted = converted.Replace("data:image/png;base64,", string.Empty);
            converted = converted.Replace("data:image/jpeg;base64,", string.Empty);
            var stringBytes = Convert.FromBase64String(converted);

            using (var s = new MemoryStream(stringBytes))
            {
                var guid = Guid.Parse(personId);
                var persistedFaceResult = await FaceServiceClient.AddPersonFaceAsync(personGroupId, guid, s);
                
                //person group must be trained after face addition
                TrainPersonGroup(personGroupId);
                return persistedFaceResult;
            }
        }
    }
}