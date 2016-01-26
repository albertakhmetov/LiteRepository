﻿/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteRepository.Database.SqlServer.Models;

namespace LiteRepository.Database.SqlServer
{
    public class SqlServerGenerators
    {
        public SqlServerGenerator<IdentityEntity> IdentityGenerator
        {
            get; private set;
        }

        public SqlServerGenerator<Entity> EntityGenerator
        {
            get; private set;
        }

        public SqlServerGenerators()
        {
            IdentityGenerator = new SqlServerGenerator<IdentityEntity>();
            EntityGenerator = new SqlServerGenerator<Entity>();
        }
    }

    public class SqlServerGeneratorTests : IClassFixture<SqlServerGenerators>
    {
        private readonly SqlServerGenerators _fixture;

        public SqlServerGeneratorTests(SqlServerGenerators fixture)
        {
            _fixture = fixture;
        }

        #region Identity

        [Fact]
        public void Identity_Insert_Test()
        {
            var expected = "INSERT INTO people (first_name, second_name, birthday) VALUES (@FirstName, @SecondName, @Birthday)\r\nSELECT SCOPE_IDENTITY()";
            Assert.Equal(expected, _fixture.IdentityGenerator.InsertSql);
        }

        [Fact]
        public void Identity_Update_Test()
        {
            var expected = "UPDATE people SET first_name = @FirstName, second_name = @SecondName, birthday = @Birthday WHERE id = @Id";
            Assert.Equal(expected, _fixture.IdentityGenerator.UpdateSql);
        }

        [Fact]
        public void Identity_Delete_Test()
        {
            var expected = "DELETE FROM people WHERE id = @Id";
            Assert.Equal(expected, _fixture.IdentityGenerator.DeleteSql);
        }

        [Fact]
        public void Identity_DeleteAll_Test()
        {
            var expected = "DELETE FROM people";
            Assert.Equal(expected, _fixture.IdentityGenerator.DeleteAllSql);
        }

        [Fact]
        public void Identity_Select_Test()
        {
            var expected = "SELECT id AS Id, first_name AS FirstName, second_name AS SecondName, birthday AS Birthday FROM people WHERE id = @Id";
            Assert.Equal(expected, _fixture.IdentityGenerator.SelectSql);
        }

        [Fact]
        public void Identity_SelectAll_Test()
        {
            var expected = "SELECT id AS Id, first_name AS FirstName, second_name AS SecondName, birthday AS Birthday FROM people";
            Assert.Equal(expected, _fixture.IdentityGenerator.SelectAllSql);
        }

        [Fact]
        public void Identity_Count_Test()
        {
            var expected = "SELECT COUNT(1) FROM people";
            Assert.Equal(expected, _fixture.IdentityGenerator.CountSql);
        }

        #endregion

        #region Entity

        [Fact]
        public void Entity_Insert_Test()
        {
            var expected = "INSERT INTO cash_check_item (id, shop_id, text, price) VALUES (@Id, @ShopId, @Text, @Price)";
            Assert.Equal(expected, _fixture.EntityGenerator.InsertSql);
        }

        [Fact]
        public void Entity_Update_Test()
        {
            var expected = "UPDATE cash_check_item SET text = @Text, price = @Price WHERE id = @Id AND shop_id = @ShopId";
            Assert.Equal(expected, _fixture.EntityGenerator.UpdateSql);
        }

        [Fact]
        public void Entity_Delete_Test()
        {
            var expected = "DELETE FROM cash_check_item WHERE id = @Id AND shop_id = @ShopId";
            Assert.Equal(expected, _fixture.EntityGenerator.DeleteSql);
        }

        [Fact]
        public void Entity_DeleteAll_Test()
        {
            var expected = "DELETE FROM cash_check_item";
            Assert.Equal(expected, _fixture.EntityGenerator.DeleteAllSql);
        }

        [Fact]
        public void Entity_Select_Test()
        {
            var expected = "SELECT id AS Id, shop_id AS ShopId, text AS Text, price AS Price FROM cash_check_item WHERE id = @Id AND shop_id = @ShopId";
            Assert.Equal(expected, _fixture.EntityGenerator.SelectSql);
        }

        [Fact]
        public void Entity_SelectAll_Test()
        {
            var expected = "SELECT id AS Id, shop_id AS ShopId, text AS Text, price AS Price FROM cash_check_item";
            Assert.Equal(expected, _fixture.EntityGenerator.SelectAllSql);
        }

        [Fact]
        public void Entity_Count_Test()
        {
            var expected = "SELECT COUNT(1) FROM cash_check_item";
            Assert.Equal(expected, _fixture.EntityGenerator.CountSql);
        }

        #endregion
    }
}