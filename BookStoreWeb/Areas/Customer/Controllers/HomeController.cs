using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Diagnostics;
using System.Security.Claims;

namespace BookStoreWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Catagory,CoverType");
            return View(productList);
        }


		public IActionResult Details(int productId) {

            ShopingCart objCart = new()
            {
                Count = 1,
                ProductId = productId,
                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Catagory,CoverType"),
            };

            return View(objCart);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize]
        public IActionResult Details(ShopingCart shopingCart)
        {

            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shopingCart.ApplicationUserId = claim.Value;
            
            var cartFromDb = _unitOfWork.ShopingCart.GetFirstOrDefault(
                u=> u.ApplicationUserId == claim.Value && u.ProductId == shopingCart.ProductId);

            if (cartFromDb != null)
            {
                _unitOfWork.ShopingCart.UpdateCount(cartFromDb, shopingCart.Count);
                _unitOfWork.Save();
               
            }
            else
            {
                _unitOfWork.ShopingCart.Add(shopingCart);
               
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
              _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count);

            }

          
           
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}