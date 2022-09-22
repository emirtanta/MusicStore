using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.IMainRepository
{
    public interface IRepository<T> where T:class
    {
        T Get(int id);

        IEnumerable<T> GetAll(
            Expression<Func<T, bool>> filter = null,
            //sıralama yapar
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null);

        //ilk bulunan kaydı getirir
        T GetFirstOrDefault(
            Expression<Func<T, bool>> filter = null,
            string includeProperties = null);

        void Add(T entity);
        void Remove(int id);
        void Remove(T entity);

        //çoklu kayıt siler
        void RemoveRange(IEnumerable<T> entity);
    }
}
