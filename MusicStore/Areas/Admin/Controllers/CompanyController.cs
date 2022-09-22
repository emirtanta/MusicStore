using Microsoft.AspNetCore.Mvc;
using MusicStore.Data;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CompanyController(IUnitOfWork uow)
        {
            _uow = uow;
        }


        #region Şirket Listesi Bölümü

        public IActionResult Index()
        {
            return View();
        }

        #endregion

        #region Şirket Ekle & Güncelle Bölümü


        /// <summary>
        /// Şirket Ekleme Ve Güncelleme
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Upsert(int? id)
        {
            Company cat = new Company();

            //şirket ekler
            if (id==null)
            {
                return View(cat);
            }

            //kategori günceller
            //(int)id=> int değer girilmesi zorunlu olmadığından hata almamak için tanımlandı
            cat = _uow.company.Get((int)id);

            if (cat!=null)
            {
                return View(cat);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                //şirket ekler
                if (company.Id==0)
                {
                    _uow.company.Add(company);
                }

                //şirket günceller
                else
                {
                    _uow.company.Update(company);
                }

                _uow.Save();

                return RedirectToAction("Index");
            }

            return View(company);
        }

        #endregion

        


        #region Şirket Json(company.js) dosyasından okur

        public IActionResult GetAll()
        {
            var allObj = _uow.company.GetAll();

            return Json(new { data = allObj });
        }

        //şirket siler
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var deleteData = _uow.company.Get(id);

            //veri yoksa 
            if (deleteData==null)
            {
                return Json(new { success = false, message = "Veri bulunamadı" });
            }

            //veri varsa veriyi siler
            _uow.company.Remove(deleteData);

            _uow.Save();

            return Json(new { success=true,message="Veri silindi"});
        }

        #endregion
    }
}
