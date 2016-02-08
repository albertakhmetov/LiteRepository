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
    public class SqlExpression_SelectScalar_Tests
    {
        [Fact]
        public void NullExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);

            Assert.Throws<ArgumentNullException>(() => exp.GetSelectScalarSql<int>(null));
        }

        [Fact]
        public void Where_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = @Cource";
            var p = new { Cource = 123 };

            exp.GetSelectScalarSql<int>(i => i.Count(), where: i => i.Cource == p.Cource);
            dialect.Received(1).SelectScalar(exp.Metadata.DbName, Arg.Any<string>(), expected);
        }

        [Fact]
        public void Count_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "COUNT(1)";

            exp.GetSelectScalarSql<int>(i => i.Count());
            dialect.Received(1).SelectScalar(exp.Metadata.DbName, expected, string.Empty);
        }

        [Fact]
        public void Avg_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "AVG(cource)";

            exp.GetSelectScalarSql<double>(i => i.Average(x => x.Cource));
            dialect.Received(1).SelectScalar(exp.Metadata.DbName, expected, string.Empty);
        }

        [Fact]
        public void Sum_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "SUM(cource)";

            exp.GetSelectScalarSql<long>(i => i.Sum(x => x.Cource));
            dialect.Received(1).SelectScalar(exp.Metadata.DbName, expected, string.Empty);
        }
    }
}
