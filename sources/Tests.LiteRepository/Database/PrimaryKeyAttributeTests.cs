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
    public class PrimaryKeyAttributeTests
    {
        [Fact]
        public void Ctor_Default_Test()
        {
            var pk = new PrimaryKeyAttribute();

            Assert.False(pk.IsIdentity);
        }

        [Fact]
        public void Ctor_IsIdentity_True_Test()
        {
            var pk = new PrimaryKeyAttribute(isIdentity: true);

            Assert.True(pk.IsIdentity);
        }
    }
}
