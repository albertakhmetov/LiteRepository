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
    public class SqlExpression_Select_Tests
    {
        [Fact]
        public void Null_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource AS Cource, letter AS Letter, local_id AS LocalId, first_name AS FirstName, second_name AS SecondName, birthday AS Birthday";

            exp.GetSelectSql();
            dialect.Received(1).Select(exp.Metadata.DbName, expected, string.Empty, string.Empty);
        }

        [Fact]
        public void Where_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            dialect.Parameter("Cource").Returns("%Cource");
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = %Cource";
            var p = new { Cource = 123 };

            exp.GetSelectSql(where: i => i.Cource == p.Cource);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Order_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource";

            exp.GetSelectSql(orderBy: i => i.OrderBy(x => x.Cource));
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), string.Empty, expected);
        }

        [Fact]
        public void SelectSubEntity_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "letter AS Letter, second_name AS SecondName";
            var p = new { Letter = 'A', SecondName = "B" };

            exp.GetSelectSql(p.GetType());
            dialect.Received(1).Select(exp.Metadata.DbName, expected, string.Empty, string.Empty);
        }

        [Fact]
        public void SelectIntersectEntity_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "letter AS Letter, second_name AS SecondName";
            var p = new { Letter = 'A', IsStudent = false, SecondName = "B" };

            exp.GetSelectSql(p.GetType());
            dialect.Received(1).Select(exp.Metadata.DbName, expected, string.Empty, string.Empty);
        }

        [Fact]
        public void SelectDifferentEntity_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = string.Empty;
            var p = new { Salary = 100m, IsStudent = false };

            Assert.Throws<InvalidOperationException>(() => exp.GetSelectSql(p.GetType()));
        }
    }
}
