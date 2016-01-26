/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteRepository.Database
{
    public interface ISqlExecutor
    {
        Task<T> QueryScalarAsync<T>(IDbConnection connection, string sql, CancellationToken? cancellationToken = default(CancellationToken?));
        Task<T> QueryScalarAsync<Q, T>(IDbConnection connection, string sql, Q parameters, CancellationToken? cancellationToken = default(CancellationToken?));

        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, CancellationToken? cancellationToken = default(CancellationToken?));
        Task<IEnumerable<T>> QueryAsync<Q, T>(IDbConnection connection, string sql, Q parameters, CancellationToken? cancellationToken = default(CancellationToken?));

        Task<int> ExecuteAsync(IDbConnection connection, string sql, CancellationToken? cancellationToken = default(CancellationToken?));
        Task<int> ExecuteAsync<Q>(IDbConnection connection, string sql, Q parameters, CancellationToken? cancellationToken = default(CancellationToken?));
    }
}
