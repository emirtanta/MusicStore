using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Data;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.DbModels;
using MusicStore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =ProjectConstant.Role_User_Admin+","+ProjectConstant.Role_User_Employee)] //admin ve employee yetkisine sahip kullanıcılar görür
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CategoryController(IUnitOfWork uow)
        {
            _uow = uow;
        }


        #region Kategoriler Listesi Bölümü

        public IActionResult Index()
        {
            return View();
        }

        #endregion

        #region Kategori Ekle & Güncelle Bölümü


        /// <summary>
        /// Kategori Ekleme Ve Güncelleme
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Upsert(int? id)
        {
            Category cat = new Category();

            //kategori ekler
            if (id==null)
            {
                return View(cat);
            }

            //kategori günceller
            //(int)id=> int değer girilmesi zorunlu olmadığından hata almamak için tanımlandı
            cat = _uow.category.Get((int)id);

            if (cat!=null)
            {
                return View(cat);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                //kategori ekler
                if (category.Id==0)
                {
                    _uow.category.Add(category);
                }

                //kategori günceller
                else
                {
                    _uow.category.Update(category);
                }

                _uow.Save();

                return RedirectToAction("Index");
            }

            return View(category);
        }

        #endregion

        #region Kategori Silme Bölgesi



        #endregion


        #region Kategori Json(category.js) dosyasından okur

        public IActionResult GetAll()
        {
            var allObj = _uow.category.GetAll();

            return Json(new { data = allObj });
        }

        //kategori siler
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var deleteData = _uow.category.Get(id);

            //veri yoksa 
            if (deleteData==null)
            {
                return Json(new { success = false, message = "Veri bulunamadı" });
            }

            //veri varsa veriyi siler
            _uow.category.Remove(deleteData);

            _uow.Save();

            return Json(new { success=true,message="Veri silindi"});
        }

        #endregion
    }
}
