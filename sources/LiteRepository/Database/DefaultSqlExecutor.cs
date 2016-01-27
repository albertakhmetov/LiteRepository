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

        public Task<T> QueryScalarAsync<T>(IDbConnection connection, string sql, CommandType commandType, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<T>(() => connection.ExecuteScalar<T>(sql: sql, commandType: commandType), cancellationToken ?? CancellationToken.None);
        }

        public Task<T> QueryScalarAsync<Q, T>(IDbConnection connection, string sql, Q parameters, CommandType commandType, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<T>(() => connection.ExecuteScalar<T>(sql: sql, param: parameters, commandType: commandType), cancellationToken ?? CancellationToken.None);
        }

        public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, CommandType commandType, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<IEnumerable<T>>(() => connection.Query<T>(sql: sql, commandType: commandType), cancellationToken ?? CancellationToken.None);
        }

        public Task<IEnumerable<T>> QueryAsync<Q, T>(IDbConnection connection, string sql, Q parameters, CommandType commandType, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<IEnumerable<T>>(() => connection.Query<T>(sql: sql, param: parameters, commandType: commandType), cancellationToken ?? CancellationToken.None);
        }

        public Task<int> ExecuteAsync(IDbConnection connection, string sql, CommandType commandType, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<int>(() => connection.Execute(sql: sql, commandType: commandType), cancellationToken ?? CancellationToken.None);
        }

        public Task<int> ExecuteAsync<Q>(IDbConnection connection, string sql, Q parameters, CommandType commandType, CancellationToken? cancellationToken = default(CancellationToken?))
        {
            return _taskFactory.StartNew<int>(() => connection.Execute(sql: sql, param: parameters, commandType: commandType), cancellationToken ?? CancellationToken.None);
        }
    }
}
