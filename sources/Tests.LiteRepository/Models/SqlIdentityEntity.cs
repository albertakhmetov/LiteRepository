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

namespace LiteRepository.Database.Models
{
    [StoreAs("people")]
    public class SqlIdentityEntity
    {
        [StoreAs("id")]
        [PrimaryKey(isIdentity: true)]
        public long Id { get; set; }

        [StoreAs("first_name")]
        public string FirstName { get; set; }

        [StoreAs("second_name")]
        public string SecondName { get; set; }

        [StoreAs("birthday")]
        public DateTime Birthday { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as SqlIdentityEntity);
        }

        public bool Equals(SqlIdentityEntity e)
        {
            return Id == e.Id && FirstName == e.FirstName && SecondName == e.SecondName && Birthday == e.Birthday;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ (FirstName ?? string.Empty).GetHashCode() ^ (SecondName ?? string.Empty).GetHashCode() ^ Birthday.GetHashCode();
        }
    }

    public class SqlIdentityId
    {
        public long Id { get; set; }
    }
}
