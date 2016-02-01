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

using LiteRepository.Sql.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Sql
{
    public class SqlRepository<E, K> : IRepository<E, K>
    {
        private readonly IDb _db;
        private readonly SqlInsert<E> _sqlInsert;


        public virtual Task<E> InsertAsync(E entity)
        {
            return _sqlInsert.ExecuteAsync(entity, _db);
        }

        public virtual Task<int> UpdateAsync(E entity)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> DeleteAsync(K key)
        {
            throw new NotImplementedException();
        }

        public virtual Task<E> GetAsync(K key)
        {
            throw new NotImplementedException();
        }
    }
}
