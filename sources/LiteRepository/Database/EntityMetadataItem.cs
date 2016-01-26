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
    public sealed class EntityMetadataItem
    {
        public string Name
        {
            get; private set;
        }

        public string DbName
        {
            get; private set;
        }

        public bool IsPrimaryKey
        {
            get; private set;
        }

        public bool IsIdentity
        {
            get; private set;
        }

        public EntityMetadataItem(string name, string dbName, bool isPrimaryKey, bool isIdentity)
        {
            Name = name;
            DbName = dbName;
            IsPrimaryKey = isPrimaryKey;
            IsIdentity = isIdentity;
        }
    }
}
