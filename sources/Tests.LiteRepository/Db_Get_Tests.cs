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
using LiteRepository.Models;
using System.Data;

namespace LiteRepository
{
    public class Db_Get_Tests
    {
        private Db GetDb(SqlDialectBase sqlDialect = null, DbConnection dbConnection = null)
        {
            return new Db(
                sqlDialect ?? Substitute.For<SqlDialectBase>(),
                dbConnection ?? Substitute.For<DbConnection>());
        }

        [Fact]
        public void GetByKey_NullKey_Test()
        {
            var db = GetDb();
            Assert.Throws<ArgumentNullException>(() => db.GetByKey<Entity, EntityKey>(null));
        }

        [Fact]
        public async void GetByKeyAsync_NullKey_Test()
        {
            var db = GetDb();
            await Assert.ThrowsAsync<ArgumentNullException>(() => db.GetByKeyAsync<Entity, EntityKey>(null));
        }

        [Fact]
        public void GetScalar_NullExpression_Test()
        {
            var db = GetDb();
            Assert.Throws<ArgumentNullException>(() => db.GetScalar<Entity, int>(null));
        }

        [Fact]
        public async void GetScalarAsync_NullExpression_Test()
        {
            var db = GetDb();
            await Assert.ThrowsAsync<ArgumentNullException>(() => db.GetScalarAsync<Entity, int>(null));
        }

        private static Entity GetEntity()
        {
            return new Entity
            {
                Cource = 3,
                Letter = 'B',
                LocalId = 12,
                FirstName = "Ivan",
                SecondName = "Petrov",
                Birthday = new DateTime(1991, 12, 26)
            };
        }

        private static EntityKey GetEntityKey()
        {
            return new EntityKey
            {
                Cource = 3,
                Letter = 'B',
                LocalId = 12,
            };
        }

