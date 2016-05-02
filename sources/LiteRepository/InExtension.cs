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
    /// Provides extension methods.
    /// </summary>
    public static class InExtension
    {
        /// <summary>
        /// Provides functionality to check if value contains in the collection.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="value">Current value.</param>
        /// <param name="values">Collection of values.</param>
        /// <returns>
        /// true if <paramref name="value"/> contains in the <paramref name="values"/>; otherwise, false.
        /// </returns>
        public static bool In<T>(this T value, params T[] values)
        {
            return values.Contains(value);
        }
    }
}
