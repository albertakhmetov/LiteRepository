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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Models
{
    [SqlAlias("students")]
    public class Entity
    {
        public class Key
        {
            public long Cource
            {
                get; set;
            }

            public char Letter
            {
                get; set;
            }
        }

        [SqlKey]
        public long Cource
        {
            get; set;
        }

        [SqlKey]
        public char Letter
        {
            get; set;
        }
        
        [SqlKey]
        [SqlAlias("local_id")]
        public int LocalId
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

        public override bool Equals(object obj)
        {
            return Equals(obj as Entity);
        }

        public bool Equals(Entity entity)
        {
            return entity != null && entity.Cource == Cource && entity.Letter == Letter
                && entity.FirstName == FirstName
                && entity.SecondName == SecondName
                && entity.Birthday == Birthday;
        }

        public override int GetHashCode()
        {
            return Cource.GetHashCode() ^ Letter.GetHashCode() 
                ^ (FirstName ?? string.Empty).GetHashCode()
                ^ (SecondName ?? string.Empty).GetHashCode()
                ^ Birthday.GetHashCode();
        }
    }
}
