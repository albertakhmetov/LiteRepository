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
            var expected = "INSERT INTO students " +
                "(cource, letter, first_name, second_name, birthday) " +
                "VALUES (@Cource, @Letter, @FirstName, @SecondName, @Birthday)";
            Assert.Equal(expected, _fixture.SqlBuilder.GetInsertSql());
        }

        [Fact]
        public void GetUpdate_Test()
        {
            var expected = "UPDATE students SET " +
                "first_name = @FirstName, " +
                "second_name = @SecondName, " +
                "birthday = @Birthday " +
                "WHERE cource = @Cource AND letter = @Letter";
            Assert.Equal(expected, _fixture.SqlBuilder.GetUpdateSql());
        }

        [Fact]
        public void GetDelete_Test()
        {
            var expected = "DELETE FROM students WHERE cource = @Cource AND letter = @Letter";
            Assert.Equal(expected, _fixture.SqlBuilder.GetDeleteByKeySql());
        }

        [Fact]
        public void GetSelect_Test()
        {
            var expected = "SELECT " +
                "cource AS Cource, " +
                "letter AS Letter, " +
                "first_name AS FirstName, " +
                "second_name AS SecondName, " +
                "birthday AS Birthday " +
                "FROM students WHERE cource = @Cource AND letter = @Letter";
            Assert.Equal(expected, _fixture.SqlBuilder.GetSelectByKeySql());
        }

        [Fact]
        public void GetCount_Test()
        {
            var expected = "SELECT COUNT(1) FROM students";
            Assert.Equal(expected, _fixture.SqlBuilder.GetCountSql());
        }
    }
}
