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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Database
{
    public sealed class EntityMetadata : IEnumerable<EntityMetadataItem>
    {
        private readonly IList<EntityMetadataItem> _items;

        public string Name
        {
            get; private set;
        }

        public string DbName
        {
            get; private set;
        }

        public EntityMetadataItem this[int index]
        {
            get { return _items[index]; }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public EntityMetadata(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            var typeStoreAs = entityType.GetCustomAttributes(typeof(StoreAsAttribute), true).FirstOrDefault() as StoreAsAttribute;
            Name = entityType.Name;
            DbName = (typeStoreAs == null) ? entityType.Name : typeStoreAs.DbName;

            var fields = new List<EntityMetadataItem>();
            foreach (var property in entityType.GetProperties())
            {
                if (property.GetCustomAttributes(typeof(IgnoreAttribute), true).Length != 0)
                    continue;

                var storeAs = property.GetCustomAttributes(typeof(StoreAsAttribute), true).FirstOrDefault() as StoreAsAttribute;
                var primaryKey = property.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).FirstOrDefault() as PrimaryKeyAttribute;

                fields.Add(new EntityMetadataItem(
                    property.Name,
                    storeAs == null ? property.Name : storeAs.DbName,
                    primaryKey != null,
                    primaryKey == null ? false : primaryKey.IsIdentity));
            }

            _items = fields;
        }

        public IEnumerator<EntityMetadataItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
