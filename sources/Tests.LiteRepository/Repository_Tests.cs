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
using System.Linq.Expressions;
using System.Data.Common;

namespace LiteRepository
{
    public class Repository_Tests
    {
        private Db GetDb()
        {
            return Substitute.For<Db>(Substitute.For<SqlDialect>(), Substitute.For<DbConnection>());
        }

        [Fact]
        public void Ctor_Test()
        {
            var db = GetDb();;
            var r = new Repository<Entity, EntityKey>(db);

            Assert.Equal(db, r.Db);
        }

        [Fact]
        public void Ctor_Null_Test()
        {
            Assert.Throws<ArgumentNullException>(() => new Repository<Entity, EntityKey>(null));
        }

        [Fact]
        public async void InsertAsync_Test()
        {
            var entity = new Entity();

            var db = GetDb();;
            var r = new Repository<Entity, EntityKey>(db);
            await r.InsertAsync(entity);

            await db.Received(1).InsertAsync(entity);
        }

        [Fact]
        public async void UpdateAsync_Test()
        {
            var entity = new Entity();

            var db = GetDb();;
            var r = new Repository<Entity, EntityKey>(db);
            await r.UpdateAsync(entity);

            await db.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async void DeleteAsync_Test()
        {
            var key = new EntityKey();

            var db = GetDb();;
            var r = new Repository<Entity, EntityKey>(db);
            await r.DeleteAsync(key);

            await db.Received(1).DeleteAsync<Entity, EntityKey>(key);
        }

        [Fact]
        public async void GetAsync_Test()
        {
            var key = new EntityKey();

            var db = GetDb();;
            var r = new Repository<Entity, EntityKey>(db);
            await r.GetAsync(key);

            await db.Received(1).GetByKeyAsync<Entity, EntityKey>(key);
        }

        [Fact]
        public async void GetAllAsync_Test()
        {
            var db = GetDb();;
            var r = new Repository<Entity, EntityKey>(db);
            await r.GetAllAsync();

            await db.Received(1).GetAsync<Entity>();
        }

        [Fact]
        public async void GetCountAsync_Test()
        {
            var db = GetDb();;
            var r = new Repository<Entity, EntityKey>(db);
            await r.GetCountAsync();

            Expression<Func<IEnumerable<Entity>, long>> countExpression = i => i.Count();

            await db.Received(1).GetScalarAsync<Entity, long>(
                Arg.Is<Expression<Func<IEnumerable<Entity>, long>>>(x => x.ToString() == countExpression.ToString()));
        }

    }
}
