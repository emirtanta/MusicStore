using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.DbModels;
using MusicStore.Models.ViewModels;
using MusicStore.Utility;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MusicStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _uow;

        [BindProperty]
        public OrderDetailsVM OrderDetailVM { get; set; }

        public OrderController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderDetailVM = new OrderDetailsVM
            {
                OrderHeader=_uow.orderHeader.GetFirstOrDefault(u=>u.Id==id,includeProperties:"ApplicationUser"),
                OrderDetails=_uow.orderDetail.GetAll(o=>o.OrderId==id,includeProperties:"Product")
            };

            return View(OrderDetailVM);
        }

        [Authorize(Roles =ProjectConstant.Role_User_Admin+","+ProjectConstant.Role_User_Employee)]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader orderHeader = _uow.orderHeader.GetFirstOrDefault(u => u.Id == id);

            orderHeader.OrderStatus = ProjectConstant.StatusInProcess;

            _uow.Save();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles =ProjectConstant.Role_User_Admin+","+ProjectConstant.Role_User_Employee)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _uow.orderHeader.GetFirstOrDefault(u => u.Id == OrderDetailVM.OrderHeader.Id);

            orderHeader.TrackingNumber = OrderDetailVM.OrderHeader.TrackingNumber;

            orderHeader.Carrier = OrderDetailVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = ProjectConstant.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _uow.Save();

            return RedirectToAction("Index");

        }

        #region Sipariş İptal Bölümü

        [Authorize(Roles =ProjectConstant.Role_User_Admin+","+ProjectConstant.Role_User_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _uow.orderHeader.GetFirstOrDefault(u => u.Id == id);

            //sipariş onaylanmışsa siparişi iptal eder
            
            if (orderHeader.PaymentStatus==ProjectConstant.StatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount=Convert.ToInt32(orderHeader.OrderTotal *100),
                    Reason=RefundReasons.RequestedByCustomer,
                    Charge=orderHeader.TransactionId
                };

                var service = new RefundService();

                Refund refund = service.Create(options);

                orderHeader.OrderStatus = ProjectConstant.StatusRefund;
                orderHeader.PaymentStatus = ProjectConstant.StatusRefund;
            }

            //sipariş onaylanmışsa siparişi iptal eder bitti


            else
            {
                orderHeader.OrderStatus = ProjectConstant.StatusCancelled;
                orderHeader.PaymentStatus = ProjectConstant.StatusCancelled;
            }

            _uow.Save();

            return RedirectToAction("Index");
        }

        #endregion

        #region Api Çağrıları Bölümü

        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimsIdentity =(ClaimsIdentity) User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList;

            if (User.IsInRole(ProjectConstant.Role_User_Admin) || User.IsInRole(ProjectConstant.Role_User_Employee))
            {
                orderHeaderList = _uow.orderHeader.GetAll(includeProperties: "ApplicationUser");
            }

            else
            {
                orderHeaderList = _uow.orderHeader.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaderList = orderHeaderList.Where(o => o.PaymentStatus == ProjectConstant.PaymentStatusDelayedPayment);
                    break;

                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == ProjectConstant.StatusApproved || o.OrderStatus==ProjectConstant.StatusInProcess || o.OrderStatus==ProjectConstant.StatusPending);
                    break;

                case "completed":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == ProjectConstant.StatusShipped);
                    break;

                case "rejected":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == ProjectConstant.StatusCancelled || o.OrderStatus==ProjectConstant.StatusRefund || o.OrderStatus==ProjectConstant.PaymentStatusRejected);
                    break;

                default:
                    break;
            }

            //orderHeaderList = _uow.orderHeader.GetAll(includeProperties:"ApplicationUser");

            return Json(new { data = orderHeaderList });
        }


        #endregion
    }
}
