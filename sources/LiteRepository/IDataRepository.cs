/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using LiteRepository.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteRepository
{
    /// <summary>
    /// Data Repository
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <typeparam name="K"></typeparam>
    public interface IDataRepository<E, K>
        where E : class
        where K : class
    {
        IDb Db { get; }

        /// <summary>
        /// Raises when entity was inserted
        /// </summary>
        IObservable<E> InsertedObservable { get; }

        /// <summary>
        /// Raises when entity was updated
        /// </summary>
        IObservable<E> UpdatedObservable { get; }

        /// <summary>
        /// Raises when entity was deleted
        /// </summary>
        IObservable<K> DeletedObservable { get; }

        /// <summary>
        /// Inserts entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Inserted entity</returns>
        Task<E> InsertAsync(E entity, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Updates entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Count of updated entity</returns>
        Task<int> UpdateAsync(E entity, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Updates or inserts entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <remarks>If entity with the same key exists - it will be updated, otherwise - new entity will be created</remarks>
        /// <returns>Updated (inserted) entity</returns>
        Task<E> UpdateOrInsertAsync(E entity, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Deletes entity with the set key
        /// </summary>
        /// <param name="key">Entities key</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Count of deleted entities</returns>
        Task<int> DeleteAsync(K key, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Deletes all entities
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Count of deleted entities</returns>
        Task<int> DeleteAllAsync(CancellationToken? cancellationToken = null);

        /// <summary>
        /// Loads entity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Entity with the same key or null if entity was not found</returns>
        Task<E> GetAsync(K key, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Returns all entities
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Collection of enities</returns>
        Task<IEnumerable<E>> GetAllAsync(CancellationToken? cancellationToken = null);

        /// <summary>
        /// Count of all entities
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Count of all entities</returns>
        Task<long> GetCountAsync(CancellationToken? cancellationToken = null);
    }
}
