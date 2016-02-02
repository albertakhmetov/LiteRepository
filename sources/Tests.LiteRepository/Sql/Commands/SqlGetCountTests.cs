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

using Xunit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteRepository.Sql.Models;
using System.Data.Common;
using System.Data;

namespace LiteRepository.Sql.Commands
{
    public class SqlGetCountTests
    {
        [Fact]
        public void Execute_NullDbConnection_Test()
        {
            var cmd = new SqlGetCount<Entity>(Substitute.For<ISqlBuilder>());
            Assert.Throws<ArgumentNullException>(() => cmd.Execute(null));
        }

        [Fact]
        public async void ExecuteAsync_NullDbConnection_Test()
        {
            var cmd = new SqlGetCount<Entity>(Substitute.For<ISqlBuilder>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => cmd.ExecuteAsync(null));
        }

        private string GetSql()
        {
            return "select count(1) from students";
        }

        private static void CheckCommand(string sql, List<DbParameter> dbParameters, IDbCommand dbCommand)
        {
            dbCommand.Received(1).CommandText = sql;
            dbCommand.Received(0).CreateParameter();
        }

        [Fact]
        public void Execute_Entity_Test()
        {
            var sql = GetSql();

            var sqlBuilder = Substitute.For<ISqlBuilder>();
            sqlBuilder.GetCountSql().Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);

            var cmd = new SqlGetCount<Entity>(sqlBuilder);
            var execResult = cmd.Execute(DbMocks.CreateConnection(dbCommand));

            CheckCommand(sql, dbParameters, dbCommand);
            ((IDbCommand)dbCommand).Received(1).ExecuteScalar();
        }

        [Fact]
        public async void ExecuteAsync_Entity_Test()
        {
            var sql = GetSql();

            var sqlBuilder = Substitute.For<ISqlBuilder>();
            sqlBuilder.GetCountSql().Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);

            var cmd = new SqlGetCount<Entity>(sqlBuilder);
            var execResult = await cmd.ExecuteAsync(DbMocks.CreateConnection(dbCommand));

            CheckCommand(sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteScalarAsync();
        }
    }
}
