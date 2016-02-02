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
    public class SqlDeleteTests
    {
        [Fact]
        public void Execute_NullEntity_Test()
        {
            var cmd = new SqlDelete<Entity, Entity.Key>(Substitute.For<ISqlBuilder>());
            Assert.Throws<ArgumentNullException>(() => cmd.Execute(null, Substitute.For<DbConnection>()));
        }

        [Fact]
        public void Execute_NullDbConnection_Test()
        {
            var cmd = new SqlDelete<Entity, Entity.Key>(Substitute.For<ISqlBuilder>());
            Assert.Throws<ArgumentNullException>(() => cmd.Execute(new Entity.Key(), null));
        }

        [Fact]
        public async void ExecuteAsync_NullEntity_Test()
        {
            var cmd = new SqlDelete<Entity, Entity.Key>(Substitute.For<ISqlBuilder>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => cmd.ExecuteAsync(null, Substitute.For<DbConnection>()));
        }

        [Fact]
        public async void ExecuteAsync_NullDbConnection_Test()
        {
            var cmd = new SqlDelete<Entity, Entity.Key>(Substitute.For<ISqlBuilder>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => cmd.ExecuteAsync(new Entity.Key(), null));
        }

        private static Entity.Key GetEntityKey()
        {
            return new Entity.Key
            {
                Cource = 3,
                Letter = 'B'
            };
        }

        private static void CheckCommand(Entity.Key key, string sql, List<DbParameter> dbParameters, IDbCommand dbCommand)
        {
            dbCommand.Received(1).CommandText = sql;
            dbCommand.Received(2).CreateParameter();

            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(key.Cource) && (long)i.Value == key.Cource));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(key.Letter) && (char)i.Value == key.Letter));
        }

        private static string GetSql()
        {
            // cource and birthday (lowercase) - it is not misstyping
            return "delete from students where cource=@cource and letter=@Letter";
        }

        [Fact]
        public void Execute_Entity_Test()
        {
            var key = GetEntityKey();
            var sql = GetSql();
            var affectedRows = 1;

            var sqlBuilder = Substitute.For<ISqlBuilder>();
            sqlBuilder.GetDeleteByKeySql().Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteNonQuery().Returns(affectedRows);

            var cmd = new SqlDelete<Entity, Entity.Key>(sqlBuilder);
            var execResult = cmd.Execute(key, DbMocks.CreateConnection(dbCommand));

            Assert.Equal(1, execResult);

            CheckCommand(key, sql, dbParameters, dbCommand);
            dbCommand.Received(1).ExecuteNonQuery();
        }

        [Fact]
        public async void ExecuteAsync_Entity_Test()
        {
            var key = GetEntityKey();
            var sql = GetSql();
            var affectedRows = 1;

            var sqlBuilder = Substitute.For<ISqlBuilder>();
            sqlBuilder.GetDeleteByKeySql().Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteNonQueryAsync().Returns(affectedRows);

            var cmd = new SqlDelete<Entity, Entity.Key>(sqlBuilder);
            var execResult = await cmd.ExecuteAsync(key, DbMocks.CreateConnection(dbCommand));

            Assert.Equal(1, execResult);

            CheckCommand(key, sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteNonQueryAsync();
        }
    }
}
