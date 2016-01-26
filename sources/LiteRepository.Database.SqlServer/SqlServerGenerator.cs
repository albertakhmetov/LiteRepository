/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Database.SqlServer
{
    public sealed class SqlServerGenerator<T> : ISqlGenerator
        where T : class
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

        public SqlServerGenerator()
        {
            var metadata = new EntityMetadata<T>();

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
