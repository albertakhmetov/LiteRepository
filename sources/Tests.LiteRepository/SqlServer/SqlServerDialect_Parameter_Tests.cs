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

namespace LiteRepository.SqlServer
{
    public class SqlServerDialect_Parameter_Tests
    {
        [Fact]
        public void HasParameters_Yes_Test()
        {
            var dialect = new SqlServerDialect();
            Assert.True(dialect.HasParameters("id=@Id"));
        }

        [Fact]
        public void HasParameters_No_Test()
        {
            var dialect = new SqlServerDialect();
            Assert.False(dialect.HasParameters("id=123"));
        }

        [Fact]
        public void Parameter_WithAt_Test()
        {
            var dialect = new SqlServerDialect();
            var sql = dialect.Parameter("@id");
            Assert.Equal("@id", sql);
        }

        [Fact]
        public void Parameter_WithoutAt_Test()
        {
            var dialect = new SqlServerDialect();
            var sql = dialect.Parameter("id");
            Assert.Equal("@id", sql);
        }
    }
}
