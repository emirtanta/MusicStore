using MusicStore.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.IMainRepository
{
    public interface IShoppingCardRepository:IRepository<ShoppingCard>
    {
        void Update(ShoppingCard shoppingCard);
    }
}
