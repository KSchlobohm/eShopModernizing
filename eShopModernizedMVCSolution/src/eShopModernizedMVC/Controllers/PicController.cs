using eShopModernizedMVC.Services;
using log4net;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace eShopModernizedMVC.Controllers
{
    public class PicController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ImageFormat[] ValidFormats = { ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Gif };
        private readonly IImageService _imageService;
        private readonly ICatalogService _catalogService;
        public const string GetPicRouteName = "GetPicRouteTemplate";


        public PicController(ICatalogService service, IImageService imageService)
        {
            _imageService = imageService;
            _catalogService = service;
        }

        private const int CACHE_DURATION_AS_SECONDS = 60;

        [HttpGet]
        [Route("items/{catalogItemId:int}/pic", Name = GetPicRouteName)]
        // note that this would cause incorrect behavior for admin users - we should have 2 image URLs
        // [OutputCache(Duration = CACHE_DURATION_AS_SECONDS, VaryByParam = "catalogItemId")]
        public ActionResult Index(int catalogItemId)
        {
            _log.Info($"Now loading... /items/Index?{catalogItemId}/pic");

            if (catalogItemId <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var item = _catalogService.FindCatalogItem(catalogItemId);

            if (item != null)
            {
                string mimetype = GetImageMimeTypeFromImageFileExtension(item.PictureFileName);
                var buffer = Convert.FromBase64String(item.ImageData);

                return File(buffer, mimetype);
            }

            return HttpNotFound();
        }

        [HttpPost]
        [Route("uploadimage")]
        public ActionResult UploadImage()
        {
            _log.Info($"Now processing... /Pic/UploadImage");
            HttpPostedFile image = System.Web.HttpContext.Current.Request.Files["HelpSectionImages"];
            var itemId = System.Web.HttpContext.Current.Request.Form["itemId"];

            if (!IsValidImage(image))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "image is not valid");
            }

            int.TryParse(itemId, out var catalogItemId);
            var tempImageId = _imageService.UploadTempImage(image, catalogItemId);
            var tempImage = new
            {
                name = tempImageId,
                url = _imageService.UrlDefaultImage()
            };

            return Json(tempImage);
        }

        private bool IsValidImage(HttpPostedFile file)
        {
            bool isValidImage = true;
            try
            {
                using (var img = Image.FromStream(file.InputStream))
                {
                    isValidImage = ValidFormats.Contains(img.RawFormat);
                }
            }
            catch (Exception)
            {
                isValidImage = false;
            }

            return isValidImage;
        }

        private string GetImageMimeTypeFromImageFileExtension(string extension)
        {
            string mimetype;

            switch (extension)
            {
                case ".png":
                    mimetype = "image/png";
                    break;
                case ".gif":
                    mimetype = "image/gif";
                    break;
                case ".jpg":
                case ".jpeg":
                    mimetype = "image/jpeg";
                    break;
                case ".bmp":
                    mimetype = "image/bmp";
                    break;
                case ".tiff":
                    mimetype = "image/tiff";
                    break;
                case ".wmf":
                    mimetype = "image/wmf";
                    break;
                case ".jp2":
                    mimetype = "image/jp2";
                    break;
                case ".svg":
                    mimetype = "image/svg+xml";
                    break;
                default:
                    mimetype = "application/octet-stream";
                    break;
            }

            return mimetype;
        }
    }
}
