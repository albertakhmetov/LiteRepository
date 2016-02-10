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
using LiteRepository.Models;
using System.Data.Common;
using System.Data;
using System.Threading;

namespace LiteRepository
{
    public class Db_Insert_Tests
    {
        private Db GetDb(SqlDialectBase sqlDialect = null, DbConnection dbConnection = null)
        {
            return new Db(
                sqlDialect ?? Substitute.For<SqlDialectBase>(),
                dbConnection ?? Substitute.For<DbConnection>());
        }

        [Fact]
        public void Insert_NullEntity_Test()
        {
            var db = GetDb();
            Assert.Throws<ArgumentNullException>(() => db.Insert<Entity>(null));
        }

        [Fact]
        public async void InsertAsync_NullEntity_Test()
        {
            var db = GetDb();
            await Assert.ThrowsAsync<ArgumentNullException>(() => db.InsertAsync<Entity>(null));
        }

        private static void CheckCommand(Entity entity, string sql, List<DbParameter> dbParameters, IDbCommand dbCommand)
        {
            dbCommand.Received(1).CommandText = sql;
            dbCommand.Received(6).CreateParameter();

            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(entity.Cource) && (long)i.Value == entity.Cource));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(entity.Letter) && (char)i.Value == entity.Letter));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(entity.LocalId) && (int)i.Value == entity.LocalId));
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
                LocalId = 10,
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
            return "insert into students (cource, letter, local_id, first_name, second_name, birthday) values (@cource, @Letter, @LocalId, @FirstName, @SecondName, @birthday)";
        }

        private static string GetIdentitySql()
        {
            // cource and birthday (lowercase) - it is not misstyping
            return "insert into students (first_name, second_name, birthday) values (@FirstName, @SecondName, @birthday)\r\nSELECT SCOPE_IDENTITY()";
        }

        [Fact]
        public void Insert_Entity_Test()
        {
            var entity = GetEntity();
            var sql = GetSql();

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Insert(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));

            var execResult = db.Insert(entity);
            Assert.Equal(entity, execResult);

            CheckCommand(entity, sql, dbParameters, dbCommand);
            dbCommand.Received(1).ExecuteNonQuery();
        }

        [Fact]
        public async void InsertAsync_Entity_Test()
        {
            var entity = GetEntity();
            var sql = GetSql();

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Insert(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));

            var execResult = await db.InsertAsync(entity);
            Assert.Equal(entity, execResult);

            CheckCommand(entity, sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteNonQueryAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public void Insert_IdentityEntity_Test()
        {
            var entity = GetIdentityEntity();
            var newId = 42;
            var sql = GetIdentitySql();

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Insert(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteScalar().Returns(newId);

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = db.Insert(entity);

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
        public async void InsertAsync_IdentityEntity_Test()
        {
            var entity = GetIdentityEntity();
            var newId = 42;
            var sql = GetIdentitySql();

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Insert(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteScalarAsync().Returns(newId);

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = await db.InsertAsync(entity);

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
