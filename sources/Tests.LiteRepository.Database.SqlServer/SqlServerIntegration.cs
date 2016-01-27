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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Configuration;

namespace LiteRepository.Database.SqlServer
{
    public sealed class SqlServerRepositories
    {
        public DataRepository<Models.Entity, Models.EntityId> EntityRepository
        {
            get; private set;
        }

        public DataRepository<Models.IdentityEntity, Models.IdentityId> IdentityEntityRepository
        {
            get; private set;
        }

        public SqlServerRepositories()
        {
            var db = new DefaultDb(
                new DefaultSqlExecutor(TaskScheduler.Default),
                t => new SqlServerGenerator(t),
                () => new SqlConnection(ConfigurationManager.ConnectionStrings["DemoDb"].ConnectionString));
            EntityRepository = new DataRepository<Models.Entity, Models.EntityId>(db);
            IdentityEntityRepository = new DataRepository<Models.IdentityEntity, Models.IdentityId>(
                db,
                (x, y) => new Models.IdentityEntity
                {
                    Id = y,
                    FirstName = x.FirstName,
                    SecondName = x.SecondName,
                    Birthday = x.Birthday
                });
        }
    }


    [Trait("Integration", "Integration")]
    public class SqlServerIntegration : IClassFixture<SqlServerRepositories>
    {
        private readonly SqlServerRepositories _fixture;

        public SqlServerIntegration(SqlServerRepositories fixture)
        {
            _fixture = fixture;
            _fixture.EntityRepository.DeleteAllAsync().Wait();
            _fixture.IdentityEntityRepository.DeleteAllAsync().Wait();
        }

        #region Entity

        [Fact]
        public async void Entity_Insert_Test()
        {
            var entity = new Models.Entity { Id = 1, ShopId = 42, Price = 10.24m, Text = "sample entity" };

            var insertedEntity = await _fixture.EntityRepository.InsertAsync(entity);
            Assert.Equal(entity, insertedEntity);

            var actualEntity = await _fixture.EntityRepository.GetAsync(new Models.EntityId { Id = 1, ShopId = 42 });
            Assert.Equal(entity, actualEntity);
        }

