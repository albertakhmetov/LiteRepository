﻿/*

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
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using LiteRepository.Common.Commands;

namespace LiteRepository.Sql.Commands
{
    public class SqlDelete<E, K> : SqlCommandBase<E>, IDeleteCommand<K>
        where E : class
        where K : class
    {
        public SqlDelete(ISqlBuilder sqlBuilder) : base(sqlBuilder)
        { }

        public int Execute(K key, DbConnection dbConnection)
        {
            CheckNotNull(key, nameof(key));
            CheckNotNull(dbConnection, nameof(dbConnection));

            return dbConnection.Execute(SqlBuilder.GetDeleteByKeySql(), param: key);
        }

        public Task<int> ExecuteAsync(K key, DbConnection dbConnection)
        {
            CheckNotNull(key, nameof(key));
            CheckNotNull(dbConnection, nameof(dbConnection));

            return dbConnection.ExecuteAsync(SqlBuilder.GetDeleteByKeySql(), param: key);
        }
    }
}