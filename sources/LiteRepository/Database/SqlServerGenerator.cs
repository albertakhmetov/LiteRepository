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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Database
{
    public sealed class SqlServerGenerator : ISqlGenerator
    {
        public string InsertSql
        {
            get; private set;
        }

        public string UpdateSql
        {
            get; private set;
        }

        public string DeleteSql
        {
            get; private set;
        }

        public string DeleteAllSql
        {
            get; private set;
        }

        public string SelectSql
        {
            get; private set;
        }

        public string SelectAllSql
        {
            get; private set;
        }

        public string CountSql
        {
            get; private set;
        }

        public bool IsIdentity
        {
            get; private set;
        }

        public CommandType CommandType
        {
            get { return CommandType.Text; }
        }

        public SqlServerGenerator(Type entityType)
        {
            var metadata = new EntityMetadata(entityType);

            IsIdentity = metadata.FirstOrDefault(x => x.IsIdentity) != null;

            var selectFields = string.Join(", ", metadata.Select(i => $"{i.DbName} AS {i.Name}"));
            var insertFields = string.Join(", ", metadata.Where(i => !i.IsIdentity).Select(i => i.DbName));
            var insertValues = string.Join(", ", metadata.Where(i => !i.IsIdentity).Select(i => $"@{i.Name}"));
            var updatePairs = string.Join(", ", metadata.Where(i => !i.IsPrimaryKey).Select(i => $"{i.DbName} = @{i.Name}"));
            var whereCondition = string.Join(" AND ", metadata.Where(i => i.IsPrimaryKey).Select(i => $"{i.DbName} = @{i.Name}"));

            SelectAllSql = $"SELECT {selectFields} FROM {metadata.DbName}";
            SelectSql = SelectAllSql;
            if (whereCondition.Length > 0)
                SelectSql += $" WHERE {whereCondition}";

            InsertSql = $"INSERT INTO {metadata.DbName} ({insertFields}) VALUES ({insertValues})";
            if (IsIdentity)
            {
                InsertSql += Environment.NewLine;
                InsertSql += "SELECT SCOPE_IDENTITY()";
            }

            UpdateSql = $"UPDATE {metadata.DbName} SET {updatePairs}";
            if (whereCondition.Length > 0)
                UpdateSql += $" WHERE {whereCondition}";

            DeleteAllSql = $"DELETE FROM {metadata.DbName}";
            DeleteSql = DeleteAllSql;
            if (whereCondition.Length > 0)
                DeleteSql += $" WHERE {whereCondition}";

            CountSql = $"SELECT COUNT(1) FROM {metadata.DbName}";
        } 
    }
}
