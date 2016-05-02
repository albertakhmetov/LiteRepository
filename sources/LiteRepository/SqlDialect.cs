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
    /// Provides functionality to generate SQL.
    /// </summary>
    public abstract class SqlDialect
    {
        /// <summary>
        /// Creates a select SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fields">Comma-separated list of columns.</param>
        /// <param name="where">Where conditions. For example: column1 = 'abc' and column2 &lt; 2.</param>
        /// <param name="orderBy">Comma-separated list of columns for sorting.</param>
        /// <param name="top">Value what represents a limit for row to retrive.</param>
        /// <returns>A string with a select query.</returns>
        public abstract string Select(string tableName, string fields, string where, string orderBy, int? top = null);

        /// <summary>
        /// Creates a scalar select SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="expression">Scalar expression. For example: sum(column1).</param>
        /// <param name="where">Where conditions. For example: column1 = 'abc' and column2 &lt; 2.</param>
        /// <returns>A string with a select scalar query.</returns>
        public abstract string SelectScalar(string tableName, string expression, string where);

        /// <summary>
        /// Creates a insert SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fields">Comma-separated list of fields.</param>
        /// <param name="values">Comma-separated list of values.</param>
        /// <param name="isIdentity">Determines whether a table is a table with identity key.</param>
        /// <returns>A string with a insert query.</returns>
        public abstract string Insert(string tableName, string fields, string values, bool isIdentity);

        /// <summary>
        /// Creates a update SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="set">A list of set expressions</param>
        /// <param name="where">Where conditions. For example: column1 = 'abc' and column2 &lt; 2.</param>
        /// <returns>A string with a update query.</returns>
        public abstract string Update(string tableName, string set, string where);

        /// <summary>
        /// Creates a delete SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="where">Where conditions. For example: column1 = 'abc' and column2 &lt; 2.</param>
        /// <returns>A string with a delete query.</returns>
        public abstract string Delete(string tableName, string where);

        /// <summary>
        /// Returns a parameter with a symbol that represents parameter in <see cref="System.Data.Common.DbParameter"/>.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>A parameter string with parameter symbol.</returns>
        public abstract string Parameter(string name);

        /// <summary>
        /// Checks the string whether it has parameters.
        /// </summary>
        /// <param name="value">Source string to check.</param>
        /// <returns>true if string has parameter; otherwise, false.</returns>
        public abstract bool HasParameters(string value);
    }
}
