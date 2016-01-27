/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using Xunit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace LiteRepository.Database
{
    public class DefaultDbTests
    {
        [Fact]
        public void SqlExecutor_Connection_Test()
        {
            var sqlExecutor = Substitute.For<ISqlExecutor>();

            var db = new DefaultDb(sqlExecutor, x => Substitute.For<ISqlGenerator>(), Substitute.For<IDbConnection>());
            Assert.Equal(sqlExecutor, db.GetSqlExecutor());
        }

        [Fact]
        public void SqlExecutor_ConnectionFactory_Test()
        {
            var sqlExecutor = Substitute.For<ISqlExecutor>();

            var db = new DefaultDb(sqlExecutor, x => Substitute.For<ISqlGenerator>(), () => Substitute.For<IDbConnection>());
            Assert.Equal(sqlExecutor, db.GetSqlExecutor());
        }

        [Fact]
        public void SqlGenerator_Connection_Test()
        {
            var g = new Dictionary<Type, ISqlGenerator>();
            g.Add(typeof(Models.Entity), Substitute.For<ISqlGenerator>());
            g.Add(typeof(Models.IdKey), Substitute.For<ISqlGenerator>());

            Assert.NotEqual(g[typeof(Models.Entity)], g[typeof(Models.IdKey)]); // insure that there different mocks

            var db = new DefaultDb(Substitute.For<ISqlExecutor>(), x => g[x], Substitute.For<IDbConnection>());
            Assert.Equal(g[typeof(Models.Entity)], db.GetSqlGenerator<Models.Entity>());
            Assert.Equal(g[typeof(Models.IdKey)], db.GetSqlGenerator<Models.IdKey>());
            Assert.Equal(g[typeof(Models.Entity)], db.GetSqlGenerator<Models.Entity>());
            Assert.Equal(g[typeof(Models.IdKey)], db.GetSqlGenerator<Models.IdKey>());
        }

        [Fact]
        public void SqlGenerator_ConnectionFactory_Test()
        {
            var g = new Dictionary<Type, ISqlGenerator>();
            g.Add(typeof(Models.Entity), Substitute.For<ISqlGenerator>());
            g.Add(typeof(Models.IdKey), Substitute.For<ISqlGenerator>());

            Assert.NotEqual(g[typeof(Models.Entity)], g[typeof(Models.IdKey)]); // insure that there different mocks

            var db = new DefaultDb(Substitute.For<ISqlExecutor>(), x => g[x], () => Substitute.For<IDbConnection>());
            Assert.Equal(g[typeof(Models.Entity)], db.GetSqlGenerator<Models.Entity>());
            Assert.Equal(g[typeof(Models.IdKey)], db.GetSqlGenerator<Models.IdKey>());
            Assert.Equal(g[typeof(Models.Entity)], db.GetSqlGenerator<Models.Entity>());
            Assert.Equal(g[typeof(Models.IdKey)], db.GetSqlGenerator<Models.IdKey>());
        }

        [Fact]
        public void OpenConnection_Test()
        {
            var dbConnection = Substitute.For<IDbConnection>();
            dbConnection.State.Returns(ConnectionState.Closed);

            var db = new DefaultDb(Substitute.For<ISqlExecutor>(), x => Substitute.For<ISqlGenerator>(), dbConnection);

            var execResult = db.OpenConnection();
            Assert.Equal(dbConnection, execResult);

            dbConnection.Received(0).Open();
        }

        [Fact]
        public void CloseConnection_Test()
        {
            var dbConnection = Substitute.For<IDbConnection>();
            dbConnection.State.Returns(ConnectionState.Open);

            var db = new DefaultDb(Substitute.For<ISqlExecutor>(), x => Substitute.For<ISqlGenerator>(), dbConnection);

            db.CloseConnection(dbConnection);
            dbConnection.Received(0).Close();
        }

        [Fact]
        public void OpenConnection_Factory_Test()
        {
            var dbConnection = Substitute.For<IDbConnection>();
            var factory = Substitute.For<Func<IDbConnection>>();
            factory.Invoke().Returns(dbConnection);

            var db = new DefaultDb(Substitute.For<ISqlExecutor>(), x => Substitute.For<ISqlGenerator>(), factory);

            var execResult = db.OpenConnection();
            Assert.Equal(dbConnection, execResult);

            factory.Received(1).Invoke();
            dbConnection.Received(1).Open();
        }

        [Fact]
        public void CloseConnection_Factory_Test()
        {
            var dbConnection = Substitute.For<IDbConnection>();
            dbConnection.State.Returns(ConnectionState.Open);

            var db = new DefaultDb(Substitute.For<ISqlExecutor>(), x => Substitute.For<ISqlGenerator>(), ()=>Substitute.For<IDbConnection>());

            db.CloseConnection(dbConnection);

            dbConnection.Received(1).Close();
        }

        [Fact]
        public void CloseClosedConnection_Factory_Test()
        {
            var dbConnection = Substitute.For<IDbConnection>();
            dbConnection.State.Returns(ConnectionState.Closed);

            var db = new DefaultDb(Substitute.For<ISqlExecutor>(), x => Substitute.For<ISqlGenerator>(), () => Substitute.For<IDbConnection>());

            db.CloseConnection(dbConnection);

            dbConnection.Received(0).Close();
        }

        [Fact]
        public void CloseNullConnection_Factory_Test()
        {
            var db = new DefaultDb(Substitute.For<ISqlExecutor>(), x => Substitute.For<ISqlGenerator>(), () => Substitute.For<IDbConnection>());

            db.CloseConnection(null);
        }
    }
}
