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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Database
{
    public class DefaultDb : IDb
    {
        private readonly ISqlExecutor _sqlExecutor;
        private readonly Func<Type, ISqlGenerator> _sqlGeneratorFactory;
        private readonly ConcurrentDictionary<Type, ISqlGenerator> _sqlGenerators;

        private readonly IDbConnection _dbConnection;             
        private readonly Func<IDbConnection> _dbConnectionFactory;

        public DefaultDb(ISqlExecutor sqlExecutor, Func<Type, ISqlGenerator> sqlGeneratorFactory, Func<IDbConnection> dbConnectionFactory)
        {
            _sqlGenerators = new ConcurrentDictionary<Type, ISqlGenerator>();
            _sqlExecutor = sqlExecutor;
            _sqlGeneratorFactory = sqlGeneratorFactory;
            _dbConnection = null;
            _dbConnectionFactory = dbConnectionFactory;
        }

        public DefaultDb(ISqlExecutor sqlExecutor, Func<Type, ISqlGenerator> sqlGeneratorFactory, IDbConnection dbConnection)
        {
            _sqlGenerators = new ConcurrentDictionary<Type, ISqlGenerator>();
            _sqlExecutor = sqlExecutor;
            _sqlGeneratorFactory = sqlGeneratorFactory;
            _dbConnection = dbConnection;
            _dbConnectionFactory = null;
        }

        public IDbConnection OpenConnection()
        {
            if (_dbConnection != null)
                return _dbConnection;
            else
            {
                var dbConnection = _dbConnectionFactory();
                dbConnection.Open();

                return dbConnection;
            }
        }

        public void CloseConnection(IDbConnection dbConnection)
        {
            if (_dbConnection == null && dbConnection != null && dbConnection.State == ConnectionState.Open)
                dbConnection.Close();
        }

        public ISqlExecutor GetSqlExecutor()
        {
            return _sqlExecutor;
        }

        public ISqlGenerator GetSqlGenerator<E>() where E : class
        {
            return _sqlGenerators.GetOrAdd(typeof(E), _sqlGeneratorFactory);
        }
    }
}
