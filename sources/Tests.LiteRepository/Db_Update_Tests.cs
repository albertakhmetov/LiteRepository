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
    public class Db_Update_Tests
    {
        private Db GetDb(ISqlDialect sqlDialect = null, DbConnection dbConnection = null)
        {
            return new Db(
                sqlDialect ?? Substitute.For<ISqlDialect>(),
                dbConnection ?? Substitute.For<DbConnection>());
        }

        [Fact]
        public void Update_NullEntity_Test()
        {
            var db = GetDb();
            Assert.Throws<ArgumentNullException>(() => db.Update<Entity>(null));
        }


        [Fact]
        public async void UpdateAsync_NullEntity_Test()
        {
            var db = GetDb();
            await Assert.ThrowsAsync<ArgumentNullException>(() => db.UpdateAsync<Entity>(null));
        }

        private static Entity GetEntity()
        {
            return new Entity
            {
                Cource = 3,
                Letter = 'B',
                LocalId = 42,
                FirstName = "Ivan",
                SecondName = "Petrov",
                Birthday = new DateTime(1991, 12, 26)
            };
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

        private static string GetSql()
        {
            // cource and birthday (lowercase) - it is not misstyping
            return "update students set first_name=@FirstName, second_name=@SecondName, birthday=@birthday where cource=@cource and letter=@Letter and local_id = @LocalId";
        }

        [Fact]
        public void Update_Entity_Test()
        {
            var entity = GetEntity();
            var sql = GetSql();
            var affectedRows = 1;

            var sqlDialect = Substitute.For<ISqlDialect>();
            sqlDialect.Update(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteNonQuery().Returns(affectedRows);

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = db.Update(entity);

            Assert.Equal(1, execResult);

            CheckCommand(entity, sql, dbParameters, dbCommand);
            dbCommand.Received(1).ExecuteNonQuery();
        }

        [Fact]
        public async void UpdateAsync_Entity_Test()
        {
            var entity = GetEntity();
            var sql = GetSql();
            var affectedRows = 1;

            var sqlDialect = Substitute.For<ISqlDialect>();
            sqlDialect.Update(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(sql);

            var dbParameters = new List<DbParameter>();
            var dbCommand = DbMocks.CreateCommand(dbParameters);
            dbCommand.ExecuteNonQueryAsync().Returns(affectedRows);

            var db = GetDb(sqlDialect, DbMocks.CreateConnection(dbCommand));
            var execResult = await db.UpdateAsync(entity);

            Assert.Equal(1, execResult);

            CheckCommand(entity, sql, dbParameters, dbCommand);
            await dbCommand.Received(1).ExecuteNonQueryAsync();
        }
    }
}
