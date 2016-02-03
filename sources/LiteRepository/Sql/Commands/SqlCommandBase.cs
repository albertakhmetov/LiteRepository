﻿/*

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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Sql.Commands
{
    public abstract class SqlCommandBase<E>
        where E : class
    {
        public ISqlBuilder SqlBuilder
        {
            get; private set;
        }

        protected SqlCommandBase(ISqlBuilder sqlBuilder)
        {
            if (sqlBuilder == null)
                throw new ArgumentNullException(nameof(sqlBuilder));
            SqlBuilder = sqlBuilder;
        }

        protected void CheckNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);
        }
    }
}