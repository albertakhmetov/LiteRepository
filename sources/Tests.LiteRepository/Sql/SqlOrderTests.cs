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

namespace LiteRepository.Sql
{
    public class SqlOrderTests
    {
        [Fact]
        public void Ctor_Default_Test()
        {
            var dbName = "age";
            var order = new SqlOrder(dbName);
            Assert.Equal(dbName, order.Name);
            Assert.Equal(SqlOrder.SqlDirection.Asc, order.Direction);
        }

        [Fact]
        public void Ctor_Test()
        {
            var dbName = "age";
            var direction = SqlOrder.SqlDirection.Desc;
            var order = new SqlOrder(dbName, direction);
            Assert.Equal(dbName, order.Name);
            Assert.Equal(direction, order.Direction);
        }

        [Fact]
        public void Ctor_NullOrEmpty_Test()
        {
            Assert.Throws<ArgumentException>(() => new SqlOrder(null));
            Assert.Throws<ArgumentException>(() => new SqlOrder(string.Empty));
            Assert.Throws<ArgumentException>(() => new SqlOrder("   "));
        }
    }
}
