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

namespace LiteRepository.Database
{
    public class EntityMetadataTests
    {
        private class Simple
        {
            [PrimaryKey(isIdentity:true)]
            public int Id { get; set; }

            public string Text { get; set; }
        }

        [StoreAs("entity")]
        private class Complex
        {
            [PrimaryKey()]
            [StoreAs("id")]
            public int Id { get; set; }

            [PrimaryKey()]
            [StoreAs("dept_id")]
            public int DeptId { get; set; }

            [Ignore]
            public string Num { get; set; }

            [StoreAs("full_name")]
            public string FullName { get; set; }
        }

        [Fact]
        public void Simple_Test()
        {
            var md = new EntityMetadata(typeof(Simple));

            Assert.Equal(2, md.Count);
            Assert.Equal("Simple", md.Name);
            Assert.Equal("Simple", md.DbName);

            Assert.Equal("Id", md[0].Name);
            Assert.Equal("Id", md[0].DbName);
            Assert.True(md[0].IsPrimaryKey);
            Assert.True(md[0].IsIdentity);

            Assert.Equal("Text", md[1].Name);
            Assert.Equal("Text", md[1].DbName);
            Assert.False(md[1].IsPrimaryKey);
            Assert.False(md[1].IsIdentity);
        }

        [Fact]
        public void Complex_Test()
        {
            var md = new EntityMetadata(typeof(Complex));

            Assert.Equal(3, md.Count);
            Assert.Equal("Complex", md.Name);
            Assert.Equal("entity", md.DbName);

            Assert.Equal("Id", md[0].Name);
            Assert.Equal("id", md[0].DbName);
            Assert.True(md[0].IsPrimaryKey);
            Assert.False(md[0].IsIdentity);

            Assert.Equal("DeptId", md[1].Name);
            Assert.Equal("dept_id", md[1].DbName);
            Assert.True(md[1].IsPrimaryKey);
            Assert.False(md[1].IsIdentity);

            Assert.Equal("FullName", md[2].Name);
            Assert.Equal("full_name", md[2].DbName);
            Assert.False(md[2].IsPrimaryKey);
            Assert.False(md[2].IsIdentity);
        }
    }
}
