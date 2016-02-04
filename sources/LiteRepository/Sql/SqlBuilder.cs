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
    public sealed class SqlBuilder<E> : ISqlBuilder
    {
        private readonly string _insertSql, _updateSql, _deleteSql, _selectSql, _countSql;
        private readonly string _deleteAllSql, _deleteByKeySql, _selectByKeySql;

        public SqlBuilder()
        {
            var metadata = new SqlMetadata(typeof(E));

            var selectFields = string.Join(", ", metadata.Select(i => $"{i.DbName.ToLower()} AS {i.Name}"));
            var insertFields = string.Join(", ", metadata.Where(i => !i.IsIdentity).Select(i => i.DbName.ToLower()));
            var insertValues = string.Join(", ", metadata.Where(i => !i.IsIdentity).Select(i => $"@{i.Name}"));
            var updatePairs = string.Join(", ", metadata.Where(i => !i.IsPrimaryKey).Select(i => $"{i.DbName.ToLower()} = @{i.Name}"));
            var whereCondition = string.Join(" AND ", metadata.Where(i => i.IsPrimaryKey).Select(i => $"{i.DbName.ToLower()} = @{i.Name}"));

            var byKey = $" WHERE {whereCondition}";

            _insertSql = $"INSERT INTO {metadata.DbName.ToLower()} ({insertFields}) VALUES ({insertValues})";
            if (metadata.IsIdentity)
            {
                _insertSql += Environment.NewLine;
                _insertSql += "SELECT SCOPE_IDENTITY()";
            }

            _updateSql = $"UPDATE {metadata.DbName.ToLower()} SET {updatePairs}{byKey}";

            _deleteSql = $"DELETE FROM {metadata.DbName.ToLower()}";
            _deleteAllSql = $"TRUNCATE TABLE {metadata.DbName.ToLower()}";
            _deleteByKeySql = _deleteSql + byKey;

            _selectSql = $"SELECT {selectFields} FROM {metadata.DbName.ToLower()}";
            _selectByKeySql = _selectSql + byKey;

            _countSql = $"SELECT COUNT(1) FROM {metadata.DbName.ToLower()}";
        }

        public string GetInsertSql()
        {
            return _insertSql;
        }

        public string GetUpdateSql()
        {
            return _updateSql;
        }

        public string GetUpdateByExpressionSql(object parameters, Func<E, bool> conditions)
        {
            return string.Empty;
        }

        public string GetDeleteAllSql()
        {
            return _deleteAllSql;
        }

        public string GetDeleteByKeySql()
        {
            return _deleteByKeySql;
        }

        public string GetDeleteByExpressionSql(Func<E, bool> conditions)
        {
            return string.Empty;
        }

        public string GetSelectByKeySql()
        {
            return _selectByKeySql;
        }

        public string GetSelectByExpressionSql(Func<E, bool> conditions, params SqlOrder[] orderByParams)
        {
            return string.Empty;
        }

        public string GetCountSql()
        {
            return _countSql;
        }

        public string GetCountByExpressionSql(Func<E, bool> conditions)
        {
            return string.Empty;
        }
    }
}
