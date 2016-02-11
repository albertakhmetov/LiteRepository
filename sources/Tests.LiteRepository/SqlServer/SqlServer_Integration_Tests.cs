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
using System.Data.SqlClient;
using System.Configuration;
using LiteRepository.Models;

namespace LiteRepository.SqlServer
{
    public sealed class SqlServerFixture
    {
        public Db Db
        {
            get; private set;
        }

        public SqlServerFixture()
        {
            Db = new Db(new SqlServerDialect(), () => new SqlConnection(ConfigurationManager.ConnectionStrings["DemoDb"].ConnectionString));
        }
    }

    public class SqlServer_Integration_Tests : IClassFixture<SqlServerFixture>
    {
        private readonly SqlServerFixture _fixture;

        public SqlServer_Integration_Tests(SqlServerFixture fixture)
        {
            //_fixture = fixture;
            //_fixture.Db.Delete<Entity>();
            //_fixture.Db.Delete<IdentityEntity>();
        }

        [Fact]
        public void Insert_Test()
        {
         //   _fixture.Db.Insert()
        }

        [Fact]
        public void InsertIdentity_Test()
        {

        }

        [Fact]
        public void Update_Test()
        {

        }

        [Fact]
        public void Delete_Test()
        {

        }
    }
}
