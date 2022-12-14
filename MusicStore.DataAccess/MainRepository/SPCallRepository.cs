using Dapper;
using MusicStore.Data;
using MusicStore.DataAccess.IMainRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace MusicStore.DataAccess.MainRepository
{
    public class SPCallRepository : ISPCallRepository
    {

        private readonly ApplicationDbContext _db;

        //dapper için bağlantı bilgisi
        private static string connectionString = "";

        public SPCallRepository(ApplicationDbContext db)
        {
            _db = db;
            connectionString = db.Database.GetDbConnection().ConnectionString;
        }
        

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Execute(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlCon=new SqlConnection(connectionString))
            {
                sqlCon.Open();

                sqlCon.Execute(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public IEnumerable<T> List<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();

                return sqlCon.Query<T>(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();

                var result = SqlMapper.QueryMultiple(sqlCon, procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);

                var item1 = result.Read<T1>().ToList();
                var item2 = result.Read<T2>().ToList();

                if (item1!=null && item2!=null)
                {
                    return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(item1, item2);
                }
            }

            return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(new List<T1>(), new List<T2>());
        }

        public T OneRecord<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();

                var value=sqlCon.Query<T>(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);

                return (T)Convert.ChangeType(value.FirstOrDefault(), typeof(T)); 

            }
        }

        public T Single<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();

                

                return (T)Convert.ChangeType(sqlCon.ExecuteScalar<T>(procedureName,parameters,commandType:System.Data.CommandType.StoredProcedure),typeof(T));

            }
        }
    }
}
