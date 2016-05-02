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
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace LiteRepository
{
    /// <summary>
    /// Provides functional to execute queries for database.
    /// </summary>
    public class Db
    {
        private readonly SqlDialect _sqlDialect;
        private readonly DbConnection _dbConnection;
        private readonly Func<DbConnection> _dbConnectionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Db"/> class.
        /// </summary>
        /// <param name="sqlDialect">The <see cref="SqlDialect"/> instance.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> instance.</param>
        public Db(SqlDialect sqlDialect, DbConnection dbConnection)
        {
            if (sqlDialect == null)
                throw new ArgumentNullException(nameof(sqlDialect));
            if (dbConnection == null)
                throw new ArgumentNullException(nameof(dbConnection));

            _sqlDialect = sqlDialect;
            _dbConnection = dbConnection;
            _dbConnectionFactory = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Db"/> class.
        /// </summary>   
        /// <param name="sqlDialect">The <see cref="SqlDialect"/> instance.</param>
        /// <param name="dbConnectionFactory">
        /// The callback function for creating instances of <see cref="DbConnection"/>.
        /// </param>
        public Db(SqlDialect sqlDialect, Func<DbConnection> dbConnectionFactory)
        {
            if (sqlDialect == null)
                throw new ArgumentNullException(nameof(sqlDialect));
            if (dbConnectionFactory == null)
                throw new ArgumentNullException(nameof(dbConnectionFactory));

            _sqlDialect = sqlDialect;
            _dbConnection = null;
            _dbConnectionFactory = dbConnectionFactory;
        }

        /// <summary>
        /// Executes any <paramref name="action"/> method in database transaction.
        /// </summary>
        /// <typeparam name="T">Type of the execution result.</typeparam>
        /// <param name="action">Callback method to execute.</param>
        /// <returns>Result of the <paramref name="action"/> execute.</returns>
        public virtual T Exec<T>(Func<DbConnection, T> action)
        {
            var isOpened = _dbConnection?.State == System.Data.ConnectionState.Open;
            var dbConnection = _dbConnection;

            try
            {
                if (dbConnection == null)
                    dbConnection = _dbConnectionFactory();
                if (!isOpened)
                    dbConnection.Open();

                return action(dbConnection);
            }
            finally
            {
                if (!isOpened)
                    dbConnection.Close();
            }
        }

        /// <summary>
        /// An asynchronous version of <see cref="Exec{T}">Exec</see>.
        /// </summary>
        /// <typeparam name="T">Type of the execution result.</typeparam>
        /// <param name="action">Callback method to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task<T> ExecAsync<T>(Func<DbConnection, Task<T>> action)
        {
            var isOpened = _dbConnection?.State == System.Data.ConnectionState.Open;
            var dbConnection = _dbConnection;

            try
            {
                if (dbConnection == null)
                    dbConnection = _dbConnectionFactory();
                if (!isOpened)
                    await dbConnection.OpenAsync();

                return await action(dbConnection);
            }
            finally
            {
                if (!isOpened)
                    dbConnection.Close();
            }
        }

        private SqlExpression<E> GetSqlExpression<E>() where E : class
        {
            return new SqlExpression<E>(_sqlDialect);
        }

        /// <summary>
        /// Inserts <paramref name="entity"/> to the database.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <param name="entity">Entity to insert into the database.</param>
        /// <returns>
        /// If entity is identity - returns a new entity with a new Id. 
        /// Otherwise it returns original <paramref name="entity"/>.</returns>
        public virtual E Insert<E>(E entity) where E : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var sqlExpression = GetSqlExpression<E>();
            return Exec(dbConnection =>
            {
                if (entity is IIdentityEntity)
                {
                    var id = dbConnection.ExecuteScalar<long>(sqlExpression.GetInsertSql(), entity);
                    return (E)(entity as IIdentityEntity).UpdateId(id);
                }
                else
                {
                    dbConnection.Execute(sqlExpression.GetInsertSql(), entity);
                    return entity;
                }
            });
        }

        /// <summary>
        /// An asynchronous version of <see cref="Insert{E}">Insert</see>.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <param name="entity">Entity to insert into the database.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<E> InsertAsync<E>(E entity) where E : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var sqlExpression = GetSqlExpression<E>();
            return ExecAsync(async dbConnection =>
            {
                if (entity is IIdentityEntity)
                {
                    var id = await dbConnection.ExecuteScalarAsync<long>(sqlExpression.GetInsertSql(), entity);
                    return (E)(entity as IIdentityEntity).UpdateId(id);
                }
                else
                {
                    await dbConnection.ExecuteAsync(sqlExpression.GetInsertSql(), entity);
                    return entity;
                }
            });
        }

        /// <summary>
        /// Updates <paramref name="entity"/> in the database.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <param name="entity">Entity to update in database.</param>
        /// <returns>Count of a effected enities.</returns>
        public virtual int Update<E>(E entity) where E : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var sqlExpression = GetSqlExpression<E>();
            return Exec(dbConnection => dbConnection.Execute(sqlExpression.GetUpdateSql(), entity));
        }

        /// <summary>
        /// An asynchronous version of <see cref="Update{E}(E)">Update</see>.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <param name="entity">Entity to update in the database.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<int> UpdateAsync<E>(E entity) where E : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var sqlExpression = GetSqlExpression<E>();
            return ExecAsync(dbConnection => dbConnection.ExecuteAsync(sqlExpression.GetUpdateSql(), entity));
        }

        /// <summary>
        /// Updates subset <paramref name="subEntity"/> of the entity in the database.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <param name="subEntity">Entity to update in database.</param>
        /// <param name="where">Where expression.</param>
        /// <returns>Count of a effected enities.</returns>
        public virtual int Update<E>(object subEntity, Expression<Func<E, bool>> where) where E : class
        {
            if (subEntity == null)
                throw new ArgumentNullException(nameof(subEntity));
            if (where == null)
                throw new ArgumentNullException(nameof(where));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetUpdateSql(subEntity.GetType(), where);
            return Exec(dbConnection => dbConnection.Execute(sql, subEntity));
        }

        /// <summary>
        /// An asynchronous version of <see cref="Update{E}(object, Expression{Func{E, bool}})">Update</see>.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <param name="subEntity">Entity to update in database.</param>
        /// <param name="where">Where expression.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<int> UpdateAsync<E>(object subEntity, Expression<Func<E, bool>> where) where E : class
        {
            if (subEntity == null)
                throw new ArgumentNullException(nameof(subEntity));
            if (where == null)
                throw new ArgumentNullException(nameof(where));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetUpdateSql(subEntity.GetType(), where);
            return ExecAsync(dbConnection => dbConnection.ExecuteAsync(sql, subEntity));
        }

        /// <summary>
        /// Deletes entity with <paramref name="key"/> from the database.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <typeparam name="K">Type of the key.</typeparam>
        /// <param name="key">Key of the entity.</param>
        /// <returns>Count of a effected enities.</returns>
        public virtual int Delete<E, K>(K key) where E : class, K where K : class
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetDeleteSql();
            return Exec(dbConnection => dbConnection.Execute(sql, key));
        }

        /// <summary>
        /// An asynchronous version of <see cref="Delete{E, K}(K)">Delete</see>.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <typeparam name="K">Type of the key.</typeparam>
        /// <param name="key">Key of the entity.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<int> DeleteAsync<E, K>(K key) where E : class, K where K : class
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetDeleteSql();
            return ExecAsync(dbConnection => dbConnection.ExecuteAsync(sql, key));
        }

        /// <summary>
        /// Deletes entities which meet <paramref name="where"/> conditions from the database.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <param name="where">Where expression.</param>
        /// <param name="param">Query parameters.</param>
        /// <returns>Count of a effected enities.</returns>
        public virtual int Delete<E>(Expression<Func<E, bool>> where, object param = null) where E : class
        {
            if (where == null)
                throw new ArgumentNullException(nameof(where));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetDeleteSql(where, param);
            return Exec(dbConnection => dbConnection.Execute(sql, param));
        }

        /// <summary>
        /// An asynchronous version of <see cref="Delete{E}(Expression{Func{E, bool}}, object)">Delete</see>.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <param name="where">Where expression.</param>
        /// <param name="param">Query parameters.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<int> DeleteAsync<E>(Expression<Func<E, bool>> where, object param = null) where E : class
        {
            if (where == null)
                throw new ArgumentNullException(nameof(where));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetDeleteSql(where, param);
            return ExecAsync(dbConnection => dbConnection.ExecuteAsync(sql, param));
        }

        /// <summary>
        /// Truncates entities from the database.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        public virtual void Truncate<E>() where E : class
        {
            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetTruncateSql();
            Exec(dbConnection => dbConnection.Execute(sql));
        }

        /// <summary>
        /// An asynchronous version of <see cref="Truncate{E}">Delete</see>.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task TruncateAsync<E>() where E : class
        {
            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetTruncateSql();
            return ExecAsync(dbConnection => dbConnection.ExecuteAsync(sql));
        }

        /// <summary>
        /// Returns an entity with the <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <typeparam name="K">Type of the entity key.</typeparam>
        /// <param name="key">Entity key.</param>
        /// <param name="type">Type which sets a subsets of the fields to retrive.</param>
        /// <returns>Entity form database or null if entity was not found.</returns>
        public virtual E GetByKey<E, K>(K key, Type type = null) where E : class, K where K : class
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetSelectByKeySql(type);
            return Exec(dbConnection => dbConnection.Query<E>(sql, key).FirstOrDefault());
        }

        /// <summary>
        /// An asynchronous version of <see cref="GetByKey{E, K}">GetByKeyAsync</see>.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <typeparam name="K">Type of the entity key.</typeparam>
        /// <param name="key">Entity key.</param>
        /// <param name="type">Type which sets a subsets of the fields to retrive.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<E> GetByKeyAsync<E, K>(K key, Type type = null) where E : class, K where K : class
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetSelectByKeySql(type);
            return ExecAsync(async dbConnection => (await dbConnection.QueryAsync<E>(sql, key)).FirstOrDefault());
        }

        /// <summary>
        /// Returns a collection of entities which meet a <paramref name="where"/> condition.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <param name="type">Type which sets a subsets of the fields to retrive.</param>
        /// <param name="where">Where expression.</param>
        /// <param name="param">Query parameters</param>
        /// <param name="orderBy">Sort expression.</param>
        /// <returns>A collection of entities from database. Empty collection if no items was found.</returns>
        public virtual IEnumerable<E> Get<E>(
            Type type = null,
            Expression<Func<E, bool>> where = null,
            object param = null,
            Expression<Func<IEnumerable<E>, IEnumerable<E>>> orderBy = null) where E : class
        {
            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetSelectSql(type, where, param, orderBy);
            return Exec(dbConnection => dbConnection.Query<E>(sql, param));
        }

        /// <summary>
        /// An asynchronous version of <see cref="Get{E}">Get</see>.
        /// </summary>
        /// <typeparam name="E">Type of the entity</typeparam>
        /// <param name="type">Type which sets a subsets of the fields to retrive.</param>
        /// <param name="where">Where expression.</param>
        /// <param name="param">Query parameters</param>
        /// <param name="orderBy">Sort expression.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<IEnumerable<E>> GetAsync<E>(
            Type type = null,
            Expression<Func<E, bool>> where = null,
            object param = null,
            Expression<Func<IEnumerable<E>, IEnumerable<E>>> orderBy = null) where E : class
        {
            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetSelectSql(type, where, param, orderBy);
            return ExecAsync(dbConnection => dbConnection.QueryAsync<E>(sql, param));
        }

        /// <summary>
        /// Returns a scalar expression.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="expression">Scalar expression.</param>
        /// <param name="where">Where expression.</param>
        /// <param name="param">Query parameters.</param>
        /// <returns>The result of execution a scalar query.</returns>
        public virtual T GetScalar<E, T>(
            Expression<Func<IEnumerable<E>, T>> expression,
            Expression<Func<E, bool>> where = null,
            object param = null) where E : class
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetSelectScalarSql<T>(expression, where, param);
            return Exec(dbConnection => dbConnection.ExecuteScalar<T>(sql, param));
        }

        /// <summary>
        /// An asynchronous version of <see cref="GetScalar{E, T}">GetScalar</see>.
        /// </summary>
        /// <typeparam name="E">Type of the entity.</typeparam>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="expression">Scalar expression.</param>
        /// <param name="where">Where expression.</param>
        /// <param name="param">Query parameters</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<T> GetScalarAsync<E, T>(
            Expression<Func<IEnumerable<E>, T>> expression,
            Expression<Func<E, bool>> where = null,
            object param = null) where E : class
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetSelectScalarSql<T>(expression, where, param);
            return ExecAsync(dbConnection => dbConnection.ExecuteScalarAsync<T>(sql, param));
        }
    }
}
