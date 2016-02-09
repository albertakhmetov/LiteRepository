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
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Sql
{
    public sealed partial class SqlExpression<E>
        where E : class
    {
        public ISqlDialect Dialect
        {
            get; private set;
        }

        public SqlMetadata Metadata
        {
            get; private set;
        }

        public SqlExpression(ISqlDialect dialect)
        {
            Metadata = SqlMetadata.GetSqlMetadata(typeof(E));
            Dialect = dialect;
        }

        public string GetSelectSql(Type type = null, Expression<Func<E, bool>> where = null, Expression<Func<IEnumerable<E>, IEnumerable<E>>> orderBy = null)
        {
            var properties = type == null || type == typeof(E) ? Metadata : Metadata.GetSubsetForType(type);
            if (properties.Count() == 0)
                throw new InvalidOperationException("There are not fields to select");

            return Dialect.Select(Metadata.DbName, GetSelectPartSql(properties), GetWherePartSql(where), GetOrderPartSql(orderBy));
        }

        public string GetSelectByKeySql(Type type = null)
        {
            var properties = type == null || type == typeof(E) ? Metadata : Metadata.GetSubsetForType(type);
            if (properties.Count() == 0)
                throw new InvalidOperationException("There are not fields to select");

            return Dialect.Select(Metadata.DbName, GetSelectPartSql(properties), GetWhereByKeyPartSql(), string.Empty);
        }

        public string GetSelectScalarSql<T>(Expression<Func<IEnumerable<E>, T>> expression, Expression<Func<E, bool>> where = null)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return Dialect.SelectScalar(Metadata.DbName, GetSelectScalarPartSql(expression), GetWherePartSql(where));
        }

        public string GetInsertSql(Type type = null)
        {
            var properties = type == null || type == typeof(E) ? Metadata : Metadata.GetSubsetForType(type);
            if (properties.Count() == 0)
                throw new InvalidOperationException("There are not fields to insert");

            return Dialect.Insert(Metadata.DbName, GetInsertFieldsPartSql(properties), GetInsertValuesPartSql(properties));
        }

        public string GetUpdateSql(Type type = null, Expression<Func<E, bool>> where = null)
        {
            var properties = type == null || type == typeof(E) ? Metadata : Metadata.GetSubsetForType(type);
            properties = properties.Where(i => !i.IsPrimaryKey);
            if (properties.Count() == 0)
                throw new InvalidOperationException("There are not fields to update");

            return Dialect.Update(Metadata.DbName,
                GetUpdatePartSql(properties),
                where == null ? GetWhereByKeyPartSql() : GetWherePartSql(where));
        }

        public string GetDeleteSql(Expression<Func<E, bool>> where = null)
        {
            return Dialect.Delete(Metadata.DbName,
                where == null ? GetWhereByKeyPartSql() : GetWherePartSql(where));
        }

        private string GetSelectPartSql(IEnumerable<SqlMetadata.Property> properties)
        {
            return string.Join(", ", properties.Select(i => $"{i.DbName} AS {i.Name}"));
        }

        private string GetSelectScalarPartSql<T>(Expression<Func<IEnumerable<E>, T>> expression)
        {
            if (expression == null)
                return string.Empty;

            if (expression.Body is MethodCallExpression)
                return ProcessScalarMethodCall(expression.Body as MethodCallExpression);
            else
                throw new NotSupportedException();
        }

        private string GetInsertFieldsPartSql(IEnumerable<SqlMetadata.Property> properties)
        {
            return string.Join(", ", properties.Where(i => !i.IsIdentity).Select(i => i.DbName));
        }

        private string GetInsertValuesPartSql(IEnumerable<SqlMetadata.Property> properties)
        {
            return string.Join(", ", properties.Where(i => !i.IsIdentity).Select(i => Dialect.Parameter(i.Name)));
        }

        private string GetUpdatePartSql(IEnumerable<SqlMetadata.Property> properties)
        {
            return string.Join(", ", properties.Where(i => !i.IsPrimaryKey).Select(i => $"{i.DbName} = {Dialect.Parameter(i.Name)}"));
        }

        private string GetWherePartSql(Expression<Func<E, bool>> expression)
        {
            if (expression == null)
                return string.Empty;

            return Process(expression.Body);
        }

        private string GetWhereByKeyPartSql()
        {
            return string.Join(" AND ", Metadata.Where(i => i.IsPrimaryKey).Select(i => $"{i.DbName} = {Dialect.Parameter(i.Name)}"));
        }

        private string GetOrderPartSql(Expression<Func<IEnumerable<E>, IEnumerable<E>>> expression)
        {
            if (expression == null)
                return string.Empty;

            if (expression.Body is MethodCallExpression)
                return ProcessOrderMethodCall(expression.Body as MethodCallExpression);
            else
                throw new NotSupportedException();
        }     
    }
}
