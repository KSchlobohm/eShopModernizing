using eShopModernizedMVC.Models;
using eShopModernizedMVC.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eShopModernizedMVC.Controllers
{
    public class SearchController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICatalogService _service;
        private readonly IImageService _imageService;

        public SearchController(ICatalogService service, IImageService imageService)
        {
            _service = service;
            _imageService = imageService;
        }


        // GET /[?pageSize=3&pageIndex=10]
        public ActionResult Index(int pageSize = 10, int pageIndex = 0, string searchTerm = null)
        {
            _log.Info($"Now loading... /Search/Index?searchTerm={searchTerm ?? "null"}pageSize={pageSize}&pageIndex={pageIndex}");
            var paginatedItems = _service.GetCatalogItemsPaginated(pageSize, pageIndex, searchTerm);
            ChangeUriPlaceholder(paginatedItems.Data);
            ViewBag.SearchTerm = searchTerm;
            return View(paginatedItems);
        }

        private void ChangeUriPlaceholder(IEnumerable<CatalogItem> items)
        {
            foreach (var catalogItem in items)
            {
                AddUriPlaceHolder(catalogItem);
            }
        }

        private void AddUriPlaceHolder(CatalogItem item)
        {
            item.PictureUri = _imageService.BuildUrlImage(item);
        }
    }
}