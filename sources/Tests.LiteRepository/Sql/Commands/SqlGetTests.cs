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
using System.Data.Common;
using LiteRepository.Sql.Models;
using System.Data;

namespace LiteRepository.Sql.Commands
{
    public class SqlGetTests
    {
        [Fact]
        public void Execute_NullEntity_Test()
        {
            var cmd = new SqlGet<Entity, Entity.Key>(Substitute.For<ISqlBuilder>());
            Assert.Throws<ArgumentNullException>(() => cmd.Execute(null, Substitute.For<DbConnection>()));
        }

        [Fact]
        public void Execute_NullDbConnection_Test()
        {
            var cmd = new SqlGet<Entity, Entity.Key>(Substitute.For<ISqlBuilder>());
            Assert.Throws<ArgumentNullException>(() => cmd.Execute(new Entity.Key(), null));
        }

        [Fact]
        public async void ExecuteAsync_NullEntity_Test()
        {
            var cmd = new SqlGet<Entity, Entity.Key>(Substitute.For<ISqlBuilder>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => cmd.ExecuteAsync(null, Substitute.For<DbConnection>()));
        }

        [Fact]
        public async void ExecuteAsync_NullDbConnection_Test()
        {
            var cmd = new SqlGet<Entity, Entity.Key>(Substitute.For<ISqlBuilder>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => cmd.ExecuteAsync(new Entity.Key(), null));
        }

        private static void CheckCommand(Entity.Key key, string sql, List<DbParameter> dbParameters, IDbCommand dbCommand)
        {
            dbCommand.Received(1).CommandText = sql;
            dbCommand.Received(2).CreateParameter();

            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(key.Cource) && (long)i.Value == key.Cource));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(key.Letter) && (char)i.Value == key.Letter));
        }

        private static Entity.Key GetEntityKey()
        {
            return new Entity.Key
            {
                Cource = 3,
                Letter = 'B'
            };
        }

        private static Entity GetEntity()
        {
            return new Entity
            {
                Cource = 3,
                Letter = 'B',
                FirstName = "Ivan",
                SecondName = "Petrov",
                Birthday = new DateTime(1991, 12, 26)
            };
        }

        private static string GetSql()
        {
            return "select " +
                "cource as Cource, " +
                "letter as Letter, " +
                "first_name as FirstName, " +
                "second_name as SecondName, " +
                "birthday as Birthday " +
                "from students " +
                "where cource=@cource and letter=@Letter";
        }

        [Fact]
        public void Execute_Entity_Test()
        {
            var key = GetEntityKey();
            var sql = GetSql();

            var sqlBuilder = Substitute.For<ISqlBuilder>();
            sqlBuilder.GetSelectSql().Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);

            var cmd = new SqlGet<Entity, Entity.Key>(sqlBuilder);
            var execResult = cmd.Execute(key, DbMocks.CreateConnection(dbCommand));

            CheckCommand(key, sql, dbParameters, dbCommand);
            ((IDbCommand)dbCommand).Received(1).ExecuteReader(Arg.Any<CommandBehavior>());
        }

        [Fact]
        public async void ExecuteAsync_Entity_Test()
        {
            var key = GetEntityKey();
            var sql = GetSql();

            var sqlBuilder = Substitute.For<ISqlBuilder>();
            sqlBuilder.GetSelectSql().Returns(sql);

            var dbDataReader = Substitute.For<DbDataReader>();
            dbDataReader.FieldCount.Returns(5);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteReaderAsync(Arg.Any<CommandBehavior>()).Returns(dbDataReader);
            
            var cmd = new SqlGet<Entity, Entity.Key>(sqlBuilder);
            var execResult = await cmd.ExecuteAsync(key, DbMocks.CreateConnection(dbCommand));

            CheckCommand(key, sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteReaderAsync(Arg.Any<CommandBehavior>());
        }
    }
}
