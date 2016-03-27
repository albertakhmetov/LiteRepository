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
    public class Db
    {
        private readonly SqlDialect _sqlDialect;
        private readonly DbConnection _dbConnection;
        private readonly Func<DbConnection> _dbConnectionFactory;

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

        public virtual int Update<E>(E entity) where E : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var sqlExpression = GetSqlExpression<E>();
            return Exec(dbConnection => dbConnection.Execute(sqlExpression.GetUpdateSql(), entity));
        }

        public virtual Task<int> UpdateAsync<E>(E entity) where E : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var sqlExpression = GetSqlExpression<E>();
            return ExecAsync(dbConnection => dbConnection.ExecuteAsync(sqlExpression.GetUpdateSql(), entity));
        }

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

        public virtual int Delete<E, K>(K key) where E : class, K where K : class
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetDeleteSql();
            return Exec(dbConnection => dbConnection.Execute(sql, key));
        }

        public virtual Task<int> DeleteAsync<E, K>(K key) where E : class, K where K : class
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetDeleteSql();
            return ExecAsync(dbConnection => dbConnection.ExecuteAsync(sql, key));
        }

        public virtual int Delete<E>(Expression<Func<E, bool>> where, object param = null) where E : class
        {
            if (where == null)
                throw new ArgumentNullException(nameof(where));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetDeleteSql(where, param);
            return Exec(dbConnection => dbConnection.Execute(sql, param));
        }

        public virtual Task<int> DeleteAsync<E>(Expression<Func<E, bool>> where, object param = null) where E : class
        {
            if (where == null)
                throw new ArgumentNullException(nameof(where));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetDeleteSql(where, param);
            return ExecAsync(dbConnection => dbConnection.ExecuteAsync(sql, param));
        }

        public virtual void Truncate<E>() where E : class
        {
            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetTruncateSql();
            Exec(dbConnection => dbConnection.Execute(sql));
        }

        public virtual Task TruncateAsync<E>() where E : class
        {
            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetTruncateSql();
            return ExecAsync(dbConnection => dbConnection.ExecuteAsync(sql));
        }

        public virtual E GetByKey<E, K>(K key, Type type = null) where E : class, K where K : class
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetSelectByKeySql(type);
            return Exec(dbConnection => dbConnection.Query<E>(sql, key).FirstOrDefault());
        }

        public virtual Task<E> GetByKeyAsync<E, K>(K key, Type type = null) where E : class, K where K : class
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var sqlExpression = GetSqlExpression<E>();
            var sql = sqlExpression.GetSelectByKeySql(type);
            return ExecAsync(async dbConnection => (await dbConnection.QueryAsync<E>(sql, key)).FirstOrDefault());
        }

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
