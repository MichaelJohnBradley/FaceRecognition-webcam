using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using FaceRecognitionWeb.Models;

namespace FaceRecognitionWeb.Controllers
{
    public class HomeController : Controller
    {
        private FacialRecognition facialRecognizer { get; }

        public HomeController()
        {
            facialRecognizer = new FacialRecognition();            
        }
        public ActionResult Index()
        {            
            return View("Index");
        }

        public ActionResult Recognition()
        {
            return View();
        }

        public ActionResult PeopleGroups()
        {            
            return View();
        }

        public ActionResult Detection()
        {            
            return View();
        }

        public async Task<JsonResult> DetectFace(string fileData)
        {
           // var facialRecognizer = new FacialRecognition();
            var faces = await facialRecognizer.UploadAndDetectFacesStream(fileData);
            return Json(new {faces});
        }

        public async Task<JsonResult> IdentifyFace(string fileData)
        {
           // var facialRecognizer = new FacialRecognition();
            var persons = await facialRecognizer.IdentifyPersonInImage("employees",fileData);
            return Json(new { persons });
        }

        public async Task<JsonResult> GetPersonsInPersonGroup(string personGroupId)
        {
            //var facialRecongniser = new FacialRecognition();
            var persons = await facialRecognizer.GetPersonsInPersonGroup(personGroupId);
            return Json(persons.OrderBy(x=>x.Name));
        }

        public async Task<JsonResult> CreatePersonGroup(string personGroupId, string name)
        {
           // var facialRecongniser = new FacialRecognition();
            var persons = await facialRecognizer.CreatePersonGroup(personGroupId, name);            
            return Json(persons);
        }

        public async Task<JsonResult> CreatePerson(string personGroupId, string name)
        {
            //var facialRecongniser = new FacialRecognition();
            var persons = await facialRecognizer.CreatePerson(name, personGroupId);
            return Json(persons);
        }

        public async Task<JsonResult> DeletePerson(string personGroupId, string personId)
        {
            //var facialRecongniser = new FacialRecognition();
            await Task.Run(() => facialRecognizer.DeletePerson(personId, personGroupId)); 
            return Json(new {success= true});
        }

        public async Task<JsonResult> AddFaceToPerson(string personId, string personGroupId, string imageBase64)
        {
            //var facialRecongniser = new FacialRecognition();
            await Task.Run(() => facialRecognizer.AddFaceToPerson(personId, personGroupId, imageBase64));
            return Json(new { success = true });
        }
    }
}