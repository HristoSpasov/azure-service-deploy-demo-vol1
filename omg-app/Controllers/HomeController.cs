namespace omg_app.Controllers
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore.Internal;
    using omg_app.Models;

    public class HomeController : Controller
    {
        private readonly BlobStorageService blobStorageService;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly FaceService faceService;

        public HomeController(BlobStorageService blobStorageService, IHostingEnvironment hostingEnvironment, FaceService faceService)
        {
            this.blobStorageService = blobStorageService;
            this.hostingEnvironment = hostingEnvironment;
            this.faceService = faceService;
        }

        public IActionResult Index()
        {
            return this.View(new MessageModel() { Message = Store.Message });
        }

        public async Task<IActionResult> Blobs()
        {
            var blobs = await this.blobStorageService.GetALLAsync();

            return this.View(blobs);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBlob([FromBody] DeleteBlobModel model)
        {
            await this.blobStorageService.DeleteAsync(model.BlobName);

            return this.View("Blobs");
        }

        [HttpPost]
        public async Task<IActionResult> UploadBlob(IFormFile file)
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                await this.blobStorageService.UploadAsync(file.FileName, stream.ToArray());

                return this.RedirectToAction("Blobs");
            }
        }

        [HttpGet]
        public ActionResult Face()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Face(IFormFile file)
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                await this.blobStorageService.UploadAsync(file.FileName, stream.ToArray());
                var imageUrl = await this.blobStorageService.GetAsync(file.FileName);

                var faceAttributes = await this.faceService.DetectFaceAttributesAsync(imageUrl);

                return this.View(new FaceAttributesModel()
                {
                    ImageUrl =  imageUrl,
                    FaceAttributes = faceAttributes.Any() ? faceAttributes[0] : null
                });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}