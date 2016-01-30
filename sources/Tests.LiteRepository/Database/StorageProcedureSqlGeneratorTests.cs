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
    public class StorageProcedureSqlGeneratorTests
    {
        [Fact]
        public void Ctor_Test()
        {
            const string insertSql = "insert";
            const string updateSql = "update";
            const string deleteSql = "delete";
            const string deleteAllSql = "delete all";
            const string selectSql = "select";
            const string selectAllSql = "select all";
            const string countSql = "count";
            const bool isIdentity = true;

            var g = new StorageProcedureSqlGenerator(
                insertSql: insertSql,
                updateSql: updateSql,
                deleteSql: deleteSql,
                deleteAllSql: deleteAllSql,
                selectSql: selectSql,
                selectAllSql: selectAllSql,
                countSql: countSql,
                isIdentity: isIdentity);

            Assert.Equal(insertSql, g.InsertSql);
            Assert.Equal(updateSql, g.UpdateSql);
            Assert.Equal(deleteSql, g.DeleteSql);
            Assert.Equal(deleteAllSql, g.DeleteAllSql);
            Assert.Equal(selectSql, g.SelectSql);
            Assert.Equal(selectAllSql, g.SelectAllSql);
            Assert.Equal(countSql, g.CountSql);
            Assert.Equal(isIdentity, g.IsIdentity);
        }
    }
}
