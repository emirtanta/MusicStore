using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicStore.Data;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.DbModels;
using MusicStore.Models.ViewModels;
using MusicStore.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ProjectConstant.Role_User_Admin )] //admin  yetkisine sahip kullanıcılar görür
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _uow;

        //resim eklemek için tanımlandı
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork uow, IWebHostEnvironment hostEnvironment)
        {
            _uow = uow;
            _hostEnvironment = hostEnvironment;
        }


        #region Ürün Listesi Bölümü

        public IActionResult Index()
        {
            return View();
        }

        #endregion

        #region Ürün Ekle & Güncelle Bölümü


        /// <summary>
        /// Ürün Ekleme Ve Güncelleme
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _uow.category.GetAll().Select(i => new SelectListItem
                {
                    Text=i.CategoryName,
                    Value=i.Id.ToString()
                }),
                CoverTypeList = _uow.coverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            
            if (id==null)
            {
                return View(productVM);
            }

            productVM.Product = _uow.product.Get(id.GetValueOrDefault());

            if (productVM.Product==null)
            {
                return NotFound();
            }

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                #region resim ekleme işlemi

                string webRootPath = _hostEnvironment.WebRootPath;

                //gelen dosyanın olup olmadığı bilgisi
                var files = HttpContext.Request.Form.Files;

                if (files.Count>0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\products");
                    var extenstion = Path.GetExtension(files[0].FileName);

                    //resim var mı kontrolü
                    if (productVM.Product.ImageUrl != null)
                    {
                        var imageUrl = productVM.Product.ImageUrl;
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        //aynı resim varsa silinir
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }

                    productVM.Product.ImageUrl = @"\images\products\" + fileName + extenstion;
                }

                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var productData = _uow.product.Get(productVM.Product.Id);
                        productVM.Product.ImageUrl = productData.ImageUrl;
                    }
                }

                #endregion

                //ürün ekler
                if (productVM.Product.Id == 0)
                {
                    _uow.product.Add(productVM.Product);
                }

                //ürün günceller
                else
                {
                    _uow.product.Update(productVM.Product);
                }

                _uow.Save();

                return RedirectToAction("Index");
            }

            else
            {
                //kategori dropdown
                productVM.CategoryList = _uow.category.GetAll().Select(a => new SelectListItem
                {
                    Text=a.CategoryName,
                    Value=a.Id.ToString()
                });

                //kapak dropdown
                productVM.CoverTypeList = _uow.coverType.GetAll().Select(a => new SelectListItem
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                });

                if (productVM.Product.Id!=0)
                {
                    productVM.Product = _uow.product.Get(productVM.Product.Id);
                }
            }

            return View(productVM.Product);
        }

        #endregion

        


        #region Kategori Json(category.js) dosyasından okur

        public IActionResult GetAll()
        {
            var allObj = _uow.product.GetAll(includeProperties:"Category");

            return Json(new { data = allObj });
        }

        //ürün siler
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var deleteData = _uow.product.Get(id);

            //veri yoksa 
            if (deleteData==null)
            {
                return Json(new { success = false, message = "Veri bulunamadı" });
            }

            #region resim silme işlemi

            string webRootPath = _hostEnvironment.WebRootPath;

            var imagePath = Path.Combine(webRootPath, deleteData.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            #endregion



            //veri varsa veriyi siler
            _uow.product.Remove(deleteData);

            _uow.Save();

            return Json(new { success=true,message="Veri silindi"});
        }

        #endregion
    }
}
