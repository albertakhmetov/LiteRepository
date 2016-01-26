/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Database
{
    public class StoreAsAttributeTests
    {
        [Fact]
        public void Ctor_NullOrEmpty_Test()
        {
            Assert.Throws<ArgumentException>(() => new StoreAsAttribute(null));
            Assert.Throws<ArgumentException>(() => new StoreAsAttribute(string.Empty));
            Assert.Throws<ArgumentException>(() => new StoreAsAttribute("   "));
        }

        [Fact]
        public void Ctor_Test()
        {
            const string dbName = "nameInDb";
            var sa = new StoreAsAttribute(dbName);

            Assert.Equal(dbName, sa.DbName);
        }
    }
}
