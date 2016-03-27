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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Attributes
{
    /// <summary>
    /// Represents the database column (table) that a property (class) is mapped to
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class SqlAliasAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the column (table) the property (class) is mapped to
        /// </summary>
        public string DbName
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlAliasAttribute"/> class.
        /// </summary>
        /// <param name="dbName">The name of the column (table) the property (class) is mapped to</param>
        public SqlAliasAttribute(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentException(nameof(dbName));

            DbName = dbName;
        }
    }
}
