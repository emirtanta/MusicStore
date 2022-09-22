using Dapper;
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
    [Authorize(Roles = ProjectConstant.Role_User_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CoverTypeController(IUnitOfWork uow)
        {
            _uow = uow;
        }


        #region Kapak Listesi Bölümü

        public IActionResult Index()
        {
            return View();
        }

        #endregion

        #region Kapak Ekle & Güncelle Bölümü


        /// <summary>
        /// Kategori Ekleme Ve Güncelleme
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();

            //kategori ekler
            if (id==null)
            {
                return View(coverType);
            }

            //coverType günceller
            //(int)id=> int değer girilmesi zorunlu olmadığından hata almamak için tanımlandı
            var parameter = new DynamicParameters();
            parameter.Add("@Id",id);

            coverType = _uow.sp_call.OneRecord<CoverType>(ProjectConstant.Proc_CoverType_Get, parameter);

            //coverType = _uow.coverType.Get((int)id);

            if (coverType !=null)
            {
                return View(coverType);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                //coverType ekler

                var parameter = new DynamicParameters();
                parameter.Add("@Name", coverType.Name);

                
                if (coverType.Id==0)
                {
                    //_uow.coverType.Add(coverType);
                    _uow.sp_call.Execute(ProjectConstant.Proc_CoverType_Create, parameter);
                }

                //coverType günceller
                else
                {
                    parameter.Add("@Id", coverType.Id);
                    //_uow.coverType.Update(coverType);
                    _uow.sp_call.Execute(ProjectConstant.Proc_CoverType_Update, parameter);
                }

                _uow.Save();

                return RedirectToAction("Index");
            }

            return View(coverType);
        }

        #endregion

       


        #region Kategori Json(category.js) dosyasından okur

        public IActionResult GetAll()
        {
            //var allObj = _uow.coverType.GetAll();

            //kapak resimleri listesini store procedure ile çağırdık
            var allCoverTypes = _uow.sp_call.List<CoverType>(ProjectConstant.Proc_CoverType_GetAll, null);

            return Json(new { data = allCoverTypes });
        }

        //kategori siler
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            //var deleteData = _uow.coverType.Get(id);

            ////veri yoksa 
            //if (deleteData==null)
            //{
            //    return Json(new { success = false, message = "Veri bulunamadı" });
            //}

            ////veri varsa veriyi siler
            //_uow.coverType.Remove(deleteData);

            //_uow.Save();

            //return Json(new { success=true,message="Veri silindi"});

            //silme işlemi yapan store procedure
            var parameter = new DynamicParameters();
            parameter.Add("@Id", id);

            var deleteData = _uow.sp_call.OneRecord<CoverType>(ProjectConstant.Proc_CoverType_Get, parameter);

            if (deleteData == null)
            {
                return Json(new { success = false, message = "Veri bulunamadı" });
            }

            _uow.sp_call.Execute(ProjectConstant.Proc_CoverType_Delete, parameter);

            _uow.Save();

            return Json(new { success=true,message="Veri silindi"});

        }

        #endregion
    }
}
