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

namespace LiteRepository
{
    public class SqlExpression_Update_Tests
    {
        [Fact]
        public void Null_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("Cource").Returns("%Cource");
            dialect.Parameter("Letter").Returns("%Letter");
            dialect.Parameter("LocalId").Returns("%LocalId");
            dialect.Parameter("FirstName").Returns("%FirstName");
            dialect.Parameter("SecondName").Returns("%SecondName");
            dialect.Parameter("Birthday").Returns("%Birthday");

            var exp = new SqlExpression<Entity>(dialect);
            var expectedSet = "first_name = %FirstName, second_name = %SecondName, birthday = %Birthday";
            var expectedWhere = "cource = %Cource AND letter = %Letter AND local_id = %LocalId";

            exp.GetUpdateSql();
            dialect.Received(1).Update(exp.Metadata.DbName, expectedSet, expectedWhere);
        }

        [Fact]
        public void Where_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "birthday = '2000-01-01 00:00:00'";

            exp.GetUpdateSql(where: i => i.Birthday == new DateTime(2000, 1, 1));
            dialect.Received(1).Update(exp.Metadata.DbName, Arg.Any<string>(), expected);
        }

        [Fact]
        public void WhereWithParameters_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("Birthday").Returns("%Birthday");
            dialect.HasParameters(Arg.Is<string>(x => x.Contains("%"))).Returns(true);

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "birthday = '2000-01-01 00:00:00'";

            var p = new { Birthday = new DateTime(2000, 1, 1) };

            exp.GetUpdateSql(where: i => i.Birthday == p.Birthday);
            dialect.Received(1).Update(exp.Metadata.DbName, Arg.Any<string>(), expected);
        }

        [Fact]
        public void UpdateSub_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("FirstName").Returns("%FirstName");
            dialect.Parameter("SecondName").Returns("%SecondName");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name = %FirstName, second_name = %SecondName";

            var p = new { FirstName = "A", SecondName = "I" };

            exp.GetUpdateSql(p.GetType());
            dialect.Received(1).Update(exp.Metadata.DbName, expected, Arg.Any<string>());
        }

        [Fact]
        public void UpdateIntersect_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("FirstName").Returns("%FirstName");
            dialect.Parameter("SecondName").Returns("%SecondName");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name = %FirstName, second_name = %SecondName";

            var p = new { FirstName = "A", SecondName = "I", IsStudent = false };

            exp.GetUpdateSql(p.GetType());
            dialect.Received(1).Update(exp.Metadata.DbName, expected, Arg.Any<string>());
        }

        [Fact]
        public void UpdateDifferent_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var p = new { IsStudent = false };

            Assert.Throws<InvalidOperationException>(() => exp.GetUpdateSql(p.GetType()));
        }

        [Fact]
        public void UpdateKey_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var p = new { Cource = 5 };

            Assert.Throws<InvalidOperationException>(() => exp.GetUpdateSql(p.GetType()));
        }

        [Fact]
        public void UpdateIdentity_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("Id").Returns("%Id");
            dialect.Parameter("FirstName").Returns("%FirstName");
            dialect.Parameter("SecondName").Returns("%SecondName");
            dialect.Parameter("Birthday").Returns("%Birthday");

            var exp = new SqlExpression<IdentityEntity>(dialect);
            var expected = "first_name = %FirstName, second_name = %SecondName, birthday = %Birthday";

            exp.GetUpdateSql();
            dialect.Received(1).Update(exp.Metadata.DbName, expected, Arg.Any<string>());
        }
    }
}
