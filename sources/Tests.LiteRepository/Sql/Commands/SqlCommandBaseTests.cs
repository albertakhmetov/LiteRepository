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
using System.Reflection;

namespace LiteRepository.Sql.Commands
{
    public class SqlCommandBaseTests
    {
        [Fact]
        public void Ctor_Test()
        {
            var sqlBuilder = Substitute.For<ISqlBuilder>();

            var cmd = Substitute.For<SqlCommandBase<Entity>>(sqlBuilder);
            Assert.Equal(sqlBuilder, cmd.SqlBuilder);
        }

        [Fact]
        public void Ctor_Null_Test()
        {
            try
            {
                Substitute.For<SqlCommandBase<Entity>>(default(ISqlBuilder));
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException == null || ex.InnerException.GetType() != typeof(ArgumentNullException))
                    Assert.True(false, "expected ArgumentNullException, but was " + ex.GetType());
            }
        }
    }
}
