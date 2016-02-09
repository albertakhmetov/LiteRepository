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

using Xunit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository
{
    public class InExtensionTests
    {
        [Fact]
        public void NullValue_Test()
        {
            var v = default(string);
            Assert.False(InExtension.In(v, "A", "B", "C"));
            Assert.False(v.In("A", "B", "C"));

        }

        [Fact]
        public void NullValue_NullParams_Test()
        {
            var v = default(string);
            Assert.True(InExtension.In(v, default(string)));
        }

        [Fact]
        public void EmptyParams_Test()
        {
            var v = "";
            Assert.False(InExtension.In(v));
        }

        [Fact]
        public void Contains_Test()
        {
            var v = "B";
            Assert.True(v.In("A", "B", "C"));
        }

        [Fact]
        public void NotContains_Test()
        {
            var v = "D";
            Assert.False(v.In("A", "B", "C"));
        }
    }
}
