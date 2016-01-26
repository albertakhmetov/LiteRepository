/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using LiteRepository.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteRepository
{
    public class DataRepository<E, K> : IDataRepository<E, K>
        where E : class
        where K : class
    {
        private readonly IDb _db;
        private readonly Func<E, long, E> _entityFactory;

        private readonly Subject<E> _insertedSubject;
        private readonly Subject<E> _updatedSubject;
        private readonly Subject<K> _deletedSubject;

        public IDb Db
        {
            get { return _db; }
        }

        public IObservable<E> InsertedObservable
        {
            get { return _insertedSubject; }
        }

        public IObservable<E> UpdatedObservable
        {
            get { return _updatedSubject; }
        }

        public IObservable<K> DeletedObservable
        {
            get { return _deletedSubject; }
        }

        public DataRepository(IDb db, Func<E, long, E> entityFactory = null)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            _insertedSubject = new Subject<E>();
            _updatedSubject = new Subject<E>();
            _deletedSubject = new Subject<K>();

            _db = db;
            _entityFactory = entityFactory;
        }

        private async Task<T> Exec<T>(Func<IDbConnection, Task<T>> action)
        {
            var connection = default(IDbConnection);
            try
            {
                connection = _db.OpenConnection();
                return await action(connection);
            }
            catch (Exception ex)
            {
                throw new DataRepositoryException("", ex);
            }
            finally
            {
                _db.CloseConnection(connection);
            }
        }

        public async Task<E> InsertAsync(E entity, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            if (entity == default(E))
                throw new ArgumentNullException(nameof(entity));

            return await Exec<E>(async connection => await InsertAsync(entity, connection, cancellationToken));
        }

        private async Task<E> InsertAsync(E entity, IDbConnection connection, CancellationToken? cancellationToken)
        {
            var sql = Db.GetSqlGenerator<E>().InsertSql;
            var execResult = await Db.GetSqlExecutor().ExecuteAsync<E>(connection, sql, entity, cancellationToken);

            if (execResult == 0)
                return default(E);
            else if (_entityFactory != null)
            {
                var insertedRowId = await Db.GetSqlExecutor().GetLastInsertedRowIdAsync(connection);
                return _entityFactory(entity, insertedRowId);
            }
            else
                return entity;
        }

        public async Task<int> UpdateAsync(E entity, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            if (entity == default(E))
                throw new ArgumentNullException(nameof(entity));

            return await Exec<int>(async connection => await UpdateAsync(entity, connection, cancellationToken));
        }

        private async Task<int> UpdateAsync(E entity, IDbConnection connection, CancellationToken? cancellationToken)
        {
            var sql = Db.GetSqlGenerator<E>().UpdateSql;
            var execResult = await Db.GetSqlExecutor().ExecuteAsync<E>(connection, sql, entity, cancellationToken);

            return execResult;
        }

        public async Task<E> UpdateOrInsertAsync(E entity, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            if (entity == default(E))
                throw new ArgumentNullException(nameof(entity));

            return await Exec<E>(async connection =>
            {
                var execResult = await UpdateAsync(entity, connection, cancellationToken);
                if (execResult > 0)
                    return entity;
                else
                    return await InsertAsync(entity, connection, cancellationToken);
            });
        }

        public async Task<int> DeleteAsync(K key, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            if (key == default(K))
                throw new ArgumentNullException(nameof(key));

            return await Exec<int>(async connection =>
            {
                var sql = Db.GetSqlGenerator<E>().DeleteSql;
                var execResult = await Db.GetSqlExecutor().ExecuteAsync<K>(connection, sql, key, cancellationToken);

                return execResult;
            });
        }

        public async Task<int> DeleteAllAsync(CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return await Exec<int>(async connection =>
            {
                var sql = Db.GetSqlGenerator<E>().DeleteAllSql;
                var execResult = await Db.GetSqlExecutor().ExecuteAsync(connection, sql, cancellationToken);

                return execResult;
            });
        }

        public async Task<E> GetAsync(K key, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            if (key == default(K))
                throw new ArgumentNullException(nameof(key));

            return await Exec<E>(async connection =>
            {
                var sql = Db.GetSqlGenerator<E>().SelectSql;
                var execResult = await Db.GetSqlExecutor().QueryAsync<E, K>(connection, sql, key, cancellationToken);

                return execResult.FirstOrDefault();
            });
        }

        public async Task<IEnumerable<E>> GetAllAsync(CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return await Exec<IEnumerable<E>>(async connection =>
            {
                var sql = Db.GetSqlGenerator<E>().SelectAllSql;
                var execResult = await Db.GetSqlExecutor().QueryAsync<E>(connection, sql, cancellationToken);

                return execResult;
            });
        }

        public async Task<long> GetCountAsync(CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return await Exec<long>(async connection =>
            {
                var sql = Db.GetSqlGenerator<E>().CountSql;
                var execResult = await Db.GetSqlExecutor().QueryScalarAsync<long>(connection, sql, cancellationToken);

                return execResult;
            });
        }

    }
}
