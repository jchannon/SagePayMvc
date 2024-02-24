using Microsoft.AspNetCore.Mvc;

namespace SagePayMvc.Sample.Controllers {
	using SagePayMvc.Sample.Models;

	public class BasketController : Controller {
		IShoppingBasket _basket;
		IProductRepository _productRepository;
		ITransactionService _transactionService;

		public BasketController(IShoppingBasket basket, IProductRepository productRepository, ITransactionService transactionService) {
			_basket = basket;
			_transactionService = transactionService;
			_productRepository = productRepository;
		}

		// Display the products currently in the shopping basket
		public ActionResult Index() {
			return View(_basket.GetItemsInBasket(this.HttpContext));
		}

		public ActionResult Add(int id) {
			var product = _productRepository.FindById(id);
			_basket.AddProduct(product, this.HttpContext);
			return RedirectToAction("Index");
		}

		public ActionResult Remove(int id) {
			_basket.RemoveItem(id, this.HttpContext);
			return RedirectToAction("Index");
		}

		public ActionResult Checkout(User user) {
			// Register the transaction with SagePay and send the user to the SagePay site.
			var transaction = _transactionService.SendTransaction(_basket, this.Url, user, this.HttpContext);
			return Redirect(transaction.NextURL);
		}
	}
}