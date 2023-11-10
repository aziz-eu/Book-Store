using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
