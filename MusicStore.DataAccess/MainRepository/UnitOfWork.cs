using MusicStore.Data;
using MusicStore.DataAccess.IMainRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.MainRepository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;

            category = new CategoryRepository(_db);
            company = new CompanyRepository(_db);
            product = new ProductRepository(_db);
            coverType = new CoverTypeRepository(_db);
            shoppingCard = new ShoppingCardRepository(_db);
            orderHeader = new OrderHeaderRepository(_db);
            orderDetail = new OrderDetailRepository(_db);
            applicationUser = new ApplicationUserRepository(_db);
            sp_call = new SPCallRepository(_db);
        }

        public ICategoryRepository category { get; private set; }
        public ICompanyRepository company { get; private set; }
        public IProductRepository product { get; private set; }
        public ICoverTypeRepository coverType { get; private set; }
        public IShoppingCardRepository shoppingCard { get; private set; }
        public IOrderHeaderRepository orderHeader { get; private set; }
        public IOrderDetailRepository orderDetail { get; private set; }
        public IApplicationUserRepository applicationUser { get; private set; }

        public ISPCallRepository sp_call { get; private set; }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
