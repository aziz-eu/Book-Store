using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStoreWeb.Areas.Customer.Controllers
{

    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShopingCartVM ShopingCartVM {  get; set; }
		public CartController(IUnitOfWork unitOfWork)
		{
		    _unitOfWork = unitOfWork;
		}

		public IActionResult Index()
        {
			var claimsInditity = (ClaimsIdentity)User.Identity;
            var claim = claimsInditity.FindFirst(ClaimTypes.NameIdentifier);

            ShopingCartVM = new ShopingCartVM()
            {

                ListCart = _unitOfWork.ShopingCart.GetAll(u=> u.ApplicationUserId == claim.Value , includeProperties: "Product")
            };

            foreach (var cart in ShopingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShopingCartVM.CartTotal += (cart.Price * cart.Count);
            }

            return View(ShopingCartVM);
        }


        public IActionResult Plus(int CartId)
        {
            var cart = _unitOfWork.ShopingCart.GetFirstOrDefault(u => u.Id == CartId);
            _unitOfWork.ShopingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int CartId)
        {
            var cart = _unitOfWork.ShopingCart.GetFirstOrDefault(u=>u.Id == CartId);

            if(cart.Count > 1)
            {
				_unitOfWork.ShopingCart.DecrementCount(cart, 1);
			}
           
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int CartId)
        {
            var cart = _unitOfWork.ShopingCart.GetFirstOrDefault(u=>u.Id ==CartId);
            _unitOfWork.ShopingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            return View();
        }

        private double GetPriceBasedOnQuantity(int quantity, double price, double price50,double price100)
        {
            if(quantity < 50)
            {
                return price;
            }
            else
            {
                if(quantity< 100)
                {
                    return price50;
                }
                else
                {
                    return price100;
                }
            }
        }
    }
}
