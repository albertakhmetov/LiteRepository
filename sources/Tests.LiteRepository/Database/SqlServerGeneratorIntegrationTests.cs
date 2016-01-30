﻿/*

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
using LiteRepository.Database;
using LiteRepository.Database.Models;

namespace LiteRepository.Database
{
    public abstract class SqlServerRepositoriesBase
    {
        public DataRepository<SqlEntity, SqlEntityId> EntityRepository
        {
            get; private set;
        }

        public DataRepository<SqlIdentityEntity, SqlIdentityId> IdentityEntityRepository
        {
            get; private set;
        }

        protected void Init()
        {
            var db = new DefaultDb(
                new DefaultSqlExecutor(TaskScheduler.Default),
                GetGenerator,
                () => new SqlConnection(ConfigurationManager.ConnectionStrings["DemoDb"].ConnectionString));
            EntityRepository = new DataRepository<SqlEntity, SqlEntityId>(db);
            IdentityEntityRepository = new DataRepository<SqlIdentityEntity, SqlIdentityId>(
                db,
                (x, y) => new SqlIdentityEntity
                {
                    Id = y,
                    FirstName = x.FirstName,
                    SecondName = x.SecondName,
                    Birthday = x.Birthday
                });
        }

        protected abstract ISqlGenerator GetGenerator(Type entityType);
    }

    public class SpSqlServerRepositories : SqlServerRepositoriesBase
    {
        private readonly StorageProcedureSqlGenerator _entityGenerator, _identityEntityGenerator;

        public SpSqlServerRepositories()
        {
            _entityGenerator = new StorageProcedureSqlGenerator(
                "sci_insert",
                "sci_update",
                "sci_delete",
                "sci_delete_all",
                "sci_select",
                "sci_select_all",
                "sci_count",
                false);

            _identityEntityGenerator = new StorageProcedureSqlGenerator(
                "people_insert",
                "people_update",
                "people_delete",
                "people_delete_all",
                "people_select",
                "people_select_all",
                "people_count",
                false);

            Init();
        }

        protected override ISqlGenerator GetGenerator(Type entityType)
        {
            if (entityType == typeof(SqlEntity))
                return _entityGenerator;
            else if (entityType == typeof(SqlIdentityEntity))
                return _identityEntityGenerator;
            else
                throw new NotSupportedException();
        }
    }

    [Trait("Integration", "Integration")]
    public class SpSql : SqlServerGeneratorIntegrationBase, IClassFixture<SpSqlServerRepositories>
    {
        public SpSql(SpSqlServerRepositories fixture) : base(fixture)
        { }
    }

    public class PlainSqlServerRepositories : SqlServerRepositoriesBase
    {
        public PlainSqlServerRepositories()
        {
            Init();
        }

        protected override ISqlGenerator GetGenerator(Type entityType)
        {
            return new SqlServerGenerator(entityType);
        }
    }

    [Trait("Integration", "Integration")]
    public class PlainSql : SqlServerGeneratorIntegrationBase, IClassFixture<PlainSqlServerRepositories>
    {
        public PlainSql(PlainSqlServerRepositories fixture) : base(fixture)
        {

        }
    }

    public abstract class SqlServerGeneratorIntegrationBase
    {
        private readonly SqlServerRepositoriesBase _fixture;

        public SqlServerGeneratorIntegrationBase(SqlServerRepositoriesBase fixture)
        {
            _fixture = fixture;
         //   _fixture.EntityRepository.DeleteAllAsync().Wait();
            _fixture.IdentityEntityRepository.DeleteAllAsync().Wait();
        }

        #region Entity

        [Fact]
        public async void Entity_Insert_Test()
        {
            var entity = new SqlEntity { Id = 1, ShopId = 42, Price = 10.24m, Text = "sample entity" };

            var insertedEntity = await _fixture.EntityRepository.InsertAsync(entity);
            Assert.Equal(entity, insertedEntity);

            var actualEntity = await _fixture.EntityRepository.GetAsync(new SqlEntityId { Id = 1, ShopId = 42 });
            Assert.Equal(entity, actualEntity);
        }

        [Fact]
        public async void Entity_Update_Test()
        {
            var entity = new SqlEntity { Id = 1, ShopId = 42, Price = 10.24m, Text = "sample entity" };
            await _fixture.EntityRepository.InsertAsync(entity);

            var updatedEntity = await _fixture.EntityRepository.UpdateAsync(
                new SqlEntity { Id = 1, ShopId = 42, Price = 14, Text = "sample entity" });

            var actualEntity = await _fixture.EntityRepository.GetAsync(new SqlEntityId { Id = 1, ShopId = 42 });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void Entity_UpdateOrInsert_Update_Test()
        {
            var entity = new SqlEntity { Id = 1, ShopId = 42, Price = 10.24m, Text = "sample entity" };
            await _fixture.EntityRepository.InsertAsync(entity);

            var updatedEntity = await _fixture.EntityRepository.UpdateOrInsertAsync(
                new SqlEntity { Id = 1, ShopId = 42, Price = 14, Text = "sample entity" });

            var actualEntity = await _fixture.EntityRepository.GetAsync(new SqlEntityId { Id = 1, ShopId = 42 });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void Entity_UpdateOrInsert_Insert_Test()
        {
            var entity = new SqlEntity { Id = 1, ShopId = 42, Price = 10.24m, Text = "sample entity" };

            var updatedEntity = await _fixture.EntityRepository.UpdateOrInsertAsync(
                new SqlEntity { Id = 1, ShopId = 42, Price = 14, Text = "sample entity" });

            var actualEntity = await _fixture.EntityRepository.GetAsync(new SqlEntityId { Id = 1, ShopId = 42 });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void Entity_Delete_Test()
        {
            var count = 10;

            var entity = new SqlEntity { Id = 1, ShopId = 42, Price = 10.24m, Text = "sample entity" };
            for (var i = 0; i < count; i++)
            {
                entity.Id = i;
                await _fixture.EntityRepository.InsertAsync(entity);
            }

            var deletedKey = await _fixture.EntityRepository.DeleteAsync(new SqlEntityId { Id = 1, ShopId = 42 });
            Assert.NotNull(deletedKey);

            var deletedEntity = await _fixture.EntityRepository.GetAsync(new SqlEntityId { Id = 1, ShopId = 42 });
            Assert.Null(deletedEntity);

            var execResult = await _fixture.EntityRepository.GetCountAsync();
            Assert.Equal(count - 1, execResult);
        }

        #endregion

        #region Identity Entity

        [Fact]
        public async void IdentityEntity_Insert_Test()
        {
            var entity = new SqlIdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanov", Birthday = new DateTime(1991, 12, 26) };

            var insertedEntity = await _fixture.IdentityEntityRepository.InsertAsync(entity);
            Assert.NotEqual(entity.Id, insertedEntity.Id);
            Assert.Equal(entity.FirstName, insertedEntity.FirstName);
            Assert.Equal(entity.SecondName, insertedEntity.SecondName);
            Assert.Equal(entity.Birthday, insertedEntity.Birthday);


            var actualEntity = await _fixture.IdentityEntityRepository.GetAsync(new SqlIdentityId { Id = insertedEntity.Id });
            Assert.Equal(insertedEntity, actualEntity);
        }

        [Fact]
        public async void IdentityEntity_Update_Test()
        {
            var entity = new SqlIdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanov", Birthday = new DateTime(1991, 12, 26) };
            var insertedEntity = await _fixture.IdentityEntityRepository.InsertAsync(entity);

            var updatedEntity = await _fixture.IdentityEntityRepository.UpdateAsync(
                new SqlIdentityEntity { Id = insertedEntity.Id, FirstName = "Ivan", SecondName = "Ivanoff", Birthday = new DateTime(1991, 12, 26) });

            Assert.NotEqual(entity.Id, updatedEntity.Id);
            Assert.Equal(entity.FirstName, updatedEntity.FirstName);
            Assert.NotEqual(entity.SecondName, updatedEntity.SecondName);
            Assert.Equal(entity.Birthday, updatedEntity.Birthday);

            var actualEntity = await _fixture.IdentityEntityRepository.GetAsync(new SqlIdentityId { Id = updatedEntity.Id });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void IdentityEntity_UpdateOrInsert_Update_Test()
        {
            var entity = new SqlIdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanov", Birthday = new DateTime(1991, 12, 26) };
            var insertedEntity = await _fixture.IdentityEntityRepository.InsertAsync(entity);

            var updatedEntity = await _fixture.IdentityEntityRepository.UpdateOrInsertAsync(
                            new SqlIdentityEntity { Id = insertedEntity.Id, FirstName = "Ivan", SecondName = "Ivanoff", Birthday = new DateTime(1991, 12, 26) });

            Assert.NotEqual(entity.Id, updatedEntity.Id);
            Assert.Equal(entity.FirstName, updatedEntity.FirstName);
            Assert.NotEqual(entity.SecondName, updatedEntity.SecondName);
            Assert.Equal(entity.Birthday, updatedEntity.Birthday);

            var actualEntity = await _fixture.IdentityEntityRepository.GetAsync(new SqlIdentityId { Id = updatedEntity.Id });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void IdentityEntity_UpdateOrInsert_Insert_Test()
        {
            var entity = new SqlIdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanov", Birthday = new DateTime(1991, 12, 26) };

            var updatedEntity = await _fixture.IdentityEntityRepository.UpdateOrInsertAsync(
                            new SqlIdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanoff", Birthday = new DateTime(1991, 12, 26) });

            Assert.NotEqual(entity.Id, updatedEntity.Id);
            Assert.Equal(entity.FirstName, updatedEntity.FirstName);
            Assert.NotEqual(entity.SecondName, updatedEntity.SecondName);
            Assert.Equal(entity.Birthday, updatedEntity.Birthday);

            var actualEntity = await _fixture.IdentityEntityRepository.GetAsync(new SqlIdentityId { Id = updatedEntity.Id });
            Assert.Equal(updatedEntity, actualEntity);
        }

        [Fact]
        public async void IdentityEntity_Delete_Test()
        {
            var count = 10;

            var entity = new SqlIdentityEntity { Id = 0, FirstName = "Ivan", SecondName = "Ivanov", Birthday = new DateTime(1991, 12, 26) };
            var insertedEntities = new SqlIdentityEntity[count];
            for (var i = 0; i < count; i++)
            {
                insertedEntities[i] = await _fixture.IdentityEntityRepository.InsertAsync(entity);
            }
            Assert.Equal(count, insertedEntities.Last().Id - insertedEntities.First().Id + 1); // example 1 2 3 4 5 6 => 6 - 1 = 5, but count = 6

            var deletedKey = await _fixture.IdentityEntityRepository.DeleteAsync(new SqlIdentityId { Id = insertedEntities[3].Id });
            Assert.NotNull(deletedKey);

            var deletedEntity = await _fixture.IdentityEntityRepository.GetAsync(new SqlIdentityId { Id = insertedEntities[3].Id });
            Assert.Null(deletedEntity);

            var execResult = await _fixture.IdentityEntityRepository.GetCountAsync();
            Assert.Equal(count - 1, execResult);
        }

        #endregion
    }
}
