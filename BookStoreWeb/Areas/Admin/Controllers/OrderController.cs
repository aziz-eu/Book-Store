using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BookStoreWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
			_unitOfWork = unitOfWork;
            
        }
        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId) {

            OrderVM = new OrderVM()
            {

                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product")


            };
            return View(OrderVM);
        }


        [HttpPost]
        [Authorize (Roles = SD.Role_Admin +","+ SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {

            var orderHeaderFromDB = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);

            orderHeaderFromDB.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDB.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDB.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDB.City = OrderVM.OrderHeader.City; 
            orderHeaderFromDB.PostalCode = OrderVM.OrderHeader.PostalCode;
            orderHeaderFromDB.Street = OrderVM.OrderHeader.Street;

            if(OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDB.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            if(OrderVM.OrderHeader.Carrer != null)
            {
                orderHeaderFromDB.Carrer = OrderVM.OrderHeader.Carrer;
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromDB);
            _unitOfWork.Save();

            TempData["success"] = "Order details updateed successfully";
          return RedirectToAction("Details", "Order", new {orderId = orderHeaderFromDB.Id});
        }

        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]

		public IActionResult Details_PAY_NOW()
		{


            OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product");


            // Stripe settings

                var domain = "https://localhost:44340/";

				var options = new SessionCreateOptions
				{
					SuccessUrl = domain + $"admin/order/PaymentConfirmation?OrderHeaderid={OrderVM.OrderHeader.Id}",
					CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

				foreach (var item in OrderVM.OrderDetail)
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

				_unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);

				return new StatusCodeResult(303);

		}


		public IActionResult PaymentConfirmation(int orderHeaderid)
		{

			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderid);

			if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
			{

				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderid, session.Id, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus , SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}



			}

			return View(orderHeaderid);
		}



		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
        public IActionResult StratProcessing()
        {

           
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProccess);
            _unitOfWork.Save();

            TempData["success"] = "Order Status is in Inprocess";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }


        [HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {

            var orderHeaderFromDB = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDB.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDB.Carrer= OrderVM.OrderHeader.Carrer;
            orderHeaderFromDB.OrderStatus = SD.StatusShipped;
            orderHeaderFromDB.ShippingDate = DateTime.Now;

            if(orderHeaderFromDB.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDB.PaymentDueDate = DateTime.Now.AddDays(30);
            }




            _unitOfWork.OrderHeader.Update(orderHeaderFromDB);
            _unitOfWork.Save();

            TempData["success"] = "Order Status is in shipped";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
		public IActionResult CancelOrder()
		{

			var orderHeaderFromDB = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);

            if(orderHeaderFromDB.PaymentStatus  == SD.PaymentStatusApproved)
            {

                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDB.PaymentIntentNumber,

                };
                var services = new RefundService();
                Refund refund = services.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDB.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
				_unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDB.Id, SD.StatusCancelled, SD.StatusCancelled);

			}

			_unitOfWork.Save();

			TempData["success"] = "Order Canceled Successfully";
			return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
		}

		#region API CALLS

		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderHeaders;
			

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee) ){
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");


            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId == claims.Value,includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;

                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProccess);
                    break;

                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;

                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.StatusApproved);
                    break;
                default:
                    break;
                    

            }


            return Json(new { data = orderHeaders });
		}

		#endregion
	}
}
