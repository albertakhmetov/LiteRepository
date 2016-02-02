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
using System.Data;

namespace LiteRepository.Common
{
    public class DbTests
    {
        [Fact]
        public void Ctor_Null_Test()
        {
            Assert.Throws<ArgumentNullException>(() => new Db(default(DbConnection)));
            Assert.Throws<ArgumentNullException>(() => new Db(default(Func<DbConnection>)));
        }

        [Fact]
        public void OpenDbConnection_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(ConnectionState.Open);

            var db = new Db(dbConnection);

            var execResult = db.OpenDbConnection();
            Assert.Equal(dbConnection, execResult);

            dbConnection.Received(0).Open();
        }

        [Fact]
        public void OpenDbConnection_Closed_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(ConnectionState.Closed);

            var db = new Db(dbConnection);

            var execResult = db.OpenDbConnection();
            Assert.Equal(dbConnection, execResult);

            dbConnection.Received(1).Open();
        }

        [Fact]
        public async void OpenDbConnectionAsync_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(ConnectionState.Open);

            var db = new Db(dbConnection);

            var execResult = await db.OpenDbConnectionAsync();
            Assert.Equal(dbConnection, execResult);

            await dbConnection.Received(0).OpenAsync();
        }

        [Fact]
        public async void OpenDbConnectionAsync_Closed_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(ConnectionState.Closed);

            var db = new Db(dbConnection);

            var execResult = await db.OpenDbConnectionAsync();
            Assert.Equal(dbConnection, execResult);

            await dbConnection.Received(1).OpenAsync();
        }

        [Fact]
        public void CloseDbConnection_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(ConnectionState.Open);

            var db = new Db(dbConnection);

            db.CloseDbConnection(dbConnection);
            dbConnection.Received(0).Close();
        }

        [Fact]
        public void OpenDbConnection_Factory_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            var factory = Substitute.For<Func<DbConnection>>();
            factory.Invoke().Returns(dbConnection);

            var db = new Db(factory);

            var execResult = db.OpenDbConnection();
            Assert.Equal(dbConnection, execResult);

            factory.Received(1).Invoke();
            dbConnection.Received(1).Open();
        }

        [Fact]
        public async void OpenDbConnectionAsync_Factory_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            var factory = Substitute.For<Func<DbConnection>>();
            factory.Invoke().Returns(dbConnection);

            var db = new Db(factory);

            var execResult = await db.OpenDbConnectionAsync();
            Assert.Equal(dbConnection, execResult);

            factory.Received(1).Invoke();
            await dbConnection.Received(1).OpenAsync();
        }

        [Fact]
        public void CloseDbConnection_Factory_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(ConnectionState.Open);

            var db = new Db(() => Substitute.For<DbConnection>());

            db.CloseDbConnection(dbConnection);

            dbConnection.Received(1).Close();
        }

        [Fact]
        public void CloseClosedDbConnection_Factory_Test()
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(ConnectionState.Closed);

            var db = new Db(() => Substitute.For<DbConnection>());

            db.CloseDbConnection(dbConnection);

            dbConnection.Received(0).Close();
        }

        [Fact]
        public void CloseNullDbConnection_Factory_Test()
        {
            var db = new Db(() => Substitute.For<DbConnection>());

            db.CloseDbConnection(null);
        }
    }
}
