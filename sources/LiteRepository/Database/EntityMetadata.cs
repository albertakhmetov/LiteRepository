/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Database
{
    public sealed class EntityMetadata<T> : IEnumerable<EntityMetadataItem>
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

        public EntityMetadata()
        {
            var type = typeof(T);

            var typeStoreAs = type.GetCustomAttributes(typeof(StoreAsAttribute), true).FirstOrDefault() as StoreAsAttribute;
            Name = type.Name;
            DbName = (typeStoreAs == null) ? type.Name : typeStoreAs.DbName;

            var fields = new List<EntityMetadataItem>();
            foreach (var property in type.GetProperties())
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
