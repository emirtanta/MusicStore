using MusicStore.Data;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.MainRepository
{
    public class ShoppingCardRepository: Repository<ShoppingCard>, IShoppingCardRepository
    {
        private readonly ApplicationDbContext _db;
        public ShoppingCardRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCard shoppingCard)
        {
            _db.Update(shoppingCard);

        }
    }
}
