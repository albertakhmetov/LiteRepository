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
    /// Denotes a class has identity primary key
    /// </summary>
    public interface IIdentityEntity
    {
        /// <summary>
        /// Gets the Primary Key of an entity
        /// </summary>
        long Id { get; }

        /// <summary>
        /// Provides functionality to update Primary Key
        /// </summary>
        /// <param name="id">A new primary key</param>
        /// <returns>Returns instance with a new primary key. Current or creates a new (depends on realization)</returns>
        object UpdateId(long id);
    }

    /// <summary>
    /// Defines a primary key for entities with identity primary keys
    /// </summary>
    public sealed class IdentityKey : IIdentityEntity
    {
        /// <summary>
        /// Gets the Primary Key of entity
        /// </summary>
        public long Id
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityKey"/> class.
        /// </summary>
        /// <param name="id">Primary Key of an entity</param>
        public IdentityKey(long id)
        {
            Id = id;
        }

        /// <summary>
        /// Provides functionality to update Primary Key
        /// </summary> 
        /// <returns>Returns a new instance with <paramref name="id"/> as Primary</returns>
        public object UpdateId(long id)
        {
            return new IdentityKey(id);
        }
    }
}
