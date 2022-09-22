using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Authorize(Roles = ProjectConstant.Role_User_Admin)] //admin  yetkisine sahip kullanıcılar görür
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }


        #region Kullanıcı Listesi Bölümü

        public IActionResult Index()
        {
            return View();
        }

        #endregion

        #region Kullanıcı Json(user.js) dosyasından okur

        public IActionResult GetAll()
        {
            //sistemdeki kullanıcıları getirir
            var userList = _db.ApplicationUsers.Include(c => c.Company).ToList();

            //kullanıcılara tanımlanan rolleri getirir
            var userRole = _db.UserRoles.ToList();

            //sistemde tanımlı rolleri getirir
            var roles = _db.Roles.ToList();

            foreach (var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;

                //kullanıcının rolündeki adı getirdik
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;

                //kullanıcının şirket adı boşsa ona değer atayarak hata alınmaması sağlandı
                if (user.Company==null)
                {
                    user.Company = new Company()
                    {
                        Name = string.Empty
                    };
                }
            }

            return Json(new { data = userList });
        }

        //kullanıcının kilidi açar
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var data = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (data==null)
            {
                return Json(new { success = false, message = "Error while locking/unlocking" });
            }

            if (data.LockoutEnd!=null && data.LockoutEnd>DateTime.Now)
            {
                data.LockoutEnd = DateTime.Now;
            }

            else
            {
                data.LockoutEnd = DateTime.Now.AddYears(10);
            }

            _db.SaveChanges();

            return Json(new { success = true, message = "İşlem başarılı" });
        }

        #endregion
    }
}
