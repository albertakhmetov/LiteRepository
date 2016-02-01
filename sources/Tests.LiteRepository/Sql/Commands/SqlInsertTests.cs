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
using System.Data;
using System.Data.Common;
using System.Threading;

namespace LiteRepository.Sql.Commands
{
    public class SqlInsertTests
    {
        [Fact]
        public void Execute_NullEntity_Test()
        {
            var cmd = new SqlInsert<Entity>(Substitute.For<ISqlBuilder>());
            Assert.Throws<ArgumentNullException>(() => cmd.Execute(null, Substitute.For<DbConnection>()));
        }

        [Fact]
        public void Execute_NullDbConnection_Test()
        {
            var cmd = new SqlInsert<Entity>(Substitute.For<ISqlBuilder>());
            Assert.Throws<ArgumentNullException>(() => cmd.Execute(new Entity(), null));
        }

        [Fact]
        public async void ExecuteAsync_NullEntity_Test()
        {
            var cmd = new SqlInsert<Entity>(Substitute.For<ISqlBuilder>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => cmd.ExecuteAsync(null, Substitute.For<DbConnection>()));
        }

        [Fact]
        public async void ExecuteAsync_NullDbConnection_Test()
        {
            var cmd = new SqlInsert<Entity>(Substitute.For<ISqlBuilder>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => cmd.ExecuteAsync(new Entity(), null));
        }

        private static void CheckCommand(Entity entity, string sql, List<DbParameter> dbParameters, IDbCommand dbCommand)
        {
            dbCommand.Received(1).CommandText = sql;
            dbCommand.Received(5).CreateParameter();

            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(entity.Cource) && (long)i.Value == entity.Cource));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(entity.Letter) && (char)i.Value == entity.Letter));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(entity.FirstName) && (string)i.Value == entity.FirstName));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(entity.SecondName) && (string)i.Value == entity.SecondName));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(entity.Birthday) && (DateTime)i.Value == entity.Birthday));
        }

        private static void CheckIdentityCommand(IdentityEntity entity, string sql, List<DbParameter> dbParameters, IDbCommand dbCommand)
        {
            dbCommand.Received(1).CommandText = sql;
            dbCommand.Received(3).CreateParameter();

            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(entity.FirstName) && (string)i.Value == entity.FirstName));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(entity.SecondName) && (string)i.Value == entity.SecondName));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(entity.Birthday) && (DateTime)i.Value == entity.Birthday));
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

        private static IdentityEntity GetIdentityEntity()
        {
            return new IdentityEntity
            {
                Id = 0,
                FirstName = "Ivan",
                SecondName = "Petrov",
                Birthday = new DateTime(1991, 12, 26)
            };
        }


        private static string GetSql()
        {
            // cource and birthday (lowercase) - it is not misstyping
            return "insert into students (cource, letter, first_name, second_name, birthday) values (@cource, @Letter, @FirstName, @SecondName, @birthday)";
        }

        private static string GetIdentitySql()
        {
            // cource and birthday (lowercase) - it is not misstyping
            return "insert into students (first_name, second_name, birthday) values (@FirstName, @SecondName, @birthday)\r\nSELECT SCOPE_IDENTITY()";
        }

        [Fact]
        public void Execute_Entity_Test()
        {
            var entity = GetEntity();
            var sql = GetSql();

            var sqlBuilder = Substitute.For<ISqlBuilder>();
            sqlBuilder.GetInsertSql().Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);

            var cmd = new SqlInsert<Entity>(sqlBuilder);
            var execResult = cmd.Execute(entity, DbMocks.CreateConnection(dbCommand));

            Assert.Equal(entity, execResult);

            CheckCommand(entity, sql, dbParameters, dbCommand);
            dbCommand.Received(1).ExecuteNonQuery();
        }

        [Fact]
        public async void ExecuteAsync_Entity_Test()
        {
            var entity = GetEntity();
            var sql = GetSql();

            var sqlBuilder = Substitute.For<ISqlBuilder>();
            sqlBuilder.GetInsertSql().Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);

            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(ConnectionState.Open);
            dbConnection.CreateCommand().Returns(dbCommand);

            var cmd = new SqlInsert<Entity>(sqlBuilder);
            var execResult = await cmd.ExecuteAsync(entity, DbMocks.CreateConnection(dbCommand));

            Assert.Equal(entity, execResult);

            CheckCommand(entity, sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteNonQueryAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public void Execute_IdentityEntity_Test()
        {
            var entity = GetIdentityEntity();
            var newId = 42;
            var sql = GetIdentitySql();

            var sqlBuilder = Substitute.For<ISqlBuilder>();
            sqlBuilder.GetInsertSql().Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteScalar().Returns(newId);

            var cmd = new SqlInsert<IdentityEntity>(sqlBuilder);
            var execResult = cmd.Execute(entity, DbMocks.CreateConnection(dbCommand));

            Assert.NotEqual(entity, execResult);
            Assert.NotEqual(entity.Id, execResult.Id);
            Assert.Equal(entity.FirstName, execResult.FirstName);
            Assert.Equal(entity.SecondName, execResult.SecondName);
            Assert.Equal(entity.Birthday, execResult.Birthday);
            Assert.Equal(newId, execResult.Id);

            CheckIdentityCommand(entity, sql, dbParameters, dbCommand);
            dbCommand.Received(1).ExecuteScalar();
        }

        [Fact]
        public async void ExecuteAsync_IdentityEntity_Test()
        {
            var entity = GetIdentityEntity();
            var newId = 42;
            var sql = GetIdentitySql();

            var sqlBuilder = Substitute.For<ISqlBuilder>();
            sqlBuilder.GetInsertSql().Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteScalarAsync().Returns(newId);

            var cmd = new SqlInsert<IdentityEntity>(sqlBuilder);
            var execResult = await cmd.ExecuteAsync(entity, DbMocks.CreateConnection(dbCommand));

            Assert.NotEqual(entity, execResult);
            Assert.NotEqual(entity.Id, execResult.Id);
            Assert.Equal(entity.FirstName, execResult.FirstName);
            Assert.Equal(entity.SecondName, execResult.SecondName);
            Assert.Equal(entity.Birthday, execResult.Birthday);
            Assert.Equal(newId, execResult.Id);

            CheckIdentityCommand(entity, sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteScalarAsync();
        }
    }
}
