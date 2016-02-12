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

namespace LiteRepository
{
    partial class SqlExpression<E>
        where E : class
    {
        private sealed class Parameters
        {
            public static readonly Parameters Empty = new Parameters(null);

            public object Value
            {
                get; private set;
            }

            public Type Type
            {
                get; private set;
            }

            public bool IsEmpty
            {
                get; private set;
            }

            public Parameters(object value)
            {
                IsEmpty = value == null;
                Type = value?.GetType();
                Value = value;
            }
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
            var str = string.Empty;

            if (value is DateTime)
                str = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            else if (value is double || value is decimal)
                str = string.Format(CultureInfo.InvariantCulture, "{0:g}", value);
            else if(value != null)
                str = value.ToString();

            if (value is DateTime || value is string || value is char)
                return $"'{str}'";
            else
                return str;
        }

        private string CompileAndExecute(Expression expression)
        {
            var lambda = Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object)));
            var obj = lambda.Compile()();
            return ToString(obj);
        }

        private string Process(Expression expression, Parameters parameters)
        {
            if (expression is BinaryExpression)
                return ProcessBinary(expression as BinaryExpression, null, parameters);
            else if (expression is UnaryExpression)
                return ProcessUnary(expression as UnaryExpression, parameters);
            else if (expression is MemberExpression)
                return ProcessMember(expression as MemberExpression, parameters);
            else if (expression is ConstantExpression)
                return ProcessConstant(expression as ConstantExpression, parameters);
            else if (expression is MethodCallExpression)
                return ProcessMethodCall(expression as MethodCallExpression, parameters);
            else if (expression is NewExpression)
                return ProcessNew(expression as NewExpression, parameters);

            throw new NotSupportedException();
        }

        private string ProcessBinary(BinaryExpression expression, BinaryExpression parentExpression, Parameters parameters)
        {
            if (expression.Left.NodeType == ExpressionType.Convert)
            {
                var mEx = (expression.Left as UnaryExpression)?.Operand as MemberExpression;
                var cEx = expression.Right as ConstantExpression;

                if (mEx != null && mEx.Type == typeof(char) && cEx != null)
                    return $"{ProcessMember(mEx, parameters)} {NodeType(expression)} '{Convert.ToChar(cEx.Value)}'";
            }
            else if (expression.Right.NodeType == ExpressionType.Convert)
            {
                var mEx = (expression.Right as UnaryExpression)?.Operand as MemberExpression;
                var cEx = expression.Left as ConstantExpression;

                if (mEx != null && mEx.Type == typeof(char) && cEx != null)
                    return $"'{Convert.ToChar(cEx.Value)}' {NodeType(expression)} {ProcessMember(mEx, parameters)}";
            }

            var left = expression.Left is BinaryExpression
                ? ProcessBinary(expression.Left as BinaryExpression, expression, parameters)
                : Process(expression.Left, parameters);
            var right = expression.Right is BinaryExpression
                ? ProcessBinary(expression.Right as BinaryExpression, expression, parameters)
                : Process(expression.Right, parameters);

            var result = $"{left} {NodeType(expression)} {right}";
            if (parentExpression != null
                && expression.NodeType.In(ExpressionType.AndAlso, ExpressionType.OrElse)
                && expression.NodeType != parentExpression.NodeType)
                return $"({result})";
            else
                return result;
        }

        private string ProcessUnary(UnaryExpression expression, Parameters parameters)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    return "NOT " + Process(expression.Operand, parameters);

                default:
                    return Process(expression.Operand, parameters);
            }
        }

        private string ProcessMember(MemberExpression expression, Parameters parameters)
        {
            if (typeof(E) == expression.Member.ReflectedType || typeof(E).IsSubclassOf(expression.Member.ReflectedType))
                return Metadata[expression.Member.Name]; // this is db field
            else if (!parameters.IsEmpty && parameters.Type == expression.Member.ReflectedType)
                return Dialect.Parameter(expression.Member.Name); // this is property of the parameter's object
            else
                return CompileAndExecute(expression);
        }

        private string ProcessConstant(ConstantExpression expression, Parameters parameters)
        {
            return ToString(expression.Value);
        }

        private string ProcessMethodCall(MethodCallExpression expression, Parameters parameters)
        {
            if (expression.Method.DeclaringType == typeof(string))
            {
                var member = Process(expression.Object, parameters);
                switch (expression.Method.Name)
                {
                    case nameof(string.StartsWith):
                        return $"{member} like {ProcessStringMethodArg(expression, parameters, postfix: "%")}";
                    case nameof(string.EndsWith):
                        return $"{member} like {ProcessStringMethodArg(expression, parameters, prefix: "%")}";
                    case nameof(string.Contains):
                        return $"{member} like {ProcessStringMethodArg(expression, parameters, prefix: "%", postfix: "%")}";

                    case nameof(string.ToLower):
                        return $"lower({member})";

                    case nameof(string.ToUpper):
                        return $"upper({member})";
                }
            }

            return CompileAndExecute(expression);
        }

        private string ProcessScalarMethodCall(MethodCallExpression expression, Parameters parameters)
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
                        return $"AVG({ProcessLambda(expression.Arguments.LastOrDefault() as LambdaExpression, parameters)})";
                    case nameof(Enumerable.Sum):
                        if (expression.Arguments.LastOrDefault() is LambdaExpression == false)
                            throw new NotSupportedException();
                        return $"SUM({ProcessLambda(expression.Arguments.LastOrDefault() as LambdaExpression, parameters)})";
                }
            }

            throw new NotSupportedException();
        }

        private string ProcessOrderMethodCall(MethodCallExpression expression, Parameters parameters)
        {
            if (expression.Method.DeclaringType == typeof(Enumerable))
            {
                if (expression.Method.Name.In(nameof(Enumerable.OrderBy), nameof(Enumerable.OrderByDescending)))
                {
                    if (expression.Arguments.LastOrDefault() is LambdaExpression == false)
                        throw new NotSupportedException();
                    var dbName = ProcessLambda(expression.Arguments.LastOrDefault() as LambdaExpression, parameters);
                    if (expression.Method.Name == nameof(Enumerable.OrderByDescending))
                        dbName += " DESC";

                    var innerSql = string.Empty;
                    if (expression.Arguments.FirstOrDefault() is MethodCallExpression)
                        innerSql = ProcessOrderMethodCall(expression.Arguments.FirstOrDefault() as MethodCallExpression, parameters);
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

        private string ProcessStringMethodArg(MethodCallExpression expression, Parameters parameters, string prefix = null, string postfix = null)
        {
            var arg = expression.Arguments[0];

            if (arg is ConstantExpression)
                return $"'{prefix}{(arg as ConstantExpression).Value}{postfix}'";
            else if (arg is MemberExpression)
                return ProcessMember(arg as MemberExpression, parameters);
            else
                throw new NotSupportedException();
        }

        private string ProcessNew(NewExpression expression, Parameters parameters)
        {
            if (expression.Type != typeof(DateTime))
                throw new NotSupportedException();

            return CompileAndExecute(expression);
        }

        private string ProcessLambda(LambdaExpression expression, Parameters parameters)
        {
            return Process(expression.Body, parameters);
        }
    }
}
