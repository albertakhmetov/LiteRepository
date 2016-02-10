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
using System.Linq.Expressions;
using System.Data;

namespace LiteRepository
{
    public class Db_Delete_Tests
    {
        private Db GetDb(SqlDialectBase sqlDialect = null, DbConnection dbConnection = null)
        {
            return new Db(
                sqlDialect ?? Substitute.For<SqlDialectBase>(),
                dbConnection ?? Substitute.For<DbConnection>());
        }

        [Fact]
        public void Delete_NullKey_Test()
        {
            var db = GetDb();
            Assert.Throws<ArgumentNullException>(() => db.Delete<Entity, EntityKey>(null));
        }

        [Fact]
        public async void DeleteAsync_NullKey_Test()
        {
            var db = GetDb();
            await Assert.ThrowsAsync<ArgumentNullException>(() => db.DeleteAsync<Entity, EntityKey>(null));
        }

        [Fact]
        public void Delete_NullWhere_Test()
        {
            var db = GetDb();
            Assert.Throws<ArgumentNullException>(() => db.Delete<Entity>(null as Expression<Func<Entity, bool>>));
        }

        [Fact]
        public async void DeleteAsync_NullWhere_Test()
        {
            var db = GetDb();
            await Assert.ThrowsAsync<ArgumentNullException>(() => db.DeleteAsync<Entity>(null as Expression<Func<Entity, bool>>));
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
            return "delete from students where cource = @Cource and letter = @Letter and localId = @LocalId";
        }

        private static string GetShortSql()
        {
            return "delete from students where cource = @LastCource";
        }

        [Fact]
        public void Delete_Key_Test()
        {
            var key = GetEntityKey();
            var sql = GetSql();
            var affectedRows = 1;

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Parameter("Cource").Returns("@Cource");
            sqlDialect.Parameter("Letter").Returns("@Letter");
            sqlDialect.Parameter("LocalId").Returns("@LocalId");
            sqlDialect.Delete(
                Arg.Any<string>(),
                "cource = @Cource AND letter = @Letter AND local_id = @LocalId").Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteNonQuery().Returns(affectedRows);

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = db.Delete<Entity, EntityKey>(key);

            Assert.Equal(1, execResult);

            CheckCommand(key, sql, dbParameters, dbCommand);
            dbCommand.Received(1).ExecuteNonQuery();
        }

        [Fact]
        public async void DeleteAsync_Key_Test()
        {
            var key = GetEntityKey();
            var sql = GetSql();
            var affectedRows = 1;

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Parameter("Cource").Returns("@Cource");
            sqlDialect.Parameter("Letter").Returns("@Letter");
            sqlDialect.Parameter("LocalId").Returns("@LocalId");
            sqlDialect.Delete(
                Arg.Any<string>(),
                "cource = @Cource AND letter = @Letter AND local_id = @LocalId").Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteNonQueryAsync().Returns(affectedRows);

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = await db.DeleteAsync<Entity, EntityKey>(key);

            Assert.Equal(1, execResult);

            CheckCommand(key, sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteNonQueryAsync();
        }

        [Fact]
        public void Delete_Where_Test()
        {
            var key = GetEntityKey();
            var sql = GetShortSql();
            var affectedRows = 1;

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Parameter("LastCource").Returns("@LastCource");
            sqlDialect.Delete(
                Arg.Any<string>(),
                "cource = @LastCource").Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteNonQuery().Returns(affectedRows);

            var p = new { LastCource = 1L };

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = db.Delete<Entity>(i => i.Cource == p.LastCource, p);

            Assert.Equal(1, execResult);

            CheckShortCommand(key, sql, dbParameters, dbCommand);
            dbCommand.Received(1).ExecuteNonQuery();
        }

        [Fact]
        public async void DeleteAsync_Where_Test()
        {
            var key = GetEntityKey();
            var sql = GetShortSql();
            var affectedRows = 1;

            var sqlDialect = Substitute.For<SqlDialectBase>();
            sqlDialect.Parameter("LastCource").Returns("@LastCource");
            sqlDialect.Delete(
                Arg.Any<string>(),
                "cource = @LastCource").Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteNonQueryAsync().Returns(affectedRows);

            var p = new { LastCource = 1L };

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = await db.DeleteAsync<Entity>(i => i.Cource == p.LastCource, p);

            Assert.Equal(1, execResult);

            CheckShortCommand(key, sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteNonQueryAsync();
        }
    }
}
