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
    /// Provides implementation of basic Repository.
    /// </summary>
    /// <typeparam name="E">Type of the Entity.</typeparam>
    /// <typeparam name="K">Type of the Primary Key for Entity.</typeparam>
    public class Repository<E, K>
        where E : class, K
        where K : class
    {
        /// <summary>
        /// Gets a <see cref="Db"/> instance.
        /// </summary>
        public Db Db
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{E, K}"/> class.
        /// </summary>
        /// <param name="db">Instance of the <see cref="Db"/>.</param>
        public Repository(Db db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));
            Db = db;
        }

        /// <summary>
        /// Inserts the <paramref name="entity"/> into the database.
        /// </summary>
        /// <param name="entity">Entity to insert.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <example>
        /// <para>For inserting the entity:</para>        
        /// <code lang="c#">
        /// var entity = new Entity { FirstName = "Alex", SecondName = "Lion" };
        /// await repository.InsertAsync(entity);
        /// </code>
        /// </example>
        public virtual Task<E> InsertAsync(E entity)
        {
            return Db.InsertAsync<E>(entity);
        }

        /// <summary>
        /// Updates the <paramref name="entity"/> in the database.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <example>
        /// <para>For updating the entity:</para>
        /// <code lang="c#">
        /// var entity = new Entity { FirstName = "Alex", SecondName = "Lion" };
        /// await repository.UpdateAsync(entity);
        /// </code>       
        /// </example>
        public virtual Task<int> UpdateAsync(E entity)
        {
            return Db.UpdateAsync<E>(entity);
        }

        /// <summary>
        /// Deletes the entity with a <paramref name="key"/> from the database.
        /// </summary>
        /// <param name="key">Key of the entity.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <example>
        /// <para>For deleting the entity:</para>
        /// <code lang="c#">
        /// var key = new EntityKey { Id = 12 };
        /// await repository.DeleteAsync(entity);
        /// </code>       
        /// </example>
        public virtual Task<int> DeleteAsync(K key)
        {
            return Db.DeleteAsync<E, K>(key);
        }

        /// <summary>
        /// Gets a entity with <paramref name="key"/> from the database.
        /// </summary>
        /// <param name="key">Key of the entity.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <example>
        /// <para>For getting the entity by it's key:</para>
        /// <code lang="c#">
        /// var key = new EntityKey { Id = 12 };
        /// var entity = await repository.GetAsync(entity);
        /// </code>       
        /// </example>
        public virtual Task<E> GetAsync(K key)
        {
            return Db.GetByKeyAsync<E, K>(key);
        }

        /// <summary>
        /// Gets all entities from database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <example>
        /// <para>For getting all entities from database:</para>
        /// <code lang="c#">
        /// var entities = await repository.GetAllAsync(entity);
        /// </code>       
        /// </example>
        public virtual Task<IEnumerable<E>> GetAllAsync()
        {
            return Db.GetAsync<E>();
        }

        /// <summary>
        /// Gets a count of all entities in the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <example>
        /// <para>For getting a count of entities in database:</para>
        /// <code lang="c#">
        /// var entities = await repository.GetAllAsync(entity);
        /// </code>       
        /// </example>
        public virtual Task<long> GetCountAsync()
        {
            return Db.GetScalarAsync<E, long>(i => i.Count());
        }
    }
}
