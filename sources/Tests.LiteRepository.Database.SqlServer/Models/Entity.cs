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
    }
}
