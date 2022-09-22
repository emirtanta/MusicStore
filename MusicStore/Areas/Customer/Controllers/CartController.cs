using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.DbModels;
using MusicStore.Models.ViewModels;
using MusicStore.Utility;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MusicStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork uow,IEmailSender emailSender,UserManager<IdentityUser> userManager)
        {
            _uow = uow;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            //hangi kullanıcı ile işlem yapıldığı bilgisini getirir
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);



            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new OrderHeader(),
                ListCart = _uow.shoppingCard.GetAll(u => u.ApplicationUserId == claims.Value, includeProperties: "Product")
            };

            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _uow.applicationUser
                                                        .GetFirstOrDefault(u => u.Id == claims.Value, includeProperties: "Company");

            //sepetteki verilerin fiyat hesaplaması

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = ProjectConstant.GetPriceBaseOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                cart.Product.Description = ProjectConstant.ConvertToRawHtml(cart.Product.Description); //tiny editördeki verileri getirmek için tanımlandı

                if (cart.Product.Description.Length > 50)
                {
                    cart.Product.Description = cart.Product.Description.Substring(0, 49) + "....";
                }
            }

            //sepetteki verilerin fiyat hesaplaması bitti


            return View(ShoppingCartVM);
        }

        #region Kullanıcı Sepetten alış veriş Yapması için Email Onaylama Bölümü

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            //hangi kullanıcı ile işlem yapıldığı bilgisini getirir
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var user = _uow.applicationUser.GetFirstOrDefault(u => u.Id == claims.Value);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email is empty!");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "verification emil sent.Please check your email!");
            return RedirectToAction("Index");
        }

        #endregion

        #region Sepetteki Artı Butonu Bölümü

        //cartId=> asp-route-cartId 'deki cartId kısmıdır
        //id=> jquery ile ürün arttırma işlemi yapılabilmesi için tanımlandı. jquery kullanılmayacaksa cartId kullanılabilir
        public IActionResult Plus(int cartId /*id*/)
        {
            var cart = _uow.shoppingCard.GetFirstOrDefault(x => x.Id == cartId /*id*/, includeProperties: "Product");

            if (cart==null)
            {
                //Jquery ile sepetteki ürünü artı butonuna basınca arttırma
                //return Json(false);
                
                //jquery haricinde ürün arttırma işlemi için açın
                return RedirectToAction("Index");
            }

            cart.Count += 1;
            cart.Price = ProjectConstant.GetPriceBaseOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);

            _uow.Save();

            //Jquery ile sepetteki ürünü artı butonuna basınca arttırma
            //var allShoppingCart = _uow.shoppingCard.GetAll();
            //return Json(true);

            //jquery haricinde ürün arttırma işlemi için açın
            return RedirectToAction("Index");
            
        }

        #endregion

        #region Sepetteki Eksi Butonu Bölümü

        public IActionResult Minus(int cartId)
        {
            var cart = _uow.shoppingCard.GetFirstOrDefault(x => x.Id == cartId, includeProperties: "Product");
            if (cart.Count == 1)
            {
                var cnt = _uow.shoppingCard.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                _uow.shoppingCard.Remove(cart);
                _uow.Save();
                HttpContext.Session.SetInt32(ProjectConstant.shoppingCard, cnt - 1);
            }
            else
            {
                cart.Count -= 1;
                cart.Price = ProjectConstant.GetPriceBaseOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                _uow.Save();
            }
            return RedirectToAction("Index");
        }

        #endregion

        #region Sepetteki Ürünü Silen Buton Bölümü

        public IActionResult Remove(int cartId)
        {
            var cart = _uow.shoppingCard.GetFirstOrDefault(x => x.Id == cartId, includeProperties: "Product");

            var cnt = _uow.shoppingCard.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            _uow.shoppingCard.Remove(cart);
            _uow.Save();
            HttpContext.Session.SetInt32(ProjectConstant.shoppingCard, cnt - 1);

            return RedirectToAction("Index");
        }

        #endregion

        #region Alış Veriş Özeti Bölümü

        [HttpGet]
        public IActionResult Summary()
        {
            //hangi kullanıcı ile işlem yapıldığı bilgisini getirir
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new OrderHeader(),
                ListCart = _uow.shoppingCard.GetAll(u => u.ApplicationUserId == claims.Value, includeProperties: "Product")
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _uow.applicationUser.GetFirstOrDefault(u => u.Id == claims.Value, includeProperties: "Company");

            foreach (var item in ShoppingCartVM.ListCart)
            {
                item.Price = ProjectConstant.GetPriceBaseOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);

                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;

            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;

            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;

            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.State;
            ShoppingCartVM.OrderHeader.PostCode = ShoppingCartVM.OrderHeader.PostCode;

            return View(ShoppingCartVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string stripeToken)
        {
            //hangi kullanıcı ile işlem yapıldığı bilgisini getirir
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.OrderHeader.ApplicationUser = _uow.applicationUser.GetFirstOrDefault(a => a.Id == claims.Value, includeProperties: "Company");

            ShoppingCartVM.ListCart = _uow.shoppingCard.GetAll(s => s.ApplicationUserId == claims.Value,includeProperties:"Product");

            ShoppingCartVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusPending;

            ShoppingCartVM.OrderHeader.OrderStatus = ProjectConstant.StatusPending;

            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;

            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

            _uow.orderHeader.Add(ShoppingCartVM.OrderHeader);

            _uow.Save();

            //Sipariş Detay kısmımlarını ekleme

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();

            foreach (var orderDetail in ShoppingCartVM.ListCart)
            {
                orderDetail.Price = ProjectConstant.GetPriceBaseOnQuantity(orderDetail.Count, orderDetail.Product.Price, orderDetail.Product.Price50, orderDetail.Product.Price100);

                OrderDetails oDetails = new OrderDetails()
                {
                    ProductId = orderDetail.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = orderDetail.Price,
                    Count = orderDetail.Count
                };

                ShoppingCartVM.OrderHeader.OrderTotal += oDetails.Count * oDetails.Price;

                _uow.orderDetail.Add(oDetails);
            }

            _uow.shoppingCard.RemoveRange(ShoppingCartVM.ListCart);

            

            HttpContext.Session.SetInt32(ProjectConstant.shoppingCard, 0);

            //Sipariş Detay kısmımlarını ekleme bitti

            //ödeme işlemleri

            if (stripeToken==null)
            {
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);

                ShoppingCartVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusDelayedPayment;

                ShoppingCartVM.OrderHeader.OrderStatus = ProjectConstant.StatusApproved;
            }

            else
            {
                var options = new ChargeCreateOptions
                {
                    Amount=Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal *100),

                    Currency="usd",
                    Description="Order Id: "+ShoppingCartVM.OrderHeader.Id,
                    Source=stripeToken
                    
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);


                if (charge.BalanceTransactionId==null)
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusRejected;
                }

                else
                {
                    ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                }

                if (charge.Status.ToLower()=="succeded")
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusApproved;

                    ShoppingCartVM.OrderHeader.OrderStatus = ProjectConstant.StatusApproved;

                    ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
                }
            }

            //ödeme işlemleri bitti

            _uow.Save();

            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });

        }

        #endregion

        #region Sipariş Onaylama Sayfası Bölümü

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }

        #endregion

    }
}
