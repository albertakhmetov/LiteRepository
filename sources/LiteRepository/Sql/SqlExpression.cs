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
using LiteRepository.Common.Extensions;

namespace LiteRepository.Sql
{
    public sealed class SqlExpression<E>
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

        [Obsolete]
        public SqlExpression()
        {
            Metadata = SqlMetadata.GetSqlMetadata(typeof(E));
        }

        public SqlExpression(ISqlDialect dialect)
        {
            Metadata = SqlMetadata.GetSqlMetadata(typeof(E));
            Dialect = dialect;
        }

        public string GetSelectSql(Type type = null, Expression<Func<E, bool>> where = null, Expression<Func<IEnumerable<E>, IEnumerable<E>>> orderBy = null)
        {
            return Dialect.Select(Metadata.DbName, GetSelectPartSql(type), GetWherePartSql(where), GetOrderPartSql(orderBy));
        }

        public string GetSelectByKeySql(Type type = null)
        {
            return Dialect.Select(Metadata.DbName, GetSelectPartSql(type), GetWhereByKeyPartSql(), string.Empty);
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
            return string.Empty; // UPDATE metadata.DbName SET [part][WHERE]
        }

        public string GetUpdateByKeySql(Type type = null)
        {
            return string.Empty;
        }

        public string GetDeleteSql(Expression<Func<E, bool>> where = null)
        {
            return string.Empty; // DELETE FROM metadata.DbName[WHERE] -- TRUNCATE TABLE metadata.DbName
        }

        public string GetDeleteByKeySql()
        {
            return string.Empty;
        }

        public string GetSelectPartSql(Type type = null)
        {
            var properties = type == null || type == typeof(E) ? Metadata : Metadata.GetSubsetForType(type);
            return string.Join(", ", properties.Select(i => $"{i.DbName} AS {i.Name}"));
        }

        public string GetSelectScalarPartSql<T>(Expression<Func<IEnumerable<E>, T>> expression)
        {
            if (expression == null)
                return string.Empty;

            if (expression.Body is MethodCallExpression)
                return ProcessScalarMethodCall(expression.Body as MethodCallExpression);
            else
                throw new NotSupportedException();
        }

        public string GetInsertFieldsPartSql(IEnumerable<SqlMetadata.Property> properties)
        {
            return string.Join(", ", properties.Where(i => !i.IsIdentity).Select(i => i.DbName));
        }

        public string GetInsertValuesPartSql(IEnumerable<SqlMetadata.Property> properties)
        {
            return string.Join(", ", properties.Where(i => !i.IsIdentity).Select(i => Dialect.Parameter(i.Name)));
        }

        public string GetUpdatePartSql(Type type = null)
        {
            return string.Empty;
        }

        public string GetWherePartSql(Expression<Func<E, bool>> expression)
        {
            if (expression == null)
                return string.Empty;

            return Process(expression.Body);
        }

        public string GetWhereByKeyPartSql()
        {
            return string.Join(" AND ", Metadata.Where(i => i.IsPrimaryKey).Select(i => $"{i.DbName} = {Dialect.Parameter(i.Name)}"));
        }

        public string GetOrderPartSql(Expression<Func<IEnumerable<E>, IEnumerable<E>>> expression)
        {
            if (expression == null)
                return string.Empty;

            if (expression.Body is MethodCallExpression)
                return ProcessOrderMethodCall(expression.Body as MethodCallExpression);
            else
                throw new NotSupportedException();
        }

