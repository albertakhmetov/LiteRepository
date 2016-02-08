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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Sql
{
    public sealed class SqlBuilder<E> : ISqlBuilder
        where E : class
    {
        private readonly SqlMetadata _sqlMetadata;
        private readonly SqlExpression<E> _sqlExpression;
        private readonly string _insertSql, _updateSql, _deleteSql, _selectSql, _countSql;
        private readonly string _deleteAllSql, _deleteByKeySql, _selectByKeySql;

        public SqlBuilder()
        {
            _sqlMetadata = new SqlMetadata(typeof(E));
            _sqlExpression = new SqlExpression<E>();

            var selectFields = string.Join(", ", _sqlMetadata.Select(i => $"{i.DbName} AS {i.Name}"));
            var insertFields = string.Join(", ", _sqlMetadata.Where(i => !i.IsIdentity).Select(i => i.DbName));
            var insertValues = string.Join(", ", _sqlMetadata.Where(i => !i.IsIdentity).Select(i => $"@{i.Name}"));

            var whereCondition = string.Join(" AND ", _sqlMetadata.Where(i => i.IsPrimaryKey).Select(i => $"{i.DbName} = @{i.Name}"));

            var byKey = $" WHERE {whereCondition}";

            _insertSql = $"INSERT INTO {_sqlMetadata.DbName} ({insertFields}) VALUES ({insertValues})";
            if (_sqlMetadata.IsIdentity)
            {
                _insertSql += Environment.NewLine;
                _insertSql += "SELECT SCOPE_IDENTITY()";
            }

            _updateSql = $"UPDATE {_sqlMetadata.DbName} SET {GetUpdatePairs(_sqlMetadata.Where(i => !i.IsPrimaryKey))}{byKey}";

            _deleteSql = $"DELETE FROM {_sqlMetadata.DbName}";
            _deleteAllSql = $"TRUNCATE TABLE {_sqlMetadata.DbName}";
            _deleteByKeySql = _deleteSql + byKey;

            _selectSql = $"SELECT {selectFields} FROM {_sqlMetadata.DbName}";
            _selectByKeySql = _selectSql + byKey;

            _countSql = $"SELECT COUNT(1) FROM {_sqlMetadata.DbName}";
        }

        private string GetUpdatePairs(IEnumerable<SqlMetadata.Property> properties)
        {
            return string.Join(", ", properties.Select(i => $"{i.DbName} = @{i.Name}"));
        }

        public string GetInsertSql()
        {
            return _insertSql;
        }

        public string GetUpdateSql()
        {
            return _updateSql;
        }

        public string GetUpdateByExpressionSql(object parameters, Expression<Func<E, bool>> conditions = null)
        {
            var whereConditions = _sqlExpression.GetWherePartSql(conditions);
            var updatePairs = GetUpdatePairs(_sqlMetadata.GetSubsetForType(parameters.GetType()));

            if (whereConditions.Length > 0)
                return $"UPDATE {_sqlMetadata.DbName} SET {updatePairs} WHERE {whereConditions}";
            else
                return $"UPDATE {_sqlMetadata.DbName} SET {updatePairs}";
        }

        public string GetDeleteAllSql()
        {
            return _deleteAllSql;
        }

        public string GetDeleteByKeySql()
        {
            return _deleteByKeySql;
        }

        public string GetDeleteByExpressionSql(Expression<Func<E, bool>> conditions)
        {
            var whereConditions = _sqlExpression.GetWherePartSql(conditions);
            if (whereConditions.Length > 0)
                return $"{_deleteSql} WHERE {whereConditions}";
            else
                return _deleteAllSql;
        }

        public string GetSelectByKeySql()
        {
            return _selectByKeySql;
        }

        public string GetSelectByExpressionSql(Expression<Func<E, bool>> conditions, Expression<Func<IEnumerable<E>, IEnumerable<E>>> order = null)
        {
            var whereConditions = _sqlExpression.GetWherePartSql(conditions);
            var orderSql = _sqlExpression.GetOrderPartSql(order);

            return $"{_selectSql}{(whereConditions.Length > 0 ? " WHERE " : "")}{whereConditions}{(orderSql.Length > 0 ? " " : "")}{orderSql}";
        }


        public string GetCountSql()
        {
            return _countSql;
        }

        public string GetCountSql(Expression<Func<E, bool>> conditions = null)
        {
            var whereConditions = _sqlExpression.GetWherePartSql(conditions);
            if (whereConditions.Length > 0)
                return $"{_countSql} WHERE {whereConditions}";
            else
                return _countSql;
        }
    }
}
