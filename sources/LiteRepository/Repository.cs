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
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository
{
    /// <summary>
    /// Provides implementation of basic Repository
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <typeparam name="K"></typeparam>
    public class Repository<E, K>
        where E : class, K
        where K : class
    {
        /// <summary>
        /// Gets <see cref="Db"/> instance
        /// </summary>
        public Db Db
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{E, K}"/> class
        /// </summary>
        /// <param name="db"><see cref="Db"/></param>
        public Repository(Db db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));
            Db = db;
        }

        /// <summary>
        /// Inserts the <paramref name="entity"/>
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<E> InsertAsync(E entity)
        {
            return Db.InsertAsync<E>(entity);
        }

        /// <summary>
        /// Updates the <paramref name="entity"/>
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<int> UpdateAsync(E entity)
        {
            return Db.UpdateAsync<E>(entity);
        }

        /// <summary>
        /// Deletes the entity with a <paramref name="key"/>
        /// </summary>
        /// <param name="key">Key of the entity</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<int> DeleteAsync(K key)
        {
            return Db.DeleteAsync<E, K>(key);
        }

        /// <summary>
        /// Gets a entity with <paramref name="key"/>
        /// </summary>
        /// <param name="key">Key of the entity</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<E> GetAsync(K key)
        {
            return Db.GetByKeyAsync<E, K>(key);
        }

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<IEnumerable<E>> GetAllAsync()
        {
            return Db.GetAsync<E>();
        }

        /// <summary>
        /// Gets a count of all entities
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task<long> GetCountAsync()
        {
            return Db.GetScalarAsync<E, long>(i => i.Count());
        }
    }
}
