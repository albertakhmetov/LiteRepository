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
    public class SqlExpression_OrderTests
    {
        [Fact]
        public void NullExpression_Test()
        {
            var exp = new SqlExpression<Entity>();
            var sql = exp.GetOrderSql(null);
            Assert.Equal(string.Empty, sql);
        }

        [Fact]
        public void Order_Test()
        {
            var exp = new SqlExpression<Entity>();
            var expected = "ORDER BY birthday";
            var sql = exp.GetOrderSql(i => i.OrderBy(x => x.Birthday));
            Assert.Equal(expected, sql);
        }

        [Fact]
        public void OrderOrderByDesc_Test()
        {
            var exp = new SqlExpression<Entity>();
            var expected = "ORDER BY birthday, second_name DESC";
            var sql = exp.GetOrderSql(i => i.OrderBy(x => x.Birthday).OrderByDescending(x=>x.SecondName));
            Assert.Equal(expected, sql);
        }
    }
}
