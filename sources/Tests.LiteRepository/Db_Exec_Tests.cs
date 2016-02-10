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

namespace LiteRepository
{
    public class Db_Exec_Tests
    {
        private Db GetDb(DbConnection dbConnection)
        {
            return new Db(Substitute.For<SqlDialectBase>(), dbConnection);
        }

        private Db GetDb(Func<DbConnection> dbConnectionFactory)
        {
            return new Db(Substitute.For<SqlDialectBase>(), dbConnectionFactory);
        }

        [Fact]
        public void Connection_Opened_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(System.Data.ConnectionState.Open);
            var db = GetDb(dbConnection);

            var execResult = db.Exec<int>(connection => 42);
            Assert.Equal(42, execResult);

            dbConnection.Received(0).Open();
            dbConnection.Received(0).Close();
        }

        [Fact]
        public void Connection_Closed_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(System.Data.ConnectionState.Closed);
            var db = GetDb(dbConnection);

            var execResult = db.Exec<int>(connection => 42);
            Assert.Equal(42, execResult);

            dbConnection.Received(1).Open();
            dbConnection.Received(1).Close();
        }

        [Fact]
        public void Connection_Factory_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(System.Data.ConnectionState.Closed);
            var db = GetDb(() => dbConnection);

            var execResult = db.Exec<int>(connection => 42);
            Assert.Equal(42, execResult);

            dbConnection.Received(1).Open();
            dbConnection.Received(1).Close();
        }

        [Fact]
        public async void ConnectionAsync_Opened_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(System.Data.ConnectionState.Open);
            var db = GetDb(dbConnection);

            var execResult = await db.ExecAsync<int>(connection => Task<int>.FromResult(42));
            Assert.Equal(42, execResult);

            await dbConnection.Received(0).OpenAsync();
            dbConnection.Received(0).Close();
        }

        [Fact]
        public async void ConnectionAsync_Closed_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(System.Data.ConnectionState.Closed);
            var db = GetDb(dbConnection);

            var execResult = await db.ExecAsync<int>(connection => Task<int>.FromResult(42));
            Assert.Equal(42, execResult);

            await dbConnection.Received(1).OpenAsync();
            dbConnection.Received(1).Close();
        }

        [Fact]
        public async void ConnectionAsync_Factory_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(System.Data.ConnectionState.Closed);
            var db = GetDb(() => dbConnection);

            var execResult = await db.ExecAsync<int>(connection => Task<int>.FromResult(42));
            Assert.Equal(42, execResult);

            await dbConnection.Received(1).OpenAsync();
            dbConnection.Received(1).Close();
        }
    }
}
