/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Database
{
    [AttributeUsage(validOn: AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class StoreAsAttribute : Attribute
    {
        public string DbName
        {
            get;
            private set;
        }

        public StoreAsAttribute(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentException(nameof(dbName));

            DbName = dbName;
        }
    }
}
