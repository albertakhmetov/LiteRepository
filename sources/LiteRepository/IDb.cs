using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LiteRepository
{
    public interface IDb
    {
        T Exec<T>(Func<DbConnection, T> action);
        Task<T> ExecAsync<T>(Func<DbConnection, Task<T>> action);

        E Insert<E>(E entity) where E : class;
        Task<E> InsertAsync<E>(E entity) where E : class;

        int Update<E>(E entity) where E : class;
        Task<int> UpdateAsync<E>(E entity) where E : class;

        int Update<E>(object subEntity, Expression<Func<E, bool>> where) where E : class;
        Task<int> UpdateAsync<E>(object subEntity, Expression<Func<E, bool>> where) where E : class;

        int Delete<E, K>(K key) where E : class, K where K : class;
        Task<int> DeleteAsync<E, K>(K key) where E : class, K where K : class;

        int Delete<E>(Expression<Func<E, bool>> where, object param = null) where E : class;
        Task<int> DeleteAsync<E>(Expression<Func<E, bool>> where, object param = null) where E : class;

        void Truncate<E>() where E : class;
        Task TruncateAsync<E>() where E : class;

        E GetByKey<E, K>(K key, Type type = null) where E : class, K where K : class;
        Task<E> GetByKeyAsync<E, K>(K key, Type type = null) where E : class, K where K : class;

        IEnumerable<E> Get<E>(
            Type type = null,
            Expression<Func<E, bool>> where = null, object param = null,
            Expression<Func<IEnumerable<E>, IEnumerable<E>>> orderBy = null) where E : class;
        Task<IEnumerable<E>> GetAsync<E>(
            Type type = null,
            Expression<Func<E, bool>> where = null, object param = null,
            Expression<Func<IEnumerable<E>, IEnumerable<E>>> orderBy = null) where E : class;

        T GetScalar<E, T>(
            Expression<Func<IEnumerable<E>, T>> expression,
            Expression<Func<E, bool>> where = null, object param = null) where E : class;
        Task<T> GetScalarAsync<E, T>(
            Expression<Func<IEnumerable<E>, T>> expression,
            Expression<Func<E, bool>> where = null, object param = null) where E : class;
    }
}