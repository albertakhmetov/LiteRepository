/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace LiteRepository.Database
{
    public class DefaultSqlExecutor : ISqlExecutor
    {
        private readonly TaskFactory _taskFactory;

        public DefaultSqlExecutor(TaskScheduler taskScheduler)
        {
            if (taskScheduler == null)
                throw new ArgumentNullException(nameof(taskScheduler));
            _taskFactory = new TaskFactory(taskScheduler);
        }

        public Task<T> QueryScalarAsync<T>(IDbConnection connection, string sql, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<T>(() => connection.ExecuteScalar<T>(sql), cancellationToken ?? CancellationToken.None);
        }

        public Task<T> QueryScalarAsync<Q, T>(IDbConnection connection, string sql, Q parameters, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<T>(() => connection.ExecuteScalar<T>(sql), cancellationToken ?? CancellationToken.None);
        }

        public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<IEnumerable<T>>(() => connection.Query<T>(sql), cancellationToken ?? CancellationToken.None);
        }

        public Task<IEnumerable<T>> QueryAsync<Q, T>(IDbConnection connection, string sql, Q parameters, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<IEnumerable<T>>(() => connection.Query<T>(sql, parameters), cancellationToken ?? CancellationToken.None);
        }

        public Task<int> ExecuteAsync(IDbConnection connection, string sql, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<int>(() => connection.Execute(sql), cancellationToken ?? CancellationToken.None);
        }

        public Task<int> ExecuteAsync<Q>(IDbConnection connection, string sql, Q parameters, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<int>(() => connection.Execute(sql, parameters), cancellationToken ?? CancellationToken.None);
        }
    }
}
