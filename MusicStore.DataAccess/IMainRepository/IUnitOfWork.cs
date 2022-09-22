using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.IMainRepository
{
    public interface IUnitOfWork:IDisposable
    {
        ICategoryRepository category { get; }
        ICompanyRepository company { get; }
        IProductRepository product { get; }
        IApplicationUserRepository applicationUser { get; }
        ICoverTypeRepository coverType { get; }
        IShoppingCardRepository shoppingCard { get; }
        IOrderHeaderRepository orderHeader { get; }
        IOrderDetailRepository orderDetail { get; }
        ISPCallRepository sp_call { get; }
        void Save();
    }
}
