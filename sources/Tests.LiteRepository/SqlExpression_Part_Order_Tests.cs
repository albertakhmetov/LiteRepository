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
    public class SqlExpression_Part_Order_Tests
    {
        [Fact]
        public void NullExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);

            exp.GetSelectSql();
            dialect.Received(1).Select(exp.Metadata.DbName, Arg.Any<string>(), string.Empty, string.Empty);
        }

        [Fact]
        public void Order_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "birthday";

            exp.GetSelectSql(orderBy: i => i.OrderBy(x => x.Birthday));
            dialect.Received(1).Select(exp.Metadata.DbName, Arg.Any<string>(), string.Empty, expected);
        }

        [Fact]
        public void OrderOrderByDesc_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "birthday, second_name DESC";

            exp.GetSelectSql(orderBy: i => i.OrderBy(x => x.Birthday).OrderByDescending(x => x.SecondName));
            dialect.Received(1).Select(exp.Metadata.DbName, Arg.Any<string>(), string.Empty, expected);
        }
    }
}
