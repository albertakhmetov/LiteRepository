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

namespace LiteRepository.Database
{
    public class EntityMetadataItemTests
    {
        [Fact]
        public void Ctor_Test()
        {
            const string name = "Entity";
            const string dbName = "DbEntity";

            var md1 = new EntityMetadataItem(name, dbName, true, false);
            Assert.Equal(name, md1.Name);
            Assert.Equal(dbName, md1.DbName);
            Assert.True(md1.IsPrimaryKey);
            Assert.False(md1.IsIdentity);

            var md2 = new EntityMetadataItem(name, dbName, false, true);
            Assert.Equal(name, md2.Name);
            Assert.Equal(dbName, md2.DbName);
            Assert.False(md2.IsPrimaryKey);
            Assert.True(md2.IsIdentity);
        }
    }
}
