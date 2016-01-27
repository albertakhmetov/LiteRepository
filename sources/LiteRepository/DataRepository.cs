/*

Copyright 2016, Albert Akhmetov (email: akhmetov@live.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific

*/

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

        private volatile bool _isDisposed;
        private readonly object _disposerLock = new object();

        public bool IsDisposed
        {
            get { return _isDisposed; }
            private set { _isDisposed = value; }
        }

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

            IsDisposed = false;

            _insertedSubject = new Subject<E>();
            _updatedSubject = new Subject<E>();
            _deletedSubject = new Subject<K>();

            _db = db;
            _entityFactory = entityFactory;
        }

        ~DataRepository()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            lock (_disposerLock)
            {
                if (!IsDisposed)
                {
                    IsDisposed = true;
                    GC.SuppressFinalize(this);

                    _insertedSubject.OnCompleted();
                    _updatedSubject.OnCompleted();
                    _deletedSubject.OnCompleted();
                }
            }
        }

        private async Task<T> Exec<T>(Func<IDbConnection, Task<T>> action)
        {
            var connection = default(IDbConnection);
            try
            {
                connection = _db.OpenConnection();
                return await action(connection);
            }
            catch (DataException ex)
            {
                throw new DataRepositoryException("", ex);
            }
            finally
            {
                _db.CloseConnection(connection);
            }
        }

        private void CheckDisposed()
        {
            if (IsDisposed)
                throw new InvalidOperationException();
        }

        public async Task<E> InsertAsync(E entity, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            if (entity == default(E))
                throw new ArgumentNullException(nameof(entity));

            return await Exec<E>(async connection => await InsertAsync(entity, connection, cancellationToken));
        }

        private async Task<E> InsertAsync(E entity, IDbConnection connection, CancellationToken? cancellationToken)
        {
            CheckDisposed();

            var sql = Db.GetSqlGenerator<E>().InsertSql;
            var result = default(E);

            if (_entityFactory == null)
            {
                var execResult = await Db.GetSqlExecutor().ExecuteAsync<E>(connection, sql, entity, cancellationToken);
                if (execResult == 1)
                    result = entity;
            }
            else
            {
                var insertedRowId = await Db.GetSqlExecutor().QueryScalarAsync<E, long>(connection, sql, entity, cancellationToken);
                result = _entityFactory(entity, insertedRowId);
            }

            if (result != default(E))
                _insertedSubject.OnNext(result);
            return result;
        }

        public async Task<E> UpdateAsync(E entity, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            if (entity == default(E))
                throw new ArgumentNullException(nameof(entity));

            return await Exec<E>(async connection => await UpdateAsync(entity, connection, cancellationToken));
        }

        private async Task<E> UpdateAsync(E entity, IDbConnection connection, CancellationToken? cancellationToken)
        {
            CheckDisposed();

            var sql = Db.GetSqlGenerator<E>().UpdateSql;
            var execResult = await Db.GetSqlExecutor().ExecuteAsync<E>(connection, sql, entity, cancellationToken);

            if (execResult == 0)
                return default(E);
            else
            {
                _updatedSubject.OnNext(entity);
                return entity;
            }
        }

        public async Task<E> UpdateOrInsertAsync(E entity, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            if (entity == default(E))
                throw new ArgumentNullException(nameof(entity));

            return await Exec<E>(async connection =>
            {
                var execResult = await UpdateAsync(entity, connection, cancellationToken);
                if (execResult != null)
                    return execResult;
                else
                    return await InsertAsync(entity, connection, cancellationToken);
            });
        }

        public async Task<K> DeleteAsync(K key, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            if (key == default(K))
                throw new ArgumentNullException(nameof(key));

            return await Exec<K>(async connection => await DeleteAsync(key, connection, cancellationToken));
        }

        private async Task<K> DeleteAsync(K key, IDbConnection connection, CancellationToken? cancellationToken)
        {
            CheckDisposed();

            var sql = Db.GetSqlGenerator<E>().DeleteSql;
            var execResult = await Db.GetSqlExecutor().ExecuteAsync<K>(connection, sql, key, cancellationToken);
            if (execResult == 0)
                return default(K);
            else
            {
                _deletedSubject.OnNext(key);
                return key;
            }
        }

        public async Task<int> DeleteAllAsync(CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return await Exec<int>(async connection => await DeleteAllAsync(connection, cancellationToken));
        }

        private async Task<int> DeleteAllAsync(IDbConnection connection, CancellationToken? cancellationToken)
        {
            CheckDisposed();

            var sql = Db.GetSqlGenerator<E>().DeleteAllSql;
            var execResult = await Db.GetSqlExecutor().ExecuteAsync(connection, sql, cancellationToken);

            if (execResult > 0)
                _deletedSubject.OnNext(null);

            return execResult;
        }

        public async Task<E> GetAsync(K key, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            if (key == default(K))
                throw new ArgumentNullException(nameof(key));

            return await Exec<E>(async connection => await GetAsync(key, connection, cancellationToken));
        }

        private async Task<E> GetAsync(K key, IDbConnection connection, CancellationToken? cancellationToken)
        {
            CheckDisposed();

            var sql = Db.GetSqlGenerator<E>().SelectSql;
            var execResult = await Db.GetSqlExecutor().QueryAsync<K, E>(connection, sql, key, cancellationToken);

            return execResult.FirstOrDefault();
        }

        public async Task<IEnumerable<E>> GetAllAsync(CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return await Exec<IEnumerable<E>>(async connection => await GetAllAsync(connection, cancellationToken));
        }

        private async Task<IEnumerable<E>> GetAllAsync(IDbConnection connection, CancellationToken? cancellationToken)
        {
            CheckDisposed();

            var sql = Db.GetSqlGenerator<E>().SelectAllSql;
            var execResult = await Db.GetSqlExecutor().QueryAsync<E>(connection, sql, cancellationToken);

            return execResult;
        }

        public async Task<long> GetCountAsync(CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return await Exec<long>(async connection => await GetCountAsync(connection, cancellationToken));
        }

        private async Task<long> GetCountAsync(IDbConnection connection, CancellationToken? cancellationToken)
        {
            CheckDisposed();

            var sql = Db.GetSqlGenerator<E>().CountSql;
            var execResult = await Db.GetSqlExecutor().QueryScalarAsync<long>(connection, sql, cancellationToken);

            return execResult;
        }
    }
}
