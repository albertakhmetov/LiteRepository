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

using LiteRepository.Common;
using LiteRepository.Sql.Attributes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Sql
{
    public sealed class SqlMetadata : IEnumerable<SqlMetadata.Property>
    {
        #region Metadata's cache

        private static readonly ConcurrentDictionary<Type, SqlMetadata> _cache = new ConcurrentDictionary<Type, SqlMetadata>();

        public static SqlMetadata GetSqlMetadata(Type type)
        {
            return _cache.GetOrAdd(type, t => new SqlMetadata(t));
        }

        public static void ClearCache()
        {
            _cache.Clear();
        }

        #endregion

        public sealed class Property
        {
            public string Name
            {
                get; private set;
            }

            public string DbName
            {
                get; private set;
            }

            public bool IsPrimaryKey
            {
                get; private set;
            }

            public bool IsIdentity
            {
                get; private set;
            }

            public Property(string name, string dbName, bool isPrimaryKey, bool isIdentity)
            {
                Name = name;
                DbName = dbName;
                IsPrimaryKey = isPrimaryKey;
                IsIdentity = isIdentity;
            }
        }

        private readonly IList<Property> _properties;

        public Type Type
        {
            get; private set;
        }

        public string Name
        {
            get; private set;
        }

        public string DbName
        {
            get; private set;
        }

        public bool IsIdentity
        {
            get; private set;
        }

        public Property this[int index]
        {
            get { return _properties[index]; }
        }

        public int Count
        {
            get { return _properties.Count; }
        }

        public SqlMetadata(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            Type = type;
            IsIdentity = type.GetInterfaces().Contains(typeof(IIdentityEntity));

            var typeStoreAs = Type.GetCustomAttributes(typeof(SqlAliasAttribute), true).FirstOrDefault() as SqlAliasAttribute;
            Name = Type.Name;
            DbName = (typeStoreAs == null) ? Type.Name : typeStoreAs.DbName;

            var fields = new List<Property>();
            foreach (var property in Type.GetProperties())
            {
                if (property.GetCustomAttributes(typeof(SqlIgnoreAttribute), true).Length != 0)
                    continue;

                var storeAs = property.GetCustomAttributes(typeof(SqlAliasAttribute), true).FirstOrDefault() as SqlAliasAttribute;

                if (IsIdentity)
                {
                    var isPrimaryKey = property.Name == nameof(IIdentityEntity.Id);
                    fields.Add(new Property(
                       property.Name,
                       storeAs == null ? property.Name : storeAs.DbName,
                       isPrimaryKey,
                       isPrimaryKey));
                }
                else
                {
                    var primaryKey = property.GetCustomAttributes(typeof(SqlKeyAttribute), true).FirstOrDefault() as SqlKeyAttribute;
                    fields.Add(new Property(
                        property.Name,
                        storeAs == null ? property.Name : storeAs.DbName,
                        primaryKey != null,
                        false));
                }
            }

            _properties = fields;
        }

        public IEnumerable<Property> GetSubsetForType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
        
            var typeProperties = type.GetProperties().Select(i => i.Name);
            return _properties.Where(i => typeProperties.Contains(i.Name));
        }

        public IEnumerator<Property> GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SqlMetadata);
        }

        public bool Equals(SqlMetadata metadata)
        {
            return metadata != null && (metadata.Type == Type);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
    }
}
