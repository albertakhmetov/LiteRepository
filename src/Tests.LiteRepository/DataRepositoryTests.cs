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
using LiteRepository.Database;
using System.Data;
using System.Threading;

namespace LiteRepository
{
    public class DataRepositoryTests
    {
        [Fact]
        public void Ctor_Test()
        {
            var db = Substitute.For<IDb>();
            var r = new DataRepository<Entity, IdKey>(db);

            Assert.Equal(db, r.Db);
        }

        [Fact]
        public void Ctor_DbNull_Test()
        {
            Assert.Throws<ArgumentNullException>(() => new DataRepository<Entity, IdKey>(null));
        }
       
        private DataRepository<Entity, IdKey> GetRepository(
            ISqlGenerator sqlGenerator = null,
            ISqlExecutor sqlExecutor = null,
            Func<Entity, long, Entity> entityFactory = null,
            IDbConnection dbConnection = null)
        {
            var db = Substitute.For<IDb>();
            db.GetSqlGenerator<Entity>().Returns(sqlGenerator ?? Substitute.For<ISqlGenerator>());
            db.GetSqlExecutor().Returns(sqlExecutor ?? Substitute.For<ISqlExecutor>());
            db.OpenConnection().Returns(dbConnection ?? Substitute.For<IDbConnection>());

            return new DataRepository<Entity, IdKey>(db, entityFactory);
        }

        private void SetExecuteAsyncResult<T>(ISqlExecutor sqlExecutor, int execResult)
        {
            sqlExecutor.ExecuteAsync<T>(Arg.Any<IDbConnection>(), Arg.Any<string>(), Arg.Any<T>()).Returns(execResult);
        }

        #region Connection and error management (in the case of insert operation)

        [Fact]
        public async void OpenConnection_Test()
        {
            var r = GetRepository(entityFactory: (x, y) => x);
            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 1);
            await r.InsertAsync(new Entity());

            r.Db.Received(1).OpenConnection();
        }

        [Fact]
        public async void CloseConnection_Test()
        {
            var r = GetRepository(entityFactory: (x, y) => x);
            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 1);
            await r.InsertAsync(new Entity());

