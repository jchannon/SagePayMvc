using Microsoft.AspNetCore.Mvc;

namespace SagePayMvc.Sample.Controllers {
    using SagePayMvc.Sample.Models;

    public class HomeController : Controller {
        IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository) {
            _productRepository = productRepository;
        }

        public ActionResult Index() {
            var products = _productRepository.GetAllProducts();
            return View(products);
        }

        public ActionResult Error() {
            return View();
        }
    }
}