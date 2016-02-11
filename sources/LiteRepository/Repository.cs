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
    public class Repository<E, K> : IRepository<E, K>
        where E : class, K
        where K : class
    {
        public IDb Db
        {
            get; private set;
        }

        public Repository(IDb db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));
            Db = db;
        }

        public Task<E> InsertAsync(E entity)
        {
            return Db.InsertAsync<E>(entity);
        }

        public Task<int> UpdateAsync(E entity)
        {
            return Db.UpdateAsync<E>(entity);
        }

        public Task<int> DeleteAsync(K key)
        {
            return Db.DeleteAsync<E, K>(key);
        }

        public Task<E> GetAsync(K key)
        {
            return Db.GetByKeyAsync<E, K>(key);
        }

        public Task<IEnumerable<E>> GetAllAsync()
        {
            return Db.GetAsync<E>();
        }

        public Task<long> GetCountAsync()
        {
            return Db.GetScalarAsync<E, long>(i => i.Count());
        }
    }
}
