/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Database.SqlServer.Models
{
    [StoreAs("cash_check_item")]
    public class Entity
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
            return Equals(obj as Entity);
        }

        public bool Equals(Entity e)
        {
            return Id == e.Id && ShopId == e.ShopId && Text == e.Text && Price == e.Price;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ ShopId.GetHashCode() ^ (Text ?? string.Empty).GetHashCode() ^ Price.GetHashCode();
        }
    }

    public class EntityId
    {
        public long Id { get; set; }

        public long ShopId { get; set; }
    }
}
