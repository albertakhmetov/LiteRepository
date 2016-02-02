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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Sql.Models
{
    [SqlAlias("users")]
    public class IdentityEntity : IIdentityEntity
    {
        public long Id
        {
            get; set;
        }

        [SqlAlias("first_name")]
        public string FirstName
        {
            get; set;
        }

        [SqlAlias("second_name")]
        public string SecondName
        {
            get; set;
        }

        [SqlIgnore]
        public DateTime Timestamp
        {
            get; set;
        }

        public DateTime Birthday
        {
            get; set;
        }

        public object UpdateId(long id)
        {
            return new IdentityEntity
            {
                Id = id,
                FirstName = this.FirstName,
                SecondName = this.SecondName,
                Birthday = this.Birthday
            };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IdentityEntity);
        }

        public bool Equals(IdentityEntity entity)
        {
            return entity != null && entity.Id == Id
                && entity.FirstName == FirstName
                && entity.SecondName == SecondName
                && entity.Birthday == Birthday;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() 
                ^ (FirstName ?? string.Empty).GetHashCode()
                ^ (SecondName ?? string.Empty).GetHashCode()
                ^ Birthday.GetHashCode();
        }
    }
}
