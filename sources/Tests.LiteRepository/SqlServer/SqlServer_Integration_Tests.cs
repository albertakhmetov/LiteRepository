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
using System.Data.SqlClient;
using System.Configuration;
using LiteRepository.Models;

namespace LiteRepository.SqlServer
{
    public sealed class SqlServerFixture
    {
        public Db Db
        {
            get; private set;
        }

        public SqlServerFixture()
        {
            Db = new Db(new SqlServerDialect(), () => new SqlConnection(ConfigurationManager.ConnectionStrings["DemoDb"].ConnectionString));
        }
    }

    public class SqlServer_Integration_Tests : IClassFixture<SqlServerFixture>
    {
        private readonly SqlServerFixture _fixture;

        public SqlServer_Integration_Tests(SqlServerFixture fixture)
        {
            _fixture = fixture;
            _fixture.Db.Truncate<Entity>();
            _fixture.Db.Truncate<IdentityEntity>();
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

        private static IdentityEntity GetIdentityEntity()
        {
            return new IdentityEntity
            {
                Id = -1,
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

        [Fact]
        public void Insert_Test()
        {
            var entity = GetEntity();

            var insertedEntity = _fixture.Db.Insert(entity);
            var dbEntity = _fixture.Db.GetByKey<Entity, EntityKey>(entity as EntityKey);

            Assert.Equal(entity, dbEntity);
        }

        [Fact]
        public void InsertIdentity_Test()
        {
            var entity = GetIdentityEntity();

            var insertedEntity = _fixture.Db.Insert(entity);
            var dbEntity = _fixture.Db.GetByKey<IdentityEntity, IIdentityEntity>(new IdentityKey(insertedEntity.Id));

            Assert.NotEqual(entity, dbEntity);
            Assert.Equal(insertedEntity.Id, dbEntity.Id);
            Assert.Equal(entity.FirstName, dbEntity.FirstName);
            Assert.Equal(entity.SecondName, dbEntity.SecondName);
            Assert.Equal(entity.Birthday, dbEntity.Birthday);
        }

        [Fact]
        public void Update_Test()
        {
            var count = 10;
            var insertedEntities = new IdentityEntity[count];
            for (var i = 0; i < count; i++)
                insertedEntities[i] = _fixture.Db.Insert(GetIdentityEntity());

            var entity = GetIdentityEntity();
            entity.Id = insertedEntities[3].Id;
            entity.FirstName = "A";
            var execResult = _fixture.Db.Update(entity);

            var dbEntity = _fixture.Db.GetByKey<IdentityEntity, IIdentityEntity>(new IdentityKey(entity.Id));
            Assert.NotEqual(insertedEntities[3], dbEntity);
            Assert.Equal(entity, dbEntity);
        }

        [Fact]
        public void UpdateWhere_Test()
        {
            var count = 10;
            var insertedEntities = new IdentityEntity[count];
            for (var i = 0; i < count; i++)
                insertedEntities[i] = _fixture.Db.Insert(GetIdentityEntity());

            var count1 = _fixture.Db.GetScalar<IdentityEntity, int>(i => i.Count(), i => i.FirstName == "A");
            Assert.Equal(0, count1);

            var execResult = _fixture.Db.Update<IdentityEntity>(new { FirstName = "A" }, i => i.Id < 5);
            Assert.Equal(5, execResult);

            var count2 = _fixture.Db.GetScalar<IdentityEntity, int>(i => i.Count(), i => i.FirstName == "A");
            Assert.Equal(5, count2);
        }

        [Fact]
        public void Delete_Test()
        {
            var count = 10;
            var insertedEntities = new IdentityEntity[count];
            for (var i = 0; i < count; i++)
                insertedEntities[i] = _fixture.Db.Insert(GetIdentityEntity());

            var count1 = _fixture.Db.GetScalar<IdentityEntity, int>(i => i.Count());
            Assert.Equal(count, count1);

            var execResult = _fixture.Db.Delete<IdentityEntity, IIdentityEntity>(new IdentityKey(3));
            Assert.Equal(1, execResult);

            var dbEntity = _fixture.Db.GetByKey<IdentityEntity, IIdentityEntity>(new IdentityKey(3));
            Assert.Null(dbEntity);

            var count2 = _fixture.Db.GetScalar<IdentityEntity, int>(i => i.Count());
            Assert.Equal(count - 1, count2);
        }

        [Fact]
        public void DeleteWhere_Test()
        {
            var count = 10;
            var insertedEntities = new IdentityEntity[count];
            for (var i = 0; i < count; i++)
                insertedEntities[i] = _fixture.Db.Insert(GetIdentityEntity());

            var count1 = _fixture.Db.GetScalar<IdentityEntity, int>(i => i.Count());
            Assert.Equal(count, count1);

            var execResult = _fixture.Db.Delete<IdentityEntity>(i => i.Id < 5);
            Assert.Equal(5, execResult);

            for (var i = 0; i < 5; i++)
            {
                var dbEntity = _fixture.Db.GetByKey<IdentityEntity, IIdentityEntity>(new IdentityKey(i));
                Assert.Null(dbEntity);
            }

            var count2 = _fixture.Db.GetScalar<IdentityEntity, int>(i => i.Count());
            Assert.Equal(count - 5, count2);
        }
    }
}
