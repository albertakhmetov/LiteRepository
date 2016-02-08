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
            var sql = exp.GetSelectScalarPartSql<object>(null);
            Assert.Equal(string.Empty, sql);
        }

        [Fact]
        public void Count_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "COUNT(1)";
            var sql = exp.GetSelectScalarPartSql(i => i.Count());
            Assert.Equal(expected, sql);
        }

        [Fact]
        public void Avg_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "AVG(cource)";
            var sql = exp.GetSelectScalarPartSql(i => i.Average(x => x.Cource));
            Assert.Equal(expected, sql);
        }

        [Fact]
        public void Sum_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "SUM(cource)";
            var sql = exp.GetSelectScalarPartSql(i => i.Sum(x => x.Cource));
            Assert.Equal(expected, sql);
        }
    }
}
