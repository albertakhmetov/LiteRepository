﻿/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Database
{
    [AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false)]
    public sealed class PrimaryKeyAttribute : Attribute
    {
        public bool IsIdentity
        {
            get;
            private set;
        }

        public PrimaryKeyAttribute(bool isIdentity = false)
        {
            IsIdentity = isIdentity;
        }
    }
}
