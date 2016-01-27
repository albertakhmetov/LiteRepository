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
        private readonly ISqlExecutor _sqlExecutor;
        private readonly ISqlGenerator _sqlGenerator;
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
            _sqlExecutor = _db.GetSqlExecutor();
            _sqlGenerator = _db.GetSqlGenerator<E>();
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

                    if (_insertedSubject != null)
                        _insertedSubject.OnCompleted();
                    if (_updatedSubject != null)
                        _updatedSubject.OnCompleted();
                    if (_deletedSubject != null)
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

            var sql = _sqlGenerator.InsertSql;
            var commandType = _sqlGenerator.CommandType;
            var result = default(E);

            if (_entityFactory == null)
            {
                var execResult = await _sqlExecutor.ExecuteAsync<E>(connection, sql, entity, commandType, cancellationToken);
                if (execResult == 1)
                    result = entity;
            }
            else
            {
                var insertedRowId = await _sqlExecutor.QueryScalarAsync<E, long>(connection, sql, entity, commandType, cancellationToken);
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

            var sql = _sqlGenerator.UpdateSql;
            var commandType = _sqlGenerator.CommandType;
            var execResult = await _sqlExecutor.ExecuteAsync<E>(connection, sql, entity, commandType, cancellationToken);

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

            var sql = _sqlGenerator.DeleteSql;
            var commandType = _sqlGenerator.CommandType;
            var execResult = await _sqlExecutor.ExecuteAsync<K>(connection, sql, key, commandType, cancellationToken);
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

            var sql = _sqlGenerator.DeleteAllSql;
            var commandType = _sqlGenerator.CommandType;
            var execResult = await _sqlExecutor.ExecuteAsync(connection, sql, commandType, cancellationToken);

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

            var sql = _sqlGenerator.SelectSql;
            var commandType = _sqlGenerator.CommandType;
            var execResult = await _sqlExecutor.QueryAsync<K, E>(connection, sql, key, commandType, cancellationToken);

            return execResult.FirstOrDefault();
        }

        public async Task<IEnumerable<E>> GetAllAsync(CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return await Exec<IEnumerable<E>>(async connection => await GetAllAsync(connection, cancellationToken));
        }

        private async Task<IEnumerable<E>> GetAllAsync(IDbConnection connection, CancellationToken? cancellationToken)
        {
            CheckDisposed();

            var sql = _sqlGenerator.SelectAllSql;
            var commandType = _sqlGenerator.CommandType;
            var execResult = await _sqlExecutor.QueryAsync<E>(connection, sql, commandType, cancellationToken);

            return execResult;
        }

        public async Task<long> GetCountAsync(CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return await Exec<long>(async connection => await GetCountAsync(connection, cancellationToken));
        }

        private async Task<long> GetCountAsync(IDbConnection connection, CancellationToken? cancellationToken)
        {
            CheckDisposed();

            var sql = _sqlGenerator.CountSql;
            var commandType = _sqlGenerator.CommandType;
            var execResult = await _sqlExecutor.QueryScalarAsync<long>(connection, sql, commandType, cancellationToken);

            return execResult;
        }
    }
}