        private string NodeType(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";

                default:
                    throw new NotSupportedException();
            }
        }

        private string ToString(object value)
        {
            if (value == null)
                return string.Empty;
            else if (value is DateTime)
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            else if (value is double || value is decimal)
                return string.Format(CultureInfo.InvariantCulture, "{0:g}", value);
            else
                return value.ToString();
        }

        private string Process(Expression expression)
        {
            if (expression is BinaryExpression)
                return ProcessBinary(expression as BinaryExpression);
            else if (expression is UnaryExpression)
                return ProcessUnary(expression as UnaryExpression);
            else if (expression is MemberExpression)
                return ProcessMember(expression as MemberExpression);
            else if (expression is ConstantExpression)
                return ProcessConstant(expression as ConstantExpression);
            else if (expression is MethodCallExpression)
                return ProcessMethodCall(expression as MethodCallExpression);
            else if (expression is NewExpression)
                return ProcessNew(expression as NewExpression);

            throw new NotSupportedException();
        }

        private string ProcessBinary(BinaryExpression expression, BinaryExpression parentExpression = null)
        {
            if (expression.Left.NodeType == ExpressionType.Convert)
            {
                var mEx = (expression.Left as UnaryExpression)?.Operand as MemberExpression;
                var cEx = expression.Right as ConstantExpression;

                if (mEx != null && mEx.Type == typeof(char) && cEx != null)
                    return $"{ProcessMember(mEx)} {NodeType(expression)} '{Convert.ToChar(cEx.Value)}'";
            }
            else if (expression.Right.NodeType == ExpressionType.Convert)
            {
                var mEx = (expression.Right as UnaryExpression)?.Operand as MemberExpression;
                var cEx = expression.Left as ConstantExpression;

                if (mEx != null && mEx.Type == typeof(char) && cEx != null)
                    return $"'{Convert.ToChar(cEx.Value)}' {NodeType(expression)} {ProcessMember(mEx)}";
            }

            var left = expression.Left is BinaryExpression
                ? ProcessBinary(expression.Left as BinaryExpression, expression)
                : Process(expression.Left);
            var right = expression.Right is BinaryExpression
                ? ProcessBinary(expression.Right as BinaryExpression, expression)
                : Process(expression.Right);

            var result = $"{left} {NodeType(expression)} {right}";
            if (parentExpression != null
                && expression.NodeType.In(ExpressionType.AndAlso, ExpressionType.OrElse)
                && expression.NodeType != parentExpression.NodeType)
                return $"({result})";
            else
                return result;
        }

        private string ProcessUnary(UnaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    return "NOT " + Process(expression.Operand);

                default:
                    return Process(expression.Operand);
            }
        }

        private string ProcessMember(MemberExpression expression)
        {
            if (expression.Member.ReflectedType == typeof(E))
                return Metadata[expression.Member.Name]; // this is db field
            else
                return $"@{expression.Member.Name}"; // this is property of the parameter's object
        }

        private string ProcessConstant(ConstantExpression expression)
        {
            if (expression.Type == typeof(string))
                return $"'{expression.Value}'";
            else
                return ToString(expression.Value);
        }

        private string ProcessMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType == typeof(string))
            {
                var member = Process(expression.Object);
                switch (expression.Method.Name)
                {
                    case nameof(string.StartsWith):
                        return $"{member} like {ProcessStringMethodArg(expression, postfix: "%")}";
                    case nameof(string.EndsWith):
                        return $"{member} like {ProcessStringMethodArg(expression, prefix: "%")}";
                    case nameof(string.Contains):
                        return $"{member} like {ProcessStringMethodArg(expression, prefix: "%", postfix: "%")}";

                    case nameof(string.ToLower):
                        return $"lower({member})";

                    case nameof(string.ToUpper):
                        return $"upper({member})";
                }
            }

            throw new NotSupportedException();
        }

        private string ProcessScalarMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType == typeof(Enumerable))
            {
                switch (expression.Method.Name)
                {
                    case nameof(Enumerable.Count):
                        return "COUNT(1)";
                    case nameof(Enumerable.Average):
                        if (expression.Arguments.LastOrDefault() is LambdaExpression == false)
                            throw new NotSupportedException();
                        return $"AVG({ProcessLambda(expression.Arguments.LastOrDefault() as LambdaExpression)})";
                    case nameof(Enumerable.Sum):
                        if (expression.Arguments.LastOrDefault() is LambdaExpression == false)
                            throw new NotSupportedException();
                        return $"SUM({ProcessLambda(expression.Arguments.LastOrDefault() as LambdaExpression)})";
                }
            }

            throw new NotSupportedException();
        }

        private string ProcessOrderMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType == typeof(Enumerable))
            {
                if (expression.Method.Name.In(nameof(Enumerable.OrderBy), nameof(Enumerable.OrderByDescending)))
                {
                    if (expression.Arguments.LastOrDefault() is LambdaExpression == false)
                        throw new NotSupportedException();
                    var dbName = ProcessLambda(expression.Arguments.LastOrDefault() as LambdaExpression);
                    if (expression.Method.Name == nameof(Enumerable.OrderByDescending))
                        dbName += " DESC";

                    var innerSql = string.Empty;
                    if (expression.Arguments.FirstOrDefault() is MethodCallExpression)
                        innerSql = ProcessOrderMethodCall(expression.Arguments.FirstOrDefault() as MethodCallExpression);
                    else if (expression.Arguments.FirstOrDefault() is ParameterExpression == false)
                        throw new NotSupportedException();

                    if (innerSql.Length > 0)
                        return $"{innerSql}, {dbName}";
                    else
                        return dbName;
                }
            }

            throw new NotSupportedException();
        }

        private string ProcessStringMethodArg(MethodCallExpression expression, string prefix = null, string postfix = null)
        {
            var arg = expression.Arguments[0];

            if (arg is ConstantExpression)
                return $"'{prefix}{(arg as ConstantExpression).Value}{postfix}'";
            else if (arg is MemberExpression)
                return ProcessMember(arg as MemberExpression);
            else
                throw new NotSupportedException();
        }

        private string ProcessNew(NewExpression expression)
        {
            if (expression.Type != typeof(DateTime))
                throw new NotSupportedException();

            var lambda = Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object)));
            var obj = lambda.Compile()();


            return $"'{ToString(obj)}'";
        }

        private string ProcessLambda(LambdaExpression expression)
        {
            return Process(expression.Body);
        }
    }
}
