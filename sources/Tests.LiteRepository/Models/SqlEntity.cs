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
    [StoreAs("cash_check_item")]
    public class SqlEntity
    {
        [StoreAs("id")]
        [PrimaryKey()]
        public long Id { get; set; }

        [StoreAs("shop_id")]
        [PrimaryKey()]
        public long ShopId { get; set; }

        [StoreAs("text")]
        public string Text { get; set; }

        [StoreAs("price")]
        public decimal Price { get; set; }

        [Ignore]
        public bool IsInvalidated { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as SqlEntity);
        }

        public bool Equals(SqlEntity e)
        {
            return Id == e.Id && ShopId == e.ShopId && Text == e.Text && Price == e.Price;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ ShopId.GetHashCode() ^ (Text ?? string.Empty).GetHashCode() ^ Price.GetHashCode();
        }
    }

    public class SqlEntityId
    {
        public long Id { get; set; }

        public long ShopId { get; set; }
    }
}
