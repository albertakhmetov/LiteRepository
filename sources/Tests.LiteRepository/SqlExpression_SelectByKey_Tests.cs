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
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteRepository.Models;

namespace LiteRepository
{
    public class SqlExpression_SelectByKey_Tests
    {
        [Fact]
        public void Null_Test()
        {
            var dialect = Substitute.For<SqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name AS FirstName, second_name AS SecondName, birthday AS Birthday, cource AS Cource, letter AS Letter, local_id AS LocalId";

            exp.GetSelectByKeySql();
            dialect.Received(1).Select(exp.Metadata.DbName, expected, Arg.Any<string>(), string.Empty);
        }

        [Fact]
        public void Where_Test()
        {
            var dialect = Substitute.For<SqlDialect>();
            dialect.Parameter("Cource").Returns("%Cource");
            dialect.Parameter("Letter").Returns("%Letter");
            dialect.Parameter("LocalId").Returns("%LocalId");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = %Cource AND letter = %Letter AND local_id = %LocalId";

            exp.GetSelectByKeySql();
            dialect.Received(1).Select(exp.Metadata.DbName, Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void SelectSubEntity_Test()
        {
            var dialect = Substitute.For<SqlDialect>();
            dialect.Parameter("Cource").Returns("%Cource");
            dialect.Parameter("Letter").Returns("%Letter");
            dialect.Parameter("LocalId").Returns("%LocalId");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "second_name AS SecondName, letter AS Letter";
            var expectedWhere = "cource = %Cource AND letter = %Letter AND local_id = %LocalId";
            var p = new { Letter = 'A', SecondName = "B" };

            exp.GetSelectByKeySql(p.GetType());
            dialect.Received(1).Select(exp.Metadata.DbName, expected, expectedWhere, string.Empty);
        }

        [Fact]
        public void SelectIntersectEntity_Test()
        {
            var dialect = Substitute.For<SqlDialect>();
            dialect.Parameter("Cource").Returns("%Cource");
            dialect.Parameter("Letter").Returns("%Letter");
            dialect.Parameter("LocalId").Returns("%LocalId");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "second_name AS SecondName, letter AS Letter";
            var expectedWhere = "cource = %Cource AND letter = %Letter AND local_id = %LocalId";
            var p = new { Letter = 'A', IsStudent = false, SecondName = "B" };

            exp.GetSelectByKeySql(p.GetType());
            dialect.Received(1).Select(exp.Metadata.DbName, expected, expectedWhere, string.Empty);
        }

        [Fact]
        public void SelectDifferentEntity_Test()
        {
            var dialect = Substitute.For<SqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var p = new { Salary = 100m, IsStudent = false };
                       
            Assert.Throws<InvalidOperationException>(() =>  exp.GetSelectSql(p.GetType()));
        }
    }
}