            r.Db.Received(1).CloseConnection(Arg.Any<IDbConnection>());
        }

        [Fact]
        public async void CloseConnection_WhenError_Test()
        {
            var r = GetRepository(entityFactory: (x, y) => x);
            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 0);

            try
            {
                await r.InsertAsync(new Entity());
            }
            catch
            { }

            r.Db.Received(1).CloseConnection(Arg.Any<IDbConnection>());
        }

        [Fact]
        public async void InnerException_Test()
        {
            var r = GetRepository();
            r.Db.GetSqlExecutor()
                .When(x => x.ExecuteAsync<Entity>(Arg.Any<IDbConnection>(), Arg.Any<string>(), Arg.Any<Entity>()))
                .Do(x => { throw new DataException(); });
            await Assert.ThrowsAsync<DataRepositoryException>(() => r.InsertAsync(new Entity()));
        }

        #endregion

        #region Dispose

        [Fact]
        public async void Insert_Disposed_Test()
        {
            var r = GetRepository();
            r.Dispose();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.InsertAsync(new Entity()));        
        }

        [Fact]
        public async void Update_Disposed_Test()
        {
            var r = GetRepository();
            r.Dispose();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.UpdateAsync(new Entity()));
        }

        [Fact]
        public async void UpdateOrInsert_Disposed_Test()
        {
            var r = GetRepository();
            r.Dispose();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.UpdateOrInsertAsync(new Entity()));
        }

        [Fact]
        public async void Delete_Disposed_Test()
        {
            var r = GetRepository();
            r.Dispose();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.DeleteAsync(new IdKey()));
        }

        [Fact]
        public async void DeleteAll_Disposed_Test()
        {
            var r = GetRepository();
            r.Dispose();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.DeleteAllAsync());
        }

        [Fact]
        public async void Get_Disposed_Test()
        {
            var r = GetRepository();
            r.Dispose();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.GetAsync(new IdKey()));
        }

        [Fact]
        public async void GetAll_Disposed_Test()
        {
            var r = GetRepository();
            r.Dispose();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.GetAllAsync());
        }

        [Fact]
        public async void GetCount_Disposed_Test()
        {
            var r = GetRepository();
            r.Dispose();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.GetCountAsync());
        }

        #endregion

        #region Notifications

        private class Observers
        {
            public IObserver<Entity> Insert { get; set; }
            public IObserver<Entity> Update { get; set; }
            public IObserver<IdKey> Delete { get; set; }
        }

        private Observers SubscribeToRepository(IDataRepository<Entity, IdKey> repository)
        {
            var insertObserver = Substitute.For<IObserver<Entity>>();
            var updateObserver = Substitute.For<IObserver<Entity>>();
            var deleteObserver = Substitute.For<IObserver<IdKey>>();

            repository.InsertedObservable.Subscribe(insertObserver);
            repository.UpdatedObservable.Subscribe(updateObserver);
            repository.DeletedObservable.Subscribe(deleteObserver);

            return new Observers { Insert = insertObserver, Update = updateObserver, Delete = deleteObserver };
        }

        [Fact]
        public void CompleteNotification_Dispose_Test()
        {
            var r = GetRepository();
            var observers = SubscribeToRepository(r);

            r.Dispose();

            observers.Insert.Received(1).OnCompleted();
            observers.Update.Received(1).OnCompleted();            
            observers.Delete.Received(1).OnCompleted();
        }

        [Fact]
        public async void Insert_Notification_Test()
        {
            var entity = new Entity();

            var r = GetRepository();
            var observers = SubscribeToRepository(r);

            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 1);

            var execResult = await r.InsertAsync(entity);

            observers.Insert.Received(1).OnNext(entity);
            observers.Update.Received(0).OnNext(Arg.Any<Entity>());
            observers.Delete.Received(0).OnNext(Arg.Any<IdKey>());
        }

        [Fact]
        public async void Insert_Factory_Notification_Test()
        {
            // case when entity doesn't exist (try update and then insert)

            var entity = new Entity() { Text = "Hi!", Id = 42 };

            var r = GetRepository(entityFactory: (x, y) => new Entity { Text = x.Text, Id = y });
            var observers = SubscribeToRepository(r);

            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 1);

            var execResult = await r.InsertAsync(entity);

            observers.Insert.Received(1).OnNext(execResult);
            observers.Update.Received(0).OnNext(Arg.Any<Entity>());
            observers.Delete.Received(0).OnNext(Arg.Any<IdKey>());
        }

        [Fact]
        public async void Insert_NoData_NoNotification_Test()
        {
            var entity = new Entity();

            var r = GetRepository();
            var observers = SubscribeToRepository(r);

            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 0);

            var execResult = await r.InsertAsync(entity);

            observers.Insert.Received(0).OnNext(Arg.Any<Entity>());
            observers.Update.Received(0).OnNext(Arg.Any<Entity>());
            observers.Delete.Received(0).OnNext(Arg.Any<IdKey>());
        }

        [Fact]
        public async void Update_Notification_Test()
        {
            var entity = new Entity();

            var r = GetRepository();
            var observers = SubscribeToRepository(r);

            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 1);

            var execResult = await r.UpdateAsync(entity);

            observers.Insert.Received(0).OnNext(Arg.Any<Entity>());
            observers.Update.Received(1).OnNext(entity);
            observers.Delete.Received(0).OnNext(Arg.Any<IdKey>());
        }

        [Fact]
        public async void Update_NoData_NoNotification_Test()
        {
            var entity = new Entity();

            var r = GetRepository();
            var observers = SubscribeToRepository(r);

            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 0);

            var execResult = await r.UpdateAsync(entity);

            observers.Insert.Received(0).OnNext(Arg.Any<Entity>());
            observers.Update.Received(0).OnNext(Arg.Any<Entity>());
            observers.Delete.Received(0).OnNext(Arg.Any<IdKey>());
        }

        [Fact]
        public async void UpdateOrInsert_Exists_Notification_Test()
        {
            // case when entity already exist (only update)

            var updateSql = "update sql";
            var entity = new Entity();

            var r = GetRepository();
            var observers = SubscribeToRepository(r);
            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 1);

            r.Db.GetSqlGenerator<Entity>().UpdateSql.Returns(updateSql);

            var execResult = await r.UpdateOrInsertAsync(entity);

            observers.Insert.Received(0).OnNext(Arg.Any<Entity>());
            observers.Update.Received(1).OnNext(execResult);
            observers.Delete.Received(0).OnNext(Arg.Any<IdKey>());
        }

        [Fact]
        public async void UpdateOrInsert_NotExists_Notification_Test()
        {
            // case when entity doesn't exist (try update and then insert)

            var updateSql = "update sql";
            var insertSql = "insert sql";
            var entity = new Entity() { Text = "Hi!", Id = 42 };

            var r = GetRepository(entityFactory: (x, y) => new Entity { Text = x.Text, Id = y });
            var observers = SubscribeToRepository(r);

            r.Db.GetSqlExecutor().ExecuteAsync<Entity>(Arg.Any<IDbConnection>(), Arg.Is<string>(x => x == updateSql), entity).Returns(0);
            r.Db.GetSqlExecutor().ExecuteAsync<Entity>(Arg.Any<IDbConnection>(), Arg.Is<string>(x => x == insertSql), entity).Returns(1);

            r.Db.GetSqlGenerator<Entity>().UpdateSql.Returns(updateSql);
            r.Db.GetSqlGenerator<Entity>().InsertSql.Returns(insertSql);

            var execResult = await r.UpdateOrInsertAsync(entity);

            observers.Insert.Received(1).OnNext(execResult);
            observers.Update.Received(0).OnNext(Arg.Any<Entity>());
            observers.Delete.Received(0).OnNext(Arg.Any<IdKey>());
        }

        [Fact]
        public async void UpdateOrInsert_NoData_NoNotification_Test()
        {
            // entity doesn't exist and can't be added

            var updateSql = "update sql";
            var insertSql = "insert sql";
            var entity = new Entity() { Text = "Hi!", Id = 42 };

            var r = GetRepository();
            var observers = SubscribeToRepository(r);

            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 0);

            r.Db.GetSqlGenerator<Entity>().UpdateSql.Returns(updateSql);
            r.Db.GetSqlGenerator<Entity>().InsertSql.Returns(insertSql);

            var execResult = await r.UpdateOrInsertAsync(entity);
            observers.Insert.Received(0).OnNext(Arg.Any<Entity>());
            observers.Update.Received(0).OnNext(Arg.Any<Entity>());
            observers.Delete.Received(0).OnNext(Arg.Any<IdKey>());
        }

        [Fact]
        public async void Delete_Notification_Test()
        {
            var key = new IdKey();

            var r = GetRepository();
            var observers = SubscribeToRepository(r);

            SetExecuteAsyncResult<IdKey>(r.Db.GetSqlExecutor(), 1);

            var execResult = await r.DeleteAsync(key);

            observers.Insert.Received(0).OnNext(Arg.Any<Entity>());
            observers.Update.Received(0).OnNext(Arg.Any<Entity>());
            observers.Delete.Received(1).OnNext(key);
        }

        [Fact]
        public async void Delete_NoData_NoNotification_Test()
        {
            var key = new IdKey();

            var r = GetRepository();
            var observers = SubscribeToRepository(r);

            SetExecuteAsyncResult<IdKey>(r.Db.GetSqlExecutor(), 0);

            var execResult = await r.DeleteAsync(key);

            observers.Insert.Received(0).OnNext(Arg.Any<Entity>());
            observers.Update.Received(0).OnNext(Arg.Any<Entity>());
            observers.Delete.Received(0).OnNext(Arg.Any<IdKey>());
        }

        [Fact]
        public async void DeleteAll_Notification_Test()
        {
            var r = GetRepository();
            var observers = SubscribeToRepository(r);

            r.Db.GetSqlExecutor().ExecuteAsync(Arg.Any<IDbConnection>(), Arg.Any<string>()).Returns(10);

            var execResult = await r.DeleteAllAsync();

            observers.Insert.Received(0).OnNext(Arg.Any<Entity>());
            observers.Update.Received(0).OnNext(Arg.Any<Entity>());
            observers.Delete.Received(1).OnNext(null);
        }

        [Fact]
        public async void DeleteAll_NoData_NoNotification_Test()
        {
            var r = GetRepository();
            var observers = SubscribeToRepository(r);

            r.Db.GetSqlExecutor().ExecuteAsync(Arg.Any<IDbConnection>(), Arg.Any<string>()).Returns(0);

            var execResult = await r.DeleteAllAsync();

            observers.Insert.Received(0).OnNext(Arg.Any<Entity>());
            observers.Update.Received(0).OnNext(Arg.Any<Entity>());
            observers.Delete.Received(0).OnNext(Arg.Any<IdKey>());
        }

        #endregion

        #region Insert

        [Fact]
        public async void Insert_EntityNull_Test()
        {
            var r = GetRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() => r.InsertAsync(null));
        }

        [Fact]
        public async void Insert_Test()
        {
            var sql = "insert sql";
            var entity = new Entity();

            var dbConnection = Substitute.For<IDbConnection>();

            var r = GetRepository(dbConnection: dbConnection);
            r.Db.GetSqlExecutor().ExecuteAsync(Arg.Any<IDbConnection>(), Arg.Any<string>(), entity).Returns(1);
            r.Db.GetSqlGenerator<Entity>().InsertSql.Returns(sql);

            var insertedEntity = await r.InsertAsync(entity);
            Assert.Equal(entity, insertedEntity);

            await r.Db.GetSqlExecutor().Received(1).ExecuteAsync<Entity>(dbConnection, sql, entity);
        }

        [Fact]
        public async void Insert_Factory_Test()
        {
            var newId = 13;
            var entity = new Entity() { Text = "Hi!", Id = 42 };

            var r = GetRepository(entityFactory: (x, y) => new Entity { Text = x.Text, Id = y });
            r.Db.GetSqlExecutor().QueryScalarAsync<Entity, long>(Arg.Any<IDbConnection>(), Arg.Any<string>(), entity).Returns(newId);

            var insertedEntity = await r.InsertAsync(entity);
            Assert.NotEqual(entity, insertedEntity);

            Assert.Equal(entity.Text, insertedEntity.Text);
            Assert.Equal(newId, insertedEntity.Id);
        }

        [Fact]
        public async void Insert_NoData_Test()
        {
            var entity = new Entity() { Text = "Hi!", Id = 42 };

            var r = GetRepository();
            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 0);

            var insertedEntity = await r.InsertAsync(entity);
            Assert.Null(insertedEntity);
        }

        #endregion

        #region Update

        [Fact]
        public async void Update_EntityNull_Test()
        {
            var r = GetRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() => r.UpdateAsync(null));
        }

        [Fact]
        public async void Update_Test()
        {
            var sql = "update sql";
            var entity = new Entity();

            var dbConnection = Substitute.For<IDbConnection>();

            var r = GetRepository(dbConnection: dbConnection);
            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 1);
            r.Db.GetSqlGenerator<Entity>().UpdateSql.Returns(sql);

            var execResult = await r.UpdateAsync(entity);
            Assert.Equal(entity, execResult);

            await r.Db.GetSqlExecutor().Received(1).ExecuteAsync<Entity>(dbConnection, sql, entity);
        }

        [Fact]
        public async void Update_NoData_Test()
        {
            var r = GetRepository();
            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 0);
            var entity = new Entity();

            var execResult = await r.UpdateAsync(entity);
            Assert.Null(execResult);
        }

        #endregion

        #region Update Or Insert (Set)

        [Fact]
        public async void UpdateOrInsert_EntityNull_Test()
        {
            var r = GetRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() => r.UpdateOrInsertAsync(null));
        }

        [Fact]
        public async void UpdateOrInsert_Exists_Test()
        {
            // case when entity already exist (only update)

            var updateSql = "update sql";
            var entity = new Entity();

            var dbConnection = Substitute.For<IDbConnection>();

            var r = GetRepository(dbConnection: dbConnection);
            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 1);

            r.Db.GetSqlGenerator<Entity>().UpdateSql.Returns(updateSql);

            var execResult = await r.UpdateOrInsertAsync(entity);
            Assert.Equal(entity, execResult);

            await r.Db.GetSqlExecutor().Received(1).ExecuteAsync<Entity>(dbConnection, updateSql, entity);
            await r.Db.GetSqlExecutor().Received(1).ExecuteAsync<Entity>(dbConnection, Arg.Any<string>(), entity);
        }

        [Fact]
        public async void UpdateOrInsert_NotExists_Test()
        {
            // case when entity doesn't exist (try update and then insert)

            var updateSql = "update sql";
            var insertSql = "insert sql";
            var newId = 13;
            var entity = new Entity() { Text = "Hi!", Id = 42 };

            var dbConnection = Substitute.For<IDbConnection>();

            var r = GetRepository(dbConnection: dbConnection, entityFactory: (x, y) => new Entity { Text = x.Text, Id = y });
            r.Db.GetSqlExecutor().ExecuteAsync<Entity>(Arg.Any<IDbConnection>(), Arg.Any<string>(), entity).Returns(0);
            r.Db.GetSqlExecutor().QueryScalarAsync<Entity, long>(Arg.Any<IDbConnection>(), Arg.Any<string>(), entity).Returns(newId);

            r.Db.GetSqlGenerator<Entity>().UpdateSql.Returns(updateSql);
            r.Db.GetSqlGenerator<Entity>().InsertSql.Returns(insertSql);

            var execResult = await r.UpdateOrInsertAsync(entity);
            Assert.NotEqual(entity, execResult);

            Assert.Equal(entity.Text, execResult.Text);
            Assert.Equal(newId, execResult.Id);

            await r.Db.GetSqlExecutor().Received(1).ExecuteAsync<Entity>(dbConnection, updateSql, entity);
            await r.Db.GetSqlExecutor().Received(1).QueryScalarAsync<Entity, long>(dbConnection, insertSql, entity);
        }

        [Fact]
        public async void UpdateOrInsert_NoData_Test()
        {
            // entity doesn't exist and can't be added

            var updateSql = "update sql";
            var insertSql = "insert sql";
            var entity = new Entity() { Text = "Hi!", Id = 42 };

            var dbConnection = Substitute.For<IDbConnection>();

            var r = GetRepository(dbConnection: dbConnection);
            SetExecuteAsyncResult<Entity>(r.Db.GetSqlExecutor(), 0);

            r.Db.GetSqlGenerator<Entity>().UpdateSql.Returns(updateSql);
            r.Db.GetSqlGenerator<Entity>().InsertSql.Returns(insertSql);

            var execResult = await r.UpdateOrInsertAsync(entity);
            Assert.Null(execResult);

            await r.Db.GetSqlExecutor().Received(1).ExecuteAsync<Entity>(dbConnection, updateSql, entity);
            await r.Db.GetSqlExecutor().Received(1).ExecuteAsync<Entity>(dbConnection, insertSql, entity);
            await r.Db.GetSqlExecutor().Received(2).ExecuteAsync<Entity>(dbConnection, Arg.Any<string>(), entity);
        }

        #endregion

        #region Delete

        [Fact]
        public async void Delete_KeyNull_Test()
        {
            var r = GetRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() => r.DeleteAsync(null));
        }

        [Fact]
        public async void Delete_Test()
        {
            var sql = "delete sql";
            var key = new IdKey();

            var dbConnection = Substitute.For<IDbConnection>();

            var r = GetRepository(dbConnection: dbConnection);
            SetExecuteAsyncResult<IdKey>(r.Db.GetSqlExecutor(), 1);
            r.Db.GetSqlGenerator<Entity>().DeleteSql.Returns(sql);

            var execResult = await r.DeleteAsync(key);
            Assert.Equal(key, execResult);

            await r.Db.GetSqlExecutor().Received(1).ExecuteAsync<IdKey>(dbConnection, sql, key);
        }

        [Fact]
        public async void Delete_NoData_Test()
        {
            var r = GetRepository();
            SetExecuteAsyncResult<IdKey>(r.Db.GetSqlExecutor(), 0);
            var key = new IdKey();

            var execResult = await r.DeleteAsync(key);
            Assert.Null(execResult);
        }

        #endregion

        #region Delete All

        [Fact]
        public async void DeleteAll_Test()
        {
            var sql = "delete all sql";

            var dbConnection = Substitute.For<IDbConnection>();

            var r = GetRepository(dbConnection: dbConnection);
            r.Db.GetSqlExecutor().ExecuteAsync(Arg.Any<IDbConnection>(), Arg.Any<string>()).Returns(1);
            r.Db.GetSqlGenerator<Entity>().DeleteAllSql.Returns(sql);

            var execResult = await r.DeleteAllAsync();
            Assert.Equal(1, execResult);

            await r.Db.GetSqlExecutor().Received(1).ExecuteAsync(dbConnection, sql);
        }

        [Fact]
        public async void DeleteAll_NoData_Test()
        {
            var r = GetRepository();
            r.Db.GetSqlExecutor().ExecuteAsync(Arg.Any<IDbConnection>(), Arg.Any<string>()).Returns(0);

            var execResult = await r.DeleteAllAsync();
            Assert.Equal(0, execResult);
        }

        #endregion

        #region Get

        [Fact]
        public async void Get_KeyNull_Test()
        {
            var r = GetRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() => r.GetAsync(null));
        }

        [Fact]
        public async void Get_Test()
        {
            var sql = "select sql";
            var key = new IdKey();

            var result = new Entity();

            var dbConnection = Substitute.For<IDbConnection>();

            var r = GetRepository(dbConnection: dbConnection);
            r.Db.GetSqlExecutor().QueryAsync<IdKey, Entity>(Arg.Any<IDbConnection>(), Arg.Any<string>(), Arg.Any<IdKey>()).Returns(new Entity[] { result });
            r.Db.GetSqlGenerator<Entity>().SelectSql.Returns(sql);

            var execResult = await r.GetAsync(key);
            Assert.Equal(result, execResult);

            await r.Db.GetSqlExecutor().Received(1).QueryAsync<IdKey, Entity>(dbConnection, sql, key);
        }

        [Fact]
        public async void Get_NoData_Test()
        {
            var key = new IdKey();

            var r = GetRepository();
            r.Db.GetSqlExecutor().QueryAsync<IdKey, Entity>(Arg.Any<IDbConnection>(), Arg.Any<string>(), Arg.Any<IdKey>()).Returns(new Entity[0]);

            var execResult = await r.GetAsync(key);
            Assert.Null(execResult);
        }

        #endregion

        #region Get All

        [Fact]
        public async void GetAll_Test()
        {
            var sql = "select all sql";
            var result = new Entity[10];

            var dbConnection = Substitute.For<IDbConnection>();

            var r = GetRepository(dbConnection: dbConnection);
            r.Db.GetSqlExecutor().QueryAsync<Entity>(Arg.Any<IDbConnection>(), Arg.Any<string>()).Returns(result);
            r.Db.GetSqlGenerator<Entity>().SelectAllSql.Returns(sql);

            var execResult = await r.GetAllAsync();
            Assert.Equal(result, execResult);

            await r.Db.GetSqlExecutor().Received(1).QueryAsync<Entity>(dbConnection, sql);
        }

        [Fact]
        public async void GetAll_NoData_Test()
        {
            var r = GetRepository();
            r.Db.GetSqlExecutor().QueryAsync<Entity>(Arg.Any<IDbConnection>(), Arg.Any<string>()).Returns(new Entity[0]);

            var execResult = await r.GetAllAsync();
            Assert.Equal(0, execResult.Count());
        }

        #endregion

        #region Get Count

        [Fact]
        public async void GetCount_Test()
        {
            var sql = "get count sql";
            var result = 42L;

            var dbConnection = Substitute.For<IDbConnection>();

            var r = GetRepository(dbConnection: dbConnection);
            r.Db.GetSqlExecutor().QueryScalarAsync<long>(Arg.Any<IDbConnection>(), Arg.Any<string>()).Returns(result);
            r.Db.GetSqlGenerator<Entity>().CountSql.Returns(sql);

            var execResult = await r.GetCountAsync();
            Assert.Equal(result, execResult);

            await r.Db.GetSqlExecutor().Received(1).QueryScalarAsync<long>(dbConnection, sql);
        }

        [Fact]
        public async void GetCount_NoData_Test()
        {
            var r = GetRepository();
            r.Db.GetSqlExecutor().QueryScalarAsync<long>(Arg.Any<IDbConnection>(), Arg.Any<string>()).Returns(0);

            var execResult = await r.GetCountAsync();
            Assert.Equal(0, execResult);
        }

        #endregion
    }
}
