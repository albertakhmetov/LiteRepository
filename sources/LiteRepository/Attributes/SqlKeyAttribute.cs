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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Attributes
{
    /// <summary>
    /// Denotes a property used as a part of a primary key.
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SqlKeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlKeyAttribute"/> class.
        /// </summary>
        public SqlKeyAttribute()
        { }
    }
}