        [Fact]
        public async void Entity_Update_Test()
        {
            var entity = new Models.Entity { Id = 1, ShopId = 42, Price = 10.24m, Text = "sample entity" };
            await _fixture.EntityRepository.InsertAsync(entity);

            var updatedEntity = await _fixture.EntityRepository.UpdateAsync(
                new Models.Entity { Id = 1, ShopId = 42, Price = 14, Text = "sample entity" });

            var actualEntity = await _fixture.EntityRepository.GetAsync(new Models.EntityId { Id = 1, ShopId = 42 });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void Entity_UpdateOrInsert_Update_Test()
        {
            var entity = new Models.Entity { Id = 1, ShopId = 42, Price = 10.24m, Text = "sample entity" };
            await _fixture.EntityRepository.InsertAsync(entity);

            var updatedEntity = await _fixture.EntityRepository.UpdateOrInsertAsync(
                new Models.Entity { Id = 1, ShopId = 42, Price = 14, Text = "sample entity" });

            var actualEntity = await _fixture.EntityRepository.GetAsync(new Models.EntityId { Id = 1, ShopId = 42 });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void Entity_UpdateOrInsert_Insert_Test()
        {
            var entity = new Models.Entity { Id = 1, ShopId = 42, Price = 10.24m, Text = "sample entity" };

            var updatedEntity = await _fixture.EntityRepository.UpdateOrInsertAsync(
                new Models.Entity { Id = 1, ShopId = 42, Price = 14, Text = "sample entity" });

            var actualEntity = await _fixture.EntityRepository.GetAsync(new Models.EntityId { Id = 1, ShopId = 42 });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void Entity_Delete_Test()
        {
            var count = 10;

            var entity = new Models.Entity { Id = 1, ShopId = 42, Price = 10.24m, Text = "sample entity" };
            for (var i = 0; i < count; i++)
            {
                entity.Id = i;
                await _fixture.EntityRepository.InsertAsync(entity);
            }

            var deletedKey = await _fixture.EntityRepository.DeleteAsync(new Models.EntityId { Id = 1, ShopId = 42 });
            Assert.NotNull(deletedKey);

            var deletedEntity = await _fixture.EntityRepository.GetAsync(new Models.EntityId { Id = 1, ShopId = 42 });
            Assert.Null(deletedEntity);

            var execResult = await _fixture.EntityRepository.GetCountAsync();
            Assert.Equal(count - 1, execResult);
        }

        #endregion

        #region Identity Entity

        [Fact]
        public async void IdentityEntity_Insert_Test()
        {
            var entity = new Models.IdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanov", Birthday = new DateTime(1991, 12, 26) };

            var insertedEntity = await _fixture.IdentityEntityRepository.InsertAsync(entity);
            Assert.NotEqual(entity.Id, insertedEntity.Id);
            Assert.Equal(entity.FirstName, insertedEntity.FirstName);
            Assert.Equal(entity.SecondName, insertedEntity.SecondName);
            Assert.Equal(entity.Birthday, insertedEntity.Birthday);


            var actualEntity = await _fixture.IdentityEntityRepository.GetAsync(new Models.IdentityId { Id = insertedEntity.Id });
            Assert.Equal(insertedEntity, actualEntity);
        }

        [Fact]
        public async void IdentityEntity_Update_Test()
        {
            var entity = new Models.IdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanov", Birthday = new DateTime(1991, 12, 26) };
            var insertedEntity = await _fixture.IdentityEntityRepository.InsertAsync(entity);

            var updatedEntity = await _fixture.IdentityEntityRepository.UpdateAsync(
                new Models.IdentityEntity { Id = insertedEntity.Id, FirstName = "Ivan", SecondName = "Ivanoff", Birthday = new DateTime(1991, 12, 26) });

            Assert.NotEqual(entity.Id, updatedEntity.Id);
            Assert.Equal(entity.FirstName, updatedEntity.FirstName);
            Assert.NotEqual(entity.SecondName, updatedEntity.SecondName);
            Assert.Equal(entity.Birthday, updatedEntity.Birthday);

            var actualEntity = await _fixture.IdentityEntityRepository.GetAsync(new Models.IdentityId { Id = updatedEntity.Id });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void IdentityEntity_UpdateOrInsert_Update_Test()
        {
            var entity = new Models.IdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanov", Birthday = new DateTime(1991, 12, 26) };
            var insertedEntity = await _fixture.IdentityEntityRepository.InsertAsync(entity);

            var updatedEntity = await _fixture.IdentityEntityRepository.UpdateOrInsertAsync(
                            new Models.IdentityEntity { Id = insertedEntity.Id, FirstName = "Ivan", SecondName = "Ivanoff", Birthday = new DateTime(1991, 12, 26) });

            Assert.NotEqual(entity.Id, updatedEntity.Id);
            Assert.Equal(entity.FirstName, updatedEntity.FirstName);
            Assert.NotEqual(entity.SecondName, updatedEntity.SecondName);
            Assert.Equal(entity.Birthday, updatedEntity.Birthday);

            var actualEntity = await _fixture.IdentityEntityRepository.GetAsync(new Models.IdentityId { Id = updatedEntity.Id });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void IdentityEntity_UpdateOrInsert_Insert_Test()
        {
            var entity = new Models.IdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanov", Birthday = new DateTime(1991, 12, 26) };

            var updatedEntity = await _fixture.IdentityEntityRepository.UpdateOrInsertAsync(
                            new Models.IdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanoff", Birthday = new DateTime(1991, 12, 26) });

            Assert.NotEqual(entity.Id, updatedEntity.Id);
            Assert.Equal(entity.FirstName, updatedEntity.FirstName);
            Assert.NotEqual(entity.SecondName, updatedEntity.SecondName);
            Assert.Equal(entity.Birthday, updatedEntity.Birthday);

            var actualEntity = await _fixture.IdentityEntityRepository.GetAsync(new Models.IdentityId { Id = updatedEntity.Id });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void IdentityEntity_Delete_Test()
        {
            var count = 10;

            var entity = new Models.IdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanov", Birthday = new DateTime(1991, 12, 26) };
            var insertedEntities = new Models.IdentityEntity[count];
            for (var i = 0; i < count; i++)
            {
                insertedEntities[i] = await _fixture.IdentityEntityRepository.InsertAsync(entity);
            }
            Assert.Equal(count, insertedEntities.Last().Id - insertedEntities.First().Id + 1); // example 1 2 3 4 5 6 => 6 - 1 = 5, but count = 6

            var deletedKey = await _fixture.IdentityEntityRepository.DeleteAsync(new Models.IdentityId { Id = insertedEntities[3].Id });
            Assert.NotNull(deletedKey);

            var deletedEntity = await _fixture.IdentityEntityRepository.GetAsync(new Models.IdentityId { Id = insertedEntities[3].Id });
            Assert.Null(deletedEntity);

            var execResult = await _fixture.IdentityEntityRepository.GetCountAsync();
            Assert.Equal(count - 1, execResult);
        }

        #endregion
    }
}
