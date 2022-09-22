using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.DbModels;
using MusicStore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MusicStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _uow;

        public HomeController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _uow.product.GetAll(includeProperties:"Category,CoverType");


            //kullanıcı sisteme girdiğinde sepettindeki ürünler varsa getirir
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim!=null)
            {
                var shoppingCount = _uow.shoppingCard.GetAll(a => a.ApplicationUserId == claim.Value).ToList().Count();

                HttpContext.Session.SetInt32(ProjectConstant.shoppingCard, shoppingCount);
            }

            //kullanıcı sisteme girdiğinde sepettindeki ürünler varsa getirir bitti

            return View(productList);
        }

        #region Ürün Detayı Bölümü

        public IActionResult Details(int id)
        {
            var product = _uow.product.GetFirstOrDefault(p => p.Id == id, includeProperties: "Category,CoverType");

            ShoppingCard cart = new ShoppingCard()
            {
                Product=product,
                ProductId=product.Id
            };

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCard cartObj) 
        {
            cartObj.Id = 0; //id değeri sepete eklendiğinde hata alınmaması için tanımlandı

            if (ModelState.IsValid)
            {
                //kullanıcının bilgilerini getirir
                var claimsIdentity = (ClaimsIdentity)User.Identity;

                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                cartObj.ApplicationUserId = claim.Value;

                //eklenen kayıt varitabanında var mı kontrolü
                ShoppingCard fromDb = _uow.shoppingCard.GetFirstOrDefault(
                    s => s.ApplicationUserId == cartObj.ApplicationUserId
                    && s.ProductId == cartObj.ProductId,
                    includeProperties: "Product"
                    );

                if (fromDb==null)
                {
                    //sepete ürün ekleme işlemi
                    _uow.shoppingCard.Add(cartObj);
                }

                else
                {
                    //sepetteki ürünü güncelleme işlemi
                    fromDb.Count += cartObj.Count;
                }

                _uow.Save();

                //işlemleri session içerisine atma

                var shoppinCount = _uow.shoppingCard.GetAll(a => a.ApplicationUserId == cartObj.ApplicationUserId).ToList().Count();

                HttpContext.Session.SetInt32(ProjectConstant.shoppingCard,shoppinCount);

                //işlemleri session içerisine atma bitti

                return RedirectToAction("Index");

            }

            else
            {
                var product = _uow.product.GetFirstOrDefault(p => p.Id == cartObj.ProductId, includeProperties: "Category,CoverType");

                ShoppingCard cart = new ShoppingCard()
                {
                    Product = product,
                    ProductId = product.Id
                };

                return View(cart);
            }

            
        }

        #endregion
    }
}