        private static void CheckCommand(EntityKey key, string sql, List<DbParameter> dbParameters, IDbCommand dbCommand)
        {
            dbCommand.Received(1).CommandText = sql;
            dbCommand.Received(3).CreateParameter();

            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(key.Cource) && (long)i.Value == key.Cource));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(key.Letter) && (char)i.Value == key.Letter));
            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == nameof(key.LocalId) && (int)i.Value == key.LocalId));
        }

        private static void CheckShortCommand(object param, string sql, List<DbParameter> dbParameters, IDbCommand dbCommand)
        {
            dbCommand.Received(1).CommandText = sql;
            dbCommand.Received(1).CreateParameter();

            Assert.Equal(1, dbParameters.Count(i => i.ParameterName == "LastCource" && (long)i.Value == 1));
        }

        private static string GetSql()
        {
            return "select " +
                "first_name as FirstName, " +
                "second_name as SecondName " +
                "from students " +
                "where cource=@cource and letter=@Letter and local_id=@LocalId";
        }

        private static string GetShortSql()
        {
            return "select " +
                "first_name as FirstName, " +
                "second_name as SecondName " +
                "from students " +
                "where cource=@LastCource order by letter";
        }

        private static string GetScalarSql()
        {
            return "select " +
                "count(1)" +
                "from students " +
                "where cource=@LastCource";
        }

        [Fact]
        public void GetByKey_SubEntity_Test()
        {
            var key = GetEntityKey();
            var entity = GetEntity();
            var sql = GetSql();

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Parameter("Cource").Returns("@Cource");
            sqlDialect.Parameter("Letter").Returns("@Letter");
            sqlDialect.Parameter("LocalId").Returns("@LocalId");
            sqlDialect.Select(
                Arg.Any<string>(),
                "first_name AS FirstName, second_name AS SecondName",
                "cource = @Cource AND letter = @Letter AND local_id = @LocalId",
                string.Empty).Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);

            var p = new { entity.FirstName, entity.SecondName };

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = db.GetByKey<Entity, EntityKey>(key, p.GetType());

            CheckCommand(key, sql, dbParameters, dbCommand);
            ((IDbCommand)dbCommand).Received(1).ExecuteReader(Arg.Any<CommandBehavior>());
        }

        [Fact]
        public async void GetByKeyAsync_SubEntity_Test()
        {
            var key = GetEntityKey();
            var entity = GetEntity();
            var sql = GetSql();

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Parameter("Cource").Returns("@Cource");
            sqlDialect.Parameter("Letter").Returns("@Letter");
            sqlDialect.Parameter("LocalId").Returns("@LocalId");
            sqlDialect.Select(
                Arg.Any<string>(),
                "first_name AS FirstName, second_name AS SecondName",
                "cource = @Cource AND letter = @Letter AND local_id = @LocalId",
                string.Empty).Returns(sql);

            var dbDataReader = Substitute.For<DbDataReader>();
            dbDataReader.FieldCount.Returns(2);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteReaderAsync(Arg.Any<CommandBehavior>()).Returns(dbDataReader);

            var p = new { entity.FirstName, entity.SecondName };

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = await db.GetByKeyAsync<Entity, EntityKey>(key, p.GetType());

            CheckCommand(key, sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteReaderAsync(Arg.Any<CommandBehavior>());
        }

        [Fact]
        public void Get_SubEntity_Test()
        {
            var entity = GetEntity();
            var sql = GetShortSql();

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Parameter("LastCource").Returns("@LastCource");
            sqlDialect.Select(
                Arg.Any<string>(),
                "first_name AS FirstName, second_name AS SecondName",
                "cource = @LastCource",
                "letter").Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);

            var p = new { entity.FirstName, entity.SecondName };
            var param = new { LastCource = 1L };

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = db.Get<Entity>(
                p.GetType(),
                i => i.Cource == param.LastCource, param,
                i => i.OrderBy(x => x.Letter));

            CheckShortCommand(param, sql, dbParameters, dbCommand);
            ((IDbCommand)dbCommand).Received(1).ExecuteReader(Arg.Any<CommandBehavior>());
        }

        [Fact]
        public async void GetAsync_SubEntity_Test()
        {
            var entity = GetEntity();
            var sql = GetShortSql();

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Parameter("LastCource").Returns("@LastCource");
            sqlDialect.Select(
                Arg.Any<string>(),
                "first_name AS FirstName, second_name AS SecondName",
                "cource = @LastCource",
                "letter").Returns(sql);

            var dbDataReader = Substitute.For<DbDataReader>();
            dbDataReader.FieldCount.Returns(2);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteReaderAsync(Arg.Any<CommandBehavior>()).Returns(dbDataReader);

            var p = new { entity.FirstName, entity.SecondName };
            var param = new { LastCource = 1L };

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = await db.GetAsync<Entity>(
                p.GetType(),
                i => i.Cource == param.LastCource, param,
                i => i.OrderBy(x => x.Letter));

            CheckShortCommand(param, sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteReaderAsync(Arg.Any<CommandBehavior>());
        }

        [Fact]
        public void GetScalar_Test()
        {
            var entity = GetEntity();
            var sql = GetScalarSql();

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Parameter("LastCource").Returns("@LastCource");
            sqlDialect.SelectScalar(
                Arg.Any<string>(),
                "COUNT(1)",
                "cource = @LastCource").Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);

            var p = new { entity.FirstName, entity.SecondName };
            var param = new { LastCource = 1L };

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = db.GetScalar<Entity, int>(
                i => i.Count(),
                i => i.Cource == param.LastCource, param);

            CheckShortCommand(param, sql, dbParameters, dbCommand);
            ((IDbCommand)dbCommand).Received(1).ExecuteScalar();
        }

        [Fact]
        public async void GetScalarAsync_Test()
        {
            var entity = GetEntity();
            var sql = GetScalarSql();

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Parameter("LastCource").Returns("@LastCource");
            sqlDialect.SelectScalar(
                Arg.Any<string>(),
                "COUNT(1)",
                "cource = @LastCource").Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);

            var p = new { entity.FirstName, entity.SecondName };
            var param = new { LastCource = 1L };

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = await db.GetScalarAsync<Entity, int>(
                i => i.Count(),
                i => i.Cource == param.LastCource, param);

            CheckShortCommand(param, sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteScalarAsync();
        }
    }
}
