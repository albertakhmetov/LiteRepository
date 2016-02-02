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
using System.Threading.Tasks;
using Dapper;
using System.Data.Common;
using LiteRepository.Common.Commands;
using LiteRepository.Common;

namespace LiteRepository.Sql.Commands
{
    public class SqlInsert<E> : SqlCommandBase<E>, IInsertCommand<E>
        where E : class
    {
        public SqlInsert(ISqlBuilder sqlBuilder) : base(sqlBuilder)
        { }

        public E Execute(E entity, DbConnection dbConnection)
        {
            CheckNotNull(entity, nameof(entity));
            CheckNotNull(dbConnection, nameof(dbConnection));

            if (entity is IIdentityEntity)
            {
                var nextId = dbConnection.ExecuteScalar<long>(SqlBuilder.GetInsertSql(), param: entity);
                return (E)(entity as IIdentityEntity).UpdateId(nextId);
            }
            else
                dbConnection.Execute(SqlBuilder.GetInsertSql(), param: entity);

            return entity;
        }

        public async Task<E> ExecuteAsync(E entity, DbConnection dbConnection)
        {
            CheckNotNull(entity, nameof(entity));
            CheckNotNull(dbConnection, nameof(dbConnection));

            if (entity is IIdentityEntity)
            {
                var nextId = await dbConnection.ExecuteScalarAsync<long>(SqlBuilder.GetInsertSql(), param: entity);
                return (E)(entity as IIdentityEntity).UpdateId(nextId);
            }
            else
                await dbConnection.ExecuteAsync(SqlBuilder.GetInsertSql(), param: entity);

            return entity;
        }
    }
}
