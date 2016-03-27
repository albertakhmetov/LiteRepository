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

namespace LiteRepository
{
    /// <summary>
    /// Provides functionality to generate SQL
    /// </summary>
    public abstract class SqlDialect
    {
        /// <summary>
        /// Returns select SQL
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="fields">A list of columns separated by commas</param>
        /// <param name="where">Where conditions. For example: column1 = 'abc' and column2 &lt; 2</param>
        /// <param name="orderBy"></param>
        /// <param name="top"></param>
        /// <returns>Returns SQL for select command</returns>
        public abstract string Select(string tableName, string fields, string where, string orderBy, int? top = null);

        /// <summary>
        /// Returns scalar select SQL
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="expression">Scalar expression. For example: sum(column1)</param>
        /// <param name="where">Where conditions. For example: column1 = 'abc' and column2 &lt; 2</param>
        /// <returns>Returns SQL for scalar select command</returns>
        public abstract string SelectScalar(string tableName, string expression, string where);

        /// <summary>
        /// Returns insert SQL
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="fields">A list of columns separated by commas</param>
        /// <param name="values">A list of values separated by commas</param>
        /// <param name="isIdentity">Determines whether a table is a table with identity key.</param>
        /// <returns>Returns SQL for insert command</returns>
        public abstract string Insert(string tableName, string fields, string values, bool isIdentity);

        /// <summary>
        /// Returns update SQL
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="set">A list of set expressions</param>
        /// <param name="where">Where conditions. For example: column1 = 'abc' and column2 &lt; 2</param>
        /// <returns>Returns SQL for update command</returns>
        public abstract string Update(string tableName, string set, string where);

        /// <summary>
        /// Returns delete SQL
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="where">Where conditions. For example: column1 = 'abc' and column2 &lt; 2</param>
        /// <returns>Returns SQL for delete command</returns>
        public abstract string Delete(string tableName, string where);

        /// <summary>
        /// Returns parameter with a symbol that represents parameter in <see cref="System.Data.Common.DbParameter"/>.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>Returns parameter with parameter symbol</returns>
        public abstract string Parameter(string name);

        /// <summary>
        /// Gets a value indicating whether string has parameters.
        /// </summary>
        /// <param name="value">String</param>
        /// <returns>Returns true if string has parameter or false otherwise</returns>
        public abstract bool HasParameters(string value);
    }
}
