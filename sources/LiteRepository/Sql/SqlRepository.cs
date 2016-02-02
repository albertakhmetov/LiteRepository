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

using LiteRepository.Common;
using LiteRepository.Common.Commands;
using LiteRepository.Sql.Commands;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Sql
{
    public class SqlRepository<E, K> : IRepository<E, K>
        where E : class
        where K : class
    {
        private readonly SqlInsert<E> _insertCommand;
        private readonly SqlUpdate<E> _updateCommand;
        private readonly SqlDelete<E, K> _deleteCommand;
        private readonly SqlGet<E, K> _getCommand;
        private readonly SqlGetCount<E> _getCountCommand;

        public IDb Db
        {
            get; private set;
        }

        public SqlRepository(IDb db, ISqlBuilder sqlBuilder)
        {
            Db = db;

            _insertCommand = new SqlInsert<E>(sqlBuilder);
            _updateCommand = new SqlUpdate<E>(sqlBuilder);
            _deleteCommand = new SqlDelete<E, K>(sqlBuilder);
            _getCommand = new SqlGet<E, K>(sqlBuilder);
            _getCountCommand = new SqlGetCount<E>(sqlBuilder);
        }

        private async Task<T> Exec<T>(Func<DbConnection, Task<T>> operation)
        {
            var dbConnection = default(DbConnection);

            try
            {
                dbConnection = await Db.OpenDbConnectionAsync();
                return await operation(dbConnection);
            }
            finally
            {
                Db.CloseDbConnection(dbConnection);
            }
        }

        public Task<E> InsertAsync(E entity)
        {
            return Exec(dbConnection => _insertCommand.ExecuteAsync(entity, dbConnection));
        }

        public Task<int> UpdateAsync(E entity)
        {
            return Exec(dbConnection => _updateCommand.ExecuteAsync(entity, dbConnection));
        }

        public Task<int> DeleteAsync(K key)
        {
            return Exec(dbConnection => _deleteCommand.ExecuteAsync(key, dbConnection));
        }

        public Task<E> GetAsync(K key)
        {
            return Exec(dbConnection => _getCommand.ExecuteAsync(key, dbConnection));
        }

        public Task<long> GetCountAsync()
        {
            return Exec(dbConnection => _getCountCommand.ExecuteAsync(dbConnection));
        }
    }
}
