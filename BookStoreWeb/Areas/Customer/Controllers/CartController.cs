using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookStoreWeb.Areas.Customer.Controllers
{

	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		[BindProperty]
		public ShopingCartVM ShopingCartVM { get; set; }
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

				ListCart = _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),
				OrderHeader = new(),

			};

			foreach (var cart in ShopingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				ShopingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			return View(ShopingCartVM);
		}




		public IActionResult Summary() {

			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShopingCartVM = new ShopingCartVM()
			{

				ListCart = _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),
				OrderHeader = new(),

			};

			foreach (var cart in ShopingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				ShopingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			ShopingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);


			ShopingCartVM.OrderHeader.Name = ShopingCartVM.OrderHeader.ApplicationUser.Name;
			ShopingCartVM.OrderHeader.PhoneNumber = ShopingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShopingCartVM.OrderHeader.City = ShopingCartVM.OrderHeader.ApplicationUser.City;
			ShopingCartVM.OrderHeader.Street = ShopingCartVM.OrderHeader.ApplicationUser.Street;
			ShopingCartVM.OrderHeader.StreetAddress = ShopingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShopingCartVM.OrderHeader.PostalCode = ShopingCartVM.OrderHeader.ApplicationUser.PostalCode;

			return View(ShopingCartVM);

		}


		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShopingCartVM.ListCart = _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == claim.Value,
				includeProperties: "Product");

			
			ShopingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
			ShopingCartVM.OrderHeader.ApplicationUserId = claim.Value;


			foreach (var cart in ShopingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
					cart.Product.Price50, cart.Product.Price100);
				ShopingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			ApplicationUser applicationuser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
			if (applicationuser.CompanyId.GetValueOrDefault() == 0)
			{
				ShopingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				ShopingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			}
			else
			{
				ShopingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShopingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}


			_unitOfWork.OrderHeader.Add(ShopingCartVM.OrderHeader);
			_unitOfWork.Save();
			foreach (var cart in ShopingCartVM.ListCart)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderId = ShopingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
			}



			// Stripe settings
			if (applicationuser.CompanyId.GetValueOrDefault() == 0) {

				var domain = "https://localhost:44340/";

				var options = new SessionCreateOptions
				{
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShopingCartVM.OrderHeader.Id}",
					CancelUrl = domain + $"customer/cart/index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

				foreach (var item in ShopingCartVM.ListCart)
				{
					{
						var sessionLineItem = new SessionLineItemOptions
						{
							PriceData = new SessionLineItemPriceDataOptions
							{
								UnitAmount = (long)(item.Price * 100),
								Currency = "usd",
								ProductData = new SessionLineItemPriceDataProductDataOptions
								{
									Name = item.Product.Title,
								}

							},
							Quantity = item.Count,

						};
						options.LineItems.Add(sessionLineItem);
					}



				}

				var service = new SessionService();

				Session session = service.Create(options);
				var x = session.Id;

				_unitOfWork.OrderHeader.UpdateStripePaymentId(ShopingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);

				return new StatusCodeResult(303);


			}

			else
			{
				return RedirectToAction("OrderConfirmation", "Cart", new { id = ShopingCartVM.OrderHeader.Id });
			}
				
			

		}


		public IActionResult OrderConfirmation(int id)
		{

			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);

			if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment) {

				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}



			}


			List<ShopingCart> shopingCarts = _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

			_unitOfWork.ShopingCart.RemoveRange(shopingCarts);
			_unitOfWork.Save();

			return View(id);
		}




		//Quantity Update IActionResults Minus , Plus,Remove, Update


		public IActionResult Minus(int CartId)
			{
				var cart = _unitOfWork.ShopingCart.GetFirstOrDefault(u => u.Id == CartId);

				if (cart.Count > 1)
				{
					_unitOfWork.ShopingCart.DecrementCount(cart, 1);
				}

				_unitOfWork.Save();
				return RedirectToAction(nameof(Index));
			}

			public IActionResult Plus(int CartId)
			{
				var cart = _unitOfWork.ShopingCart.GetFirstOrDefault(u => u.Id == CartId);
				_unitOfWork.ShopingCart.IncrementCount(cart, 1);
				_unitOfWork.Save();
				return RedirectToAction(nameof(Index));
			}

			public IActionResult Remove(int CartId)
			{
				var cart = _unitOfWork.ShopingCart.GetFirstOrDefault(u => u.Id == CartId);
				_unitOfWork.ShopingCart.Remove(cart);
				_unitOfWork.Save();
				return RedirectToAction(nameof(Index));
			}


			//Function for  Get Price Based On Quantity
			private double GetPriceBasedOnQuantity(int quantity, double price, double price50, double price100)
			{
				if (quantity < 50)
				{
					return price;
				}
				else
				{
					if (quantity < 100)
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

