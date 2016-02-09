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
    public class SqlExpression_Insert_Tests
    {
        [Fact]
        public void Null_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            dialect.Parameter("Cource").Returns("%Cource");
            dialect.Parameter("Letter").Returns("%Letter");
            dialect.Parameter("LocalId").Returns("%LocalId");
            dialect.Parameter("FirstName").Returns("%FirstName");
            dialect.Parameter("SecondName").Returns("%SecondName");
            dialect.Parameter("Birthday").Returns("%Birthday");

            var exp = new SqlExpression<Entity>(dialect);
            var expectedFields = "first_name, second_name, birthday, cource, letter, local_id";
            var expectedValues = "%FirstName, %SecondName, %Birthday, %Cource, %Letter, %LocalId";

            exp.GetInsertSql();
            dialect.Received(1).Insert(exp.Metadata.DbName, expectedFields, expectedValues);
        }

        [Fact]
        public void InsertSub_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            dialect.Parameter("FirstName").Returns("%FirstName");
            dialect.Parameter("SecondName").Returns("%SecondName");

            var exp = new SqlExpression<Entity>(dialect);
            var expectedFields = "first_name, second_name";
            var expectedValues = "%FirstName, %SecondName";

            var p = new { FirstName = "A", SecondName = "I" };

            exp.GetInsertSql(p.GetType());
            dialect.Received(1).Insert(exp.Metadata.DbName, expectedFields, expectedValues);
        }

        [Fact]
        public void InsertIntersect_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            dialect.Parameter("FirstName").Returns("%FirstName");
            dialect.Parameter("SecondName").Returns("%SecondName");

            var exp = new SqlExpression<Entity>(dialect);
            var expectedFields = "first_name, second_name";
            var expectedValues = "%FirstName, %SecondName";

            var p = new { FirstName = "A", SecondName = "I", IsStudent = false };

            exp.GetInsertSql(p.GetType());
            dialect.Received(1).Insert(exp.Metadata.DbName, expectedFields, expectedValues);
        }

        [Fact]
        public void InsertDifferent_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var p = new { IsStudent = false };

            Assert.Throws<InvalidOperationException>(() => exp.GetInsertSql(p.GetType()));
        }

        [Fact]
        public void InsertIdentity_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            dialect.Parameter("Id").Returns("%Id");
            dialect.Parameter("FirstName").Returns("%FirstName");
            dialect.Parameter("SecondName").Returns("%SecondName");
            dialect.Parameter("Birthday").Returns("%Birthday");

            var exp = new SqlExpression<IdentityEntity>(dialect);
            var expectedFields = "first_name, second_name, birthday";
            var expectedValues = "%FirstName, %SecondName, %Birthday";

            exp.GetInsertSql();
            dialect.Received(1).Insert(exp.Metadata.DbName, expectedFields, expectedValues);
        }
    }
}
