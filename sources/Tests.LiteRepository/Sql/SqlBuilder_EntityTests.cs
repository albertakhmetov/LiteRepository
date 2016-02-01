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
using LiteRepository.Sql.Models;

namespace LiteRepository.Sql
{
    public class SqlBuilder_EntityFixture
    {
        public SqlBuilder<Entity> SqlBuilder
        {
            get; private set;
        }

        public SqlBuilder_EntityFixture()
        {
            SqlBuilder = new SqlBuilder<Entity>();
        }
    }

    public class SqlBuilder_EntityTests : IClassFixture<SqlBuilder_EntityFixture>
    {
        private readonly SqlBuilder_EntityFixture _fixture;

        public SqlBuilder_EntityTests(SqlBuilder_EntityFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void GetInsert_Test()
        {
            var expected = "INSERT INTO cash_check_item (id, shop_id, text, price) VALUES (@Id, @ShopId, @Text, @Price)";
            Assert.Equal(expected, _fixture.SqlBuilder.GetInsertSql());
        }

        [Fact]
        public void GetUpdate_Test()
        {
            var expected = "UPDATE cash_check_item SET text = @Text, price = @Price WHERE id = @Id AND shop_id = @ShopId";
            Assert.Equal(expected, _fixture.SqlBuilder.GetUpdateSql());
        }

        [Fact]
        public void GetDelete_Test()
        {
            var expected = "DELETE FROM cash_check_item WHERE id = @Id AND shop_id = @ShopId";
            Assert.Equal(expected, _fixture.SqlBuilder.GetDeleteSql());
        }

        [Fact]
        public void GetSelect_Test()
        {
            var expected = "SELECT Cource AS Id, shop_id AS ShopId, text AS Text, price AS Price FROM students WHERE Cource = @Cource AND shop_id = @ShopId";
            Assert.Equal(expected, _fixture.SqlBuilder.GetSelectSql());
        }

        [Fact]
        public void GetCount_Test()
        {
            var expected = "SELECT COUNT(1) FROM students";
            Assert.Equal(expected, _fixture.SqlBuilder.GetSelectCountSql());
        }
    }
}
