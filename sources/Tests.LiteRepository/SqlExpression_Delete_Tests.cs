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
    public class SqlExpression_Delete_Tests
    {
        [Fact]
        public void Null_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("Cource").Returns("%Cource");
            dialect.Parameter("Letter").Returns("%Letter");
            dialect.Parameter("LocalId").Returns("%LocalId");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = %Cource AND letter = %Letter AND local_id = %LocalId";

            exp.GetDeleteSql();
            dialect.Received(1).Delete(exp.Metadata.DbName, expected);
        }

        [Fact]
        public void Where_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("Birthday").Returns("%Birthday");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "birthday = %Birthday";

            var p = new { Birthday = new DateTime(2000, 1, 1) };

            exp.GetDeleteSql(where: i => i.Birthday == p.Birthday, param: p);
            dialect.Received(1).Delete(exp.Metadata.DbName, expected);
        }

        [Fact]
        public void Truncate_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);

            exp.GetTruncateSql();
            dialect.Received(1).Delete(exp.Metadata.DbName, string.Empty);
        }
    }
}
