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
    public abstract class SqlDialectBase
    {
        public abstract string Select(string tableName, string fields, string where, string order, int? top = null);
        public abstract string SelectScalar(string tableName, string expression, string where);
        public abstract string Insert(string tableName, string fields, string values);
        public abstract string Update(string tableName, string set, string where);
        public abstract string Delete(string tableName, string where);

        public abstract string Parameter(string name);
        public abstract bool HasParameters(string vaue);
    }
}
