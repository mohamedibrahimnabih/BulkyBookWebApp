using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderVM = new()
            {
                OrderHeader = unitOfWork.OrderHeaderRepository.GetOne(e => e.Id == id, "ApplicationUser")
            };

            if (OrderVM.OrderHeader != null)
            {
                OrderVM.OrderDetails = unitOfWork.OrderDetailRepository.Get(e => e.OrderHeaderId == id, "Product");
                return View(OrderVM);
            }
            else return NotFound();
        }

        #region APIs
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (status == "All" || string.IsNullOrEmpty(status))
            {
                orderHeaders = unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                orderHeaders = unitOfWork.OrderHeaderRepository.Get(
                    expression: o => o.OrderStatus == status || o.PaymentStatus == status,
                    includeProperties: "ApplicationUser");
            }

            return Json(orderHeaders);
        }
        #endregion
    }
}
