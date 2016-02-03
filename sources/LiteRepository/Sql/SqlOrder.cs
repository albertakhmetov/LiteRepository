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

namespace LiteRepository.Sql
{
    public sealed class SqlOrder
    {
        public enum SqlDirection
        {
            Asc,
            Desc
        }

        public string DbName
        {
            get; private set;
        }

        public SqlDirection Direction
        {
            get; private set;
        }

        public SqlOrder(string dbName, SqlDirection direction = SqlDirection.Asc)
        {
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentException("", nameof(dbName));
            DbName = dbName;
            Direction = direction;
        }
    }
}
