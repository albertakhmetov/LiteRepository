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
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Common
{
    public class Db : IDb
    {
        private readonly DbConnection _dbConnection;
        private readonly Func<DbConnection> _dbConnectionFactory;

        public Db(DbConnection dbConnection)
        {
            if (dbConnection == null)
                throw new ArgumentNullException(nameof(dbConnection));
            _dbConnection = dbConnection;
            _dbConnectionFactory = null;
        }

        public Db(Func<DbConnection> dbConnectionFactory)
        {
            if (dbConnectionFactory == null)
                throw new ArgumentNullException(nameof(dbConnectionFactory));
            _dbConnection = null;
            _dbConnectionFactory = dbConnectionFactory;
        }

        private DbConnection GetDbConnection()
        {
            var dbConnecton = _dbConnection ?? _dbConnectionFactory();
            if (dbConnecton == null)
                throw new InvalidOperationException("Connection can't be null");
            return dbConnecton;
        }

        public DbConnection OpenDbConnection()
        {
            var dbConnection = GetDbConnection();
            if (dbConnection.State == System.Data.ConnectionState.Closed)
                dbConnection.Open();
            return dbConnection;
        }

        public async Task<DbConnection> OpenDbConnectionAsync()
        {
            var dbConnection = GetDbConnection();
            if (dbConnection.State == System.Data.ConnectionState.Closed)
                await dbConnection.OpenAsync();
            return dbConnection;
        }

        public void CloseDbConnection(DbConnection dbConnection)
        {
            if (_dbConnectionFactory != null && dbConnection != null && dbConnection.State == System.Data.ConnectionState.Open)
                dbConnection.Close();
        }
    }
}
