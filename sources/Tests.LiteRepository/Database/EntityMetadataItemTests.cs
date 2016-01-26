/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

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
