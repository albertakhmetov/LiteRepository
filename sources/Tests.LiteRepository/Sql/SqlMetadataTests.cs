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
using LiteRepository.Sql.Attributes;
using LiteRepository.Common;

namespace LiteRepository.Sql
{
    public class SqlMetadataTests
    {
        private class Simple
        {
            [SqlKeyAttribute()]
            public int Id { get; set; }

            public string Text { get; set; }
        }

        private class Identity : IIdentityEntity
        {
            public long Id { get; set; }
            [SqlKeyAttribute] // must be ignored
            public string Text { get; set; }

            public object UpdateId(long id)
            {
                Id = id;
                return this; // this object is mutable, therefore just changes id and returns itself
            }
        }

        [SqlAliasAttribute("entity")]
        private class Complex
        {
            [SqlKeyAttribute()]
            [SqlAliasAttribute("id")]
            public int Id { get; set; }

            [SqlKeyAttribute()]
            [SqlAliasAttribute("dept_id")]
            public int DeptId { get; set; }

            [SqlIgnoreAttribute]
            public string Num { get; set; }

            [SqlAliasAttribute("full_name")]
            public string FullName { get; set; }
        }

        [Fact]
        public void Item_Ctor_Test()
        {
            const string name = "Entity";
            const string dbName = "DbEntity";

            var md1 = new SqlMetadata.Property(name, dbName, true, false);
            Assert.Equal(name, md1.Name);
            Assert.Equal(dbName, md1.DbName);
            Assert.True(md1.IsPrimaryKey);
            Assert.False(md1.IsIdentity);

            var md2 = new SqlMetadata.Property(name, dbName, false, true);
            Assert.Equal(name, md2.Name);
            Assert.Equal(dbName, md2.DbName);
            Assert.False(md2.IsPrimaryKey);
            Assert.True(md2.IsIdentity);
        }

        [Fact]
        public void Ctor_Test()
        {
            var m = new SqlMetadata(typeof(string));
            Assert.Equal(typeof(string), m.Type);
        }

        [Fact]
        public void Ctor_Null_Test()
        {
            Assert.Throws<ArgumentNullException>(()=>new SqlMetadata(null));
        }

        [Fact]
        public void Simple_Test()
        {
            var md = new SqlMetadata(typeof(Simple));
            Assert.False(md.IsIdentity);

            Assert.Equal(2, md.Count);
            Assert.Equal("Simple", md.Name);
            Assert.Equal("Simple", md.DbName);

            Assert.Equal("Id", md[0].Name);
            Assert.Equal("Id", md[0].DbName);
            Assert.True(md[0].IsPrimaryKey);
            Assert.False(md[0].IsIdentity);

            Assert.Equal("Text", md[1].Name);
            Assert.Equal("Text", md[1].DbName);
            Assert.False(md[1].IsPrimaryKey);
            Assert.False(md[1].IsIdentity);
        }


        [Fact]
        public void Identity_Test()
        {
            var md = new SqlMetadata(typeof(Identity));
            Assert.True(md.IsIdentity);

            Assert.Equal(2, md.Count);
            Assert.Equal("Identity", md.Name);
            Assert.Equal("Identity", md.DbName);

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
            var md = new SqlMetadata(typeof(Complex));
            Assert.False(md.IsIdentity);

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
