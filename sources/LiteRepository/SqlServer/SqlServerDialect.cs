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

namespace LiteRepository.SqlServer
{
    /// <summary>
    /// Provides functionality to generate SQL with SQL Server dialect.
    /// </summary>
    public class SqlServerDialect : SqlDialect
    {
        private string GetWhere(string where)
        {
            return where.Length > 0 ? $" WHERE {where}" : string.Empty;
        }

        private string GetOrderBy(string orderBy)
        {
            return orderBy.Length > 0 ? $" ORDER BY {orderBy}" : string.Empty;
        }
        
        /// <inheritdoc/>
        public override string Select(string tableName, string fields, string where, string orderBy, int? top = null)
        {
            return $"SELECT {fields} FROM {tableName}{GetWhere(where)}{GetOrderBy(orderBy)}";
        }

        /// <inheritdoc/>
        public override string SelectScalar(string tableName, string expression, string where)
        {
            return $"SELECT {expression} FROM {tableName}{GetWhere(where)}";
        }

        /// <inheritdoc/>
        public override string Insert(string tableName, string fields, string values, bool isIdentity)
        {
            var sql = $"INSERT INTO {tableName} ({fields}) VALUES ({values})";
            if (isIdentity)
            {
                sql += Environment.NewLine;
                sql += "SELECT SCOPE_IDENTITY()";
            }
            return sql;
        }

        /// <inheritdoc/>
        public override string Update(string tableName, string set, string where)
        {
            return $"UPDATE {tableName} SET {set}{GetWhere(where)}";
        }

        /// <inheritdoc/>
        public override string Delete(string tableName, string where)
        {
            if (string.IsNullOrWhiteSpace(where))
                return $"TRUNCATE TABLE {tableName}";
            else
                return $"DELETE FROM {tableName}{GetWhere(where)}";
        }

        /// <inheritdoc/>
        public override string Parameter(string name)
        {
            return name.StartsWith("@") ? name : "@" + name;
        }

        /// <inheritdoc/>
        public override bool HasParameters(string value)
        {
            return value.Contains("@");
        }
    }
}
