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
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using LiteRepository.Common.Commands;

namespace LiteRepository.Sql.Commands
{
    public class SqlUpdate<E> : SqlCommandBase<E>, IUpdateCommand<E>
        where E : class
    {
        public SqlUpdate(ISqlBuilder sqlBuilder) : base(sqlBuilder)
        { }

        public int Execute(E entity, DbConnection dbConnection)
        {
            CheckNotNull(entity, nameof(entity));
            CheckNotNull(dbConnection, nameof(dbConnection));

            return dbConnection.Execute(SqlBuilder.GetUpdateSql(), param: entity);
        }

        public Task<int> ExecuteAsync(E entity, DbConnection dbConnection)
        {
            CheckNotNull(entity, nameof(entity));
            CheckNotNull(dbConnection, nameof(dbConnection));

            return dbConnection.ExecuteAsync(SqlBuilder.GetUpdateSql(), param: entity);
        }
    }
}
