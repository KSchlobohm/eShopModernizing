using eShopModernizedMVC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace eShopModernizedMVC.Services
{
    public class ImageSqlStorage : IImageService
    {
        private CatalogDBContext _db;
        private readonly ICatalogService _catalogService;

        public ImageSqlStorage(ICatalogService service, CatalogDBContext db)
        {
            _catalogService = service;
            _db = db;
        }

        public string BaseUrl()
        {
            return string.Empty;
        }

        public string BuildUrlImage(CatalogItem item)
        {
            return $"/items/{item.Id}/pic";
        }

        public void Dispose()
        {
        }

        public void InitializeCatalogImages()
        {
            var webRoot = HttpContext.Current.Server.MapPath("~/Pics");

            for (int i = 1; i <= 12; i++)
            {
                var item = _catalogService.FindCatalogItem(i);

                var path = Path.Combine(webRoot, i + ".png");
                var buffer = File.ReadAllBytes(path);
                item.ImageData = Convert.ToBase64String(buffer);

                _catalogService.UpdateCatalogItem(item);
            }
        }

        public void UpdateImage(CatalogItem item)
        {
            var imageId = Guid.Parse(item.TempImageName);
            var tempImage = _db.TempImages.FirstOrDefault(t => t.Id == imageId);
            if (tempImage != null)
            {
                item.ImageData = tempImage.ImageData;
                item.PictureFileName = tempImage.Name;
                item.TempImageName = null;
                _db.TempImages.Remove(tempImage); //note that SaveChanges will be invoked when we save the catalog item
            }
        }

        public string UploadTempImage(HttpPostedFile file, int? catalogItemId)
        {
            file.InputStream.Position = 0;
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes((int)file.InputStream.Length);
            }

            var newImage = new TempImage
            {
                Name = file.FileName,
                ImageData = Convert.ToBase64String(fileData)
            };
            newImage.Id = Guid.NewGuid();
            _db.TempImages.Add(newImage);
            _db.SaveChanges();

            return newImage.Id.ToString();
        }

        public string UrlDefaultImage()
        {
            return "/Pics/default.png";
        }
    }
}