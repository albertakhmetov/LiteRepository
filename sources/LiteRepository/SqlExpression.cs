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
    /// <summary>
    /// Provides functionality to convert expression to SQL.
    /// </summary>
    /// <typeparam name="E">Type of the entity.</typeparam>
    public sealed partial class SqlExpression<E>
        where E : class
    {
        /// <summary>
        /// Gets a <see cref="SqlDialect"/>.
        /// </summary>
        public SqlDialect Dialect
        {
            get; private set;
        }

        /// <summary>
        /// Gets a <see cref="Metadata"/>.
        /// </summary>
        public SqlMetadata Metadata
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlExpression{E}"/> class.
        /// </summary>
        /// <param name="dialect">A specified <see cref="SqlDialect"/>.</param>
        public SqlExpression(SqlDialect dialect)
        {
            Metadata = SqlMetadata.GetSqlMetadata(typeof(E));
            Dialect = dialect;
        }

        /// <summary>
        /// Creates a SELECT SQL based on the passed parameters.
        /// </summary>
        /// <param name="type">
        /// Type that contains subset of <typeparamref name="E"/> members.
        /// Used for generate fields list.</param>
        /// <param name="where">
        /// Where expression. You can use members of <typeparamref name="E"/> or <paramref name="param"/>. 
        /// Other values will be evaluated.</param>
        /// <param name="param">Query parameters.</param>
        /// <param name="orderBy">
        /// Sort expression. 
        /// You can use GroupBy and GroupByDescending methods with members from <typeparamref name="E"/>.</param>
        /// <returns>A string with a select query.</returns>
        /// <example>
        /// <para>Suppose we have a class:</para> 
        /// <code lang="c#">
        /// public class User {
        ///     [SqlKey]
        ///     public int Id { get; set; }
        ///     public string FirstName { get; set; }
        ///     public string SecondName { get; set; }
        ///     public DateTime Birthday { get; set; }
        ///     [SqlIgnore]
        ///     public string FullName { get { return FirstName + " " + SecondName; } }
        /// }
        /// </code>
        /// <para>Without any parameters method returns:</para>
        /// <code lang="c#">
        /// var sql = GetSelectSql(); 
        /// // select Id, FirstName, SecondName, Birthday from User;      
        /// </code>
        /// <para>If you need to select only parts of columns:</para>
        /// <code lang="c#">
        /// var p = new { FirstName = "", SecondName = "" };
        /// var sql = GetSelectSql(type:p.GetType()); 
        /// // select FirstName, SecondName from User;   
        /// </code>
        /// <para>If you need to filter values with hard-coded parameters:</para>       
        /// <code lang="c#">
        /// var sql = GetSelectSql(where: e => e.FirstName.StartsWith("A")); 
        /// // select Id, FirstName, SecondName, Birthday from User where FirstName like 'A%';     
        /// </code>
        /// <para>If you need to filter values with parameters:</para>
        /// <code lang="c#">
        /// var parameters = { FirstLetter = "A" };
        /// var sql = GetSelectSql(where: e => e.FirstName.StartsWith(parameters.FirstLetter), param: parameters); 
        /// // select Id, FirstName, SecondName, Birthday from User where FirstName like @FirstLetter%';     
        /// </code>
        /// <para>If you need sort the results:</para>       
        /// <code lang="c#">
        /// var sql = GetSelectSql(l => l.OrderBy(x => x.FirstName).OrderByDescending(x => x.Id)); 
        /// // select Id, FirstName, SecondName, Birthday from User order by FirstName, Id desc;  
        /// </code>
        /// <para>You can combine settings to achieve the desired result. If any option will be skipped - 
        /// SQL will be generated without it.</para>
        /// </example>
        public string GetSelectSql(Type type = null, Expression<Func<E, bool>> where = null, object param = null, Expression<Func<IEnumerable<E>, IEnumerable<E>>> orderBy = null)
        {
            var properties = type == null || type == typeof(E) ? Metadata : Metadata.GetSubsetForType(type);
            if (properties.Count() == 0)
                throw new InvalidOperationException("There are no fields to select");

            return Dialect.Select(
                Metadata.DbName,
                GetSelectPartSql(properties),
                GetWherePartSql(where, new Parameters(param)),
                GetOrderPartSql(orderBy));
        }

        /// <summary>
        /// Creates a SELECT SQL for single entity based on the passed parameters.
        /// </summary>
        /// <param name="type">
        /// Type that contains subset of <typeparamref name="E"/> members. Used for generate fields list.</param>
        /// <returns>A string with a select query (parameters are the same like Primary Key).</returns>
        /// <example>
        /// <para>Suppose we have a class:</para>
        /// <code lang="c#">
        /// public class User {
        ///     [SqlKey]
        ///     public int Id { get; set; }
        ///     public string FirstName { get; set; }
        ///     public string SecondName { get; set; }
        ///     public DateTime Birthday { get; set; }
        ///     [SqlIgnore]
        ///     public string FullName { get { return FirstName + " " + SecondName; } }
        /// }
        /// </code>
        /// <para>If you don't passed any parameter into method, it returns default update SQL:</para>
        /// <code lang="c#">
        /// var sql = GetSelectByKeySql();
        /// // select * from User where Id = @Id;
        /// </code>
        /// <para>Passing <paramref name="type"/> you sets subset of columns which you want to update:</para>
        /// <code lang="c#">
        /// var p = new { FirstName = "", SecondName = "" };
        /// var sql = GetSelectByKeySql(type:p.GetType()); 
        /// // select FirstName, SecondName from User where Id = @Id;
        /// </code>
        /// </example>
        public string GetSelectByKeySql(Type type = null)
        {
            var properties = type == null || type == typeof(E) ? Metadata : Metadata.GetSubsetForType(type);
            if (properties.Count() == 0)
                throw new InvalidOperationException("There are no fields to select");

            return Dialect.Select(Metadata.DbName, GetSelectPartSql(properties), GetWhereByKeyPartSql(), string.Empty);
        }

        /// <summary>
        /// Creates a scalar SELECT SQL based on the passed parameters.
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="expression">Scalar expression</param>
        /// <param name="where">
        /// Where expression. You can use members of <typeparamref name="E"/> or <paramref name="param"/>. 
        /// Other values will be evaluated.</param>
        /// <param name="param">Query parameters</param>
        /// <returns>A string with a select scalar query.</returns>
        /// <example>
        /// <para>You need to pass <paramref name="expression"/> for retrive scalar data form database:</para>
        /// <code lang="c#">
        /// var sql = GetSelectScalarSql{T}(l => l.Average(e => e.Price));
        /// // select avg(Price) from Table;
        /// </code>
        /// <para><typeparamref name="T"/> is the return type for expression.
        /// Where expression used like in <see cref="GetSelectSql">GetSelectSql</see>.</para>
        /// </example>
        public string GetSelectScalarSql<T>(Expression<Func<IEnumerable<E>, T>> expression, Expression<Func<E, bool>> where = null, object param = null)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return Dialect.SelectScalar(
                Metadata.DbName,
                GetSelectScalarPartSql(expression),
                GetWherePartSql(where, new Parameters(param)));
        }

        /// <summary>
        /// Creates a INSERT SQL based on the passed parameters.
        /// </summary>
        /// <param name="type">
        /// Type that contains subset of <typeparamref name="E"/> members. 
        /// Used for generate fields and values lists.</param>
        /// <returns>A string with a insert query.</returns>
        /// <example>
        /// <para>Suppose we have a class:</para>
        /// <code lang="c#">
        /// public class User {
        ///     [SqlKey]
        ///     public int Id { get; set; }
        ///     public string FirstName { get; set; }
        ///     public string SecondName { get; set; }
        ///     public DateTime Birthday { get; set; }
        ///     [SqlIgnore]
        ///     public string FullName { get { return FirstName + " " + SecondName; } }
        /// }
        /// </code>
        /// <para>If you don't passed any parameter into method, it returns default insert SQL:</para>
        /// <code lang="c#">
        /// var sql = GetInsertSql();
        /// // insert into User (Id, FirstName, SecondName, Birthday) values (@Id, @FirstName, @SecondName, @Birthday);
        /// </code>
        /// <para>Passing <paramref name="type"/> you sets subset of columns which you want to insert:</para>
        /// <code lang="c#">
        /// var p = new { Id = 0, FirstName = "", SecondName = "" };
        /// var sql = GetInsertSql(type:p.GetType()); 
        /// // insert into User (Id, FirstName, SecondName) values (@Id, @FirstName, @SecondName);
        /// </code>
        /// <para>If <typeparamref name="E"/> is identity entity, then method returns special SQL:</para>
        /// <code lang="SQL">
        /// insert into User (FirstName, SecondName, Birthday) values (@FirstName, @SecondName, @Birthday);
        /// select scope_identity();
        /// </code>
        /// </example>        
        public string GetInsertSql(Type type = null)
        {
            var properties = type == null || type == typeof(E) ? Metadata : Metadata.GetSubsetForType(type);
            if (properties.Count() == 0)
                throw new InvalidOperationException("There are no fields to insert");

            return Dialect.Insert(Metadata.DbName, GetInsertFieldsPartSql(properties), GetInsertValuesPartSql(properties), Metadata.IsIdentity);
        }

        /// <summary>
        /// Creates a UPDATE SQL based on the passed parameters.
        /// </summary>
        /// <param name="type">
        /// Type that contains subset of <typeparamref name="E"/> members.
        /// Used for generate fields list</param>
        /// <param name="where">
        /// Where expression. You can use members of <typeparamref name="E"/>. 
        /// Other values will be evaluated.</param>
        /// <returns>A string with a update query.</returns>
        /// <example>
        /// <para>Suppose we have a class:</para>
        /// <code lang="c#">
        /// public class User {
        ///     [SqlKey]
        ///     public int Id { get; set; }
        ///     public string FirstName { get; set; }
        ///     public string SecondName { get; set; }
        ///     public DateTime Birthday { get; set; }
        ///     [SqlIgnore]
        ///     public string FullName { get { return FirstName + " " + SecondName; } }
        /// }
        /// </code>
        /// <para>If you don't passed any parameter into method, it returns default update SQL:</para>
        /// <code lang="c#">
        /// var sql = GetUpdateSql();
        /// // update User set FirstName = @FirstName, SecondName = @SecondName, Birthday = @Birthday where Id = @Id;
        /// </code>
        /// <para>Passing <paramref name="type"/> you sets subset of columns which you want to update:</para>
        /// <code lang="c#">
        /// var p = new { FirstName = "", SecondName = "" };
        /// var sql = GetUpdateSql(type:p.GetType()); 
        /// // update User set FirstName = @FirstName, SecondName = @SecondName where Id = @Id;
        /// </code>
        /// <para>Where expression used like in <see cref="GetSelectSql">GetSelectSql</see>, 
        /// but you can't pass parameters object.</para>        
        /// </example>
        public string GetUpdateSql(Type type = null, Expression<Func<E, bool>> where = null)
        {
            var properties = type == null || type == typeof(E) ? Metadata : Metadata.GetSubsetForType(type);
            properties = properties.Where(i => !i.IsPrimaryKey);
            if (properties.Count() == 0)
                throw new InvalidOperationException("There are no fields to update");

            var whereSql = where == null
                ? GetWhereByKeyPartSql()
                : GetWherePartSql(where, Parameters.Empty);

            return Dialect.Update(Metadata.DbName, GetUpdatePartSql(properties), whereSql);
        }

        /// <summary>
        /// Creates a DELETE SQL based on the passed parameters.
        /// </summary>
        /// <param name="where">
        /// Where expression. You can use members of <typeparamref name="E"/> or <paramref name="param"/>.
        /// Other values will be evaluated.</param>
        /// <param name="param">Query parameters.</param>
        /// <returns>A string with a delete query.</returns>
        /// <example>
        /// <para>Where expression used like in <see cref="GetSelectSql">GetSelectSql</see>.
        /// But if you don't pass where expression, method returns SQL
        /// with where condition to delete item by key.</para>
        /// <para>Suppose we have a class:</para>
        /// <code lang="c#">
        /// public class User {
        ///     [SqlKey]
        ///     public int Id { get; set; }
        ///     public string FirstName { get; set; }
        ///     public string SecondName { get; set; }
        ///     public DateTime Birthday { get; set; }
        ///     [SqlIgnore]
        ///     public string FullName { get { return FirstName + " " + SecondName; } }
        /// }
        /// </code>
        /// <para>Then you call method without parameters:</para>
        /// <code lang="c#">
        /// var sql = GetDeleteSql();
        /// // delete from User where Id=@Id;
        /// </code>
        /// </example>
        public string GetDeleteSql(Expression<Func<E, bool>> where = null, object param = null)
        {
            return Dialect.Delete(Metadata.DbName, where == null
                ? GetWhereByKeyPartSql()
                : GetWherePartSql(where, new Parameters(param)));
        }

        /// <summary>
        /// Creates a TRUNCATE SQL.
        /// </summary>
        /// <returns>A string with a truncate query.</returns>
        public string GetTruncateSql()
        {
            return Dialect.Delete(Metadata.DbName, string.Empty);
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
                return ProcessScalarMethodCall(expression.Body as MethodCallExpression, Parameters.Empty);
            else
                throw new NotSupportedException($"Expression '{expression.Body.GetType()}' in Scalar is not supported");
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

        private string GetWherePartSql(Expression<Func<E, bool>> expression, Parameters parameters)
        {
            if (expression == null)
                return string.Empty;

            return Process(expression.Body, parameters);
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
                return ProcessOrderMethodCall(expression.Body as MethodCallExpression, Parameters.Empty);
            else
                throw new NotSupportedException($"Expression '{expression.Body.GetType()}' in OrderBy is not supported");
        }
    }
}
