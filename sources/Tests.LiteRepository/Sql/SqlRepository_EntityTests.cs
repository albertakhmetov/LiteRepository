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

using LiteRepository.Common;
using LiteRepository.Sql.Models;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using Dapper;

namespace LiteRepository.Sql
{
    public class SqlRepository_EntityFixture<E, K> : IDisposable
        where E : class
        where K : class
    {
        public SqlConnection SqlConnection
        {
            get; private set;
        }

        public IDb Db
        {
            get; private set;
        }

        public SqlBuilder<E> SqlBuilder
        {
            get; private set;
        }

        public SqlRepository<E, K> SqlRepository
        {
            get; private set;
        }

        public SqlRepository_EntityFixture()
        {
            SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DemoDb"].ConnectionString);
            SqlConnection.Open();
            Db = new Db(SqlConnection);

            SqlBuilder = new SqlBuilder<E>();
            SqlRepository = new SqlRepository<E, K>(Db, SqlBuilder);
        }

        public void Dispose()
        {
            SqlConnection.Dispose();
        }
    }

    public class SqlRepository_EntityTests : IClassFixture<SqlRepository_EntityFixture<Entity, Entity.Key>>
    {
        private readonly SqlRepository_EntityFixture<Entity, Entity.Key> _fixture;

        public SqlRepository_EntityTests(SqlRepository_EntityFixture<Entity, Entity.Key> fixture)
        {
            _fixture = fixture;
            _fixture.SqlConnection.Execute("truncate table students");
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

        [Fact]
        public async void Insert_Test()
        {
            var entity = GetEntity();

            var insertedEntity = await _fixture.SqlRepository.InsertAsync(entity);
            Assert.Equal(entity, insertedEntity);

            var dbEntity = await _fixture.SqlRepository.GetAsync(new Entity.Key { Cource = insertedEntity.Cource, Letter = insertedEntity.Letter });
            Assert.NotSame(entity, dbEntity);
            Assert.Equal(entity, dbEntity);
        }

        [Fact]
        public async void Update_Test()
        {
            var entity = GetEntity();
            var updatedEntity = GetEntity();
            updatedEntity.SecondName = "Petroff";

            var insertedEntity = await _fixture.SqlRepository.InsertAsync(entity);
            var execResult = await _fixture.SqlRepository.UpdateAsync(updatedEntity);
            Assert.Equal(1, execResult);

            var dbEntity = await _fixture.SqlRepository.GetAsync(new Entity.Key { Cource = insertedEntity.Cource, Letter = insertedEntity.Letter });
            Assert.NotSame(updatedEntity, dbEntity);
            Assert.Equal(updatedEntity, dbEntity);
        }

        [Fact]
        public async void Delete_Test()
        {
            var count = 10;
            var entity = GetEntity();

            for (var i = 0; i < count; i++)
            {

                entity.Cource = i;
                await _fixture.SqlRepository.InsertAsync(entity);
            }

            var keyToDelete = new Entity.Key { Cource = 3, Letter = entity.Letter };

            var execResult = await _fixture.SqlRepository.DeleteAsync(keyToDelete);
            Assert.Equal(1, execResult);

            var dbEntity = await _fixture.SqlRepository.GetAsync(keyToDelete);
            var dbCount = await _fixture.SqlRepository.GetCountAsync();
            Assert.Null(dbEntity);
            Assert.Equal(count - 1, dbCount);
        }
    }

    public class SqlRepository_IdentityEntityTests : IClassFixture<SqlRepository_EntityFixture<IdentityEntity, IdentityKey>>
    {
        private readonly SqlRepository_EntityFixture<IdentityEntity, IdentityKey> _fixture;

        public SqlRepository_IdentityEntityTests(SqlRepository_EntityFixture<IdentityEntity, IdentityKey> fixture)
        {
            _fixture = fixture;
            _fixture.SqlConnection.Execute("truncate table users");
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

        [Fact]
        public async void Insert_Test()
        {
            var entity = GetIdentityEntity();

            var insertedEntity = await _fixture.SqlRepository.InsertAsync(entity);
            Assert.Equal(entity, insertedEntity);

            var dbEntity = await _fixture.SqlRepository.GetAsync(new IdentityKey(insertedEntity.Id));
            Assert.NotSame(entity, dbEntity);
            Assert.Equal(entity, dbEntity);
        }

        [Fact]
        public async void Insert_Many_Test()
        {
            var entity = GetIdentityEntity();
            var count = 100;
            var insertedId = new long[count];

            for (var i = 0; i < count; i++)
            {
                var insertedEntity = await _fixture.SqlRepository.InsertAsync(entity);
                insertedId[i] = insertedEntity.Id;
            }
            Assert.Equal(count, insertedId.Last() - insertedId.First() + 1);

            for (var i = 0; i < count; i++)
            {
                var dbEntity = await _fixture.SqlRepository.GetAsync(new IdentityKey(insertedId[i]));
                Assert.NotNull(dbEntity);
            }
        }
    }
}
