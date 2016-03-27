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

using LiteRepository.Attributes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository
{
    /// <summary>
    /// Provides functionality for extracting metadata from type definition
    /// </summary>
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

        /// <summary>
        /// Defines item of metadata information
        /// </summary>
        public sealed class Property
        {
            /// <summary>
            /// Gets a name of the property
            /// </summary>
            public string Name
            {
                get; private set;
            }

            /// <summary>
            /// Gets a name of the database column
            /// </summary>
            public string DbName
            {
                get; private set;
            }

            /// <summary>
            /// Gets a value indicating whether the property is a part of the primary key 
            /// </summary>
            public bool IsPrimaryKey
            {
                get; private set;
            }

            /// <summary>
            /// Gets a value indicating whether the property is identity primary key
            /// </summary>
            public bool IsIdentity
            {
                get; private set;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Property"/> class
            /// </summary>
            /// <param name="name">The name of the property.</param>
            /// <param name="dbName">The name of the database column.</param>
            /// <param name="isPrimaryKey">Determines whether a property is a part of the primary key</param>
            /// <param name="isIdentity">Determines whether a property is a identity key.</param>
            public Property(string name, string dbName, bool isPrimaryKey, bool isIdentity)
            {
                Name = name;
                DbName = dbName.ToLower();
                IsPrimaryKey = isPrimaryKey;
                IsIdentity = isIdentity;
            }
        }

        private readonly IList<Property> _properties;
        private readonly Dictionary<string, string> _nameToDbNameDictionary;

        /// <summary>
        /// Gets a type of the entity
        /// </summary>
        public Type Type
        {
            get; private set;
        }

        /// <summary>
        /// Gets a name of the entity
        /// </summary>
        public string Name
        {
            get; private set;
        }

        /// <summary>
        /// Gets a database table name
        /// </summary>
        public string DbName
        {
            get; private set;
        }

        /// <summary>
        /// Gets a value indicating whether the table is the table with identity primary key
        /// </summary>
        public bool IsIdentity
        {
            get; private set;
        }

        /// <summary>
        /// Gets a property information by <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of property information</param>
        /// <returns></returns>
        public Property this[int index]
        {
            get { return _properties[index]; }
        }

        /// <summary>
        /// Gets a property information by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <returns></returns>
        public string this[string name]
        {
            get { return _nameToDbNameDictionary[name]; }
        }

        /// <summary>
        /// Gets a count of properties
        /// </summary>
        public int Count
        {
            get { return _properties.Count; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMetadata"/> class
        /// </summary>
        /// <param name="type">Type of the entity</param>
        public SqlMetadata(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            Type = type;
            IsIdentity = type.GetInterfaces().Contains(typeof(IIdentityEntity));

            var typeStoreAs = Type.GetCustomAttributes(typeof(SqlAliasAttribute), true).FirstOrDefault() as SqlAliasAttribute;
            Name = Type.Name;
            DbName = ((typeStoreAs == null) ? Type.Name : typeStoreAs.DbName).ToLower();

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
            _nameToDbNameDictionary = _properties.ToDictionary(i => i.Name, i => i.DbName);
        }

        /// <summary>
        /// Gets a collection of property information only for properties which contains in the <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type what represents a subset of the entity</param>
        /// <returns>Collection of <see cref="Property"/>.</returns>
        public IEnumerable<Property> GetSubsetForType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var typeProperties = type.GetProperties().Select(i => i.Name);
            return _properties.Where(i => typeProperties.Contains(i.Name));
        }

        /// <summary>
        /// Returns a enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Property> GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Compares <paramref name="obj"/> and current instance
        /// </summary>
        /// <param name="obj">Another instance of the metadata to compare</param>
        /// <returns>True if <paramref name="obj"/> is equals to the current metadata</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SqlMetadata);
        }

        /// <summary>
        /// Compares <paramref name="metadata"/> and current instance
        /// </summary>
        /// <param name="metadata">Another instance of the metadata to compare</param>
        /// <returns>True if <paramref name="metadata"/> is equals to the current metadata</returns> 
        public bool Equals(SqlMetadata metadata)
        {
            return metadata != null && (metadata.Type == Type);
        }

        /// <summary>
        /// Returns the Hash Code for this instance
        /// </summary>
        /// <returns>The Hash Code for this instance</returns>
        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
    }
}
