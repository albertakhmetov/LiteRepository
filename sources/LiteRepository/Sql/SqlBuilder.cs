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

namespace LiteRepository.Sql
{
    public class SqlBuilder<E> : ISqlBuilder
    {

        private readonly string _insertSql, _updateSql, _deleteSql, _selectSql, _selectCountSql;

        public SqlBuilder()
        {
            var metadata = new SqlMetadata(typeof(E));

            var selectFields = string.Join(", ", metadata.Select(i => $"{i.DbName.ToLower()} AS {i.Name}"));
            var insertFields = string.Join(", ", metadata.Where(i => !i.IsIdentity).Select(i => i.DbName.ToLower()));
            var insertValues = string.Join(", ", metadata.Where(i => !i.IsIdentity).Select(i => $"@{i.Name}"));
            var updatePairs = string.Join(", ", metadata.Where(i => !i.IsPrimaryKey).Select(i => $"{i.DbName.ToLower()} = @{i.Name}"));
            var whereCondition = string.Join(" AND ", metadata.Where(i => i.IsPrimaryKey).Select(i => $"{i.DbName.ToLower()} = @{i.Name}"));

            _selectSql = $"SELECT {selectFields} FROM {metadata.DbName.ToLower()}";
            if (whereCondition.Length > 0)
                _selectSql += $" WHERE {whereCondition}";

            _insertSql = $"INSERT INTO {metadata.DbName.ToLower()} ({insertFields}) VALUES ({insertValues})";
            if (metadata.IsIdentity)
            {
                _insertSql += Environment.NewLine;
                _insertSql += "SELECT SCOPE_IDENTITY()";
            }

            _updateSql = $"UPDATE {metadata.DbName.ToLower()} SET {updatePairs}";
            if (whereCondition.Length > 0)
                _updateSql += $" WHERE {whereCondition}";

            _deleteSql = $"DELETE FROM {metadata.DbName.ToLower()}";
            if (whereCondition.Length > 0)
                _deleteSql += $" WHERE {whereCondition}";

            _selectCountSql = $"SELECT COUNT(1) FROM {metadata.DbName.ToLower()}";
        }

        public string GetInsertSql()
        {
            return _insertSql;
        }
        
        public string GetUpdateSql()
        {
            return _updateSql;
        }

        public string GetDeleteSql()
        {
            return _deleteSql;
        }

        public string GetSelectSql()
        {
            return _selectSql;
        }

        public string GetSelectCountSql()
        {
            return _selectCountSql;
        }
    }
}
