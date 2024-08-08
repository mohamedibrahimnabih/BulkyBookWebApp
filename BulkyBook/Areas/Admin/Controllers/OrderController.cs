using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
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
