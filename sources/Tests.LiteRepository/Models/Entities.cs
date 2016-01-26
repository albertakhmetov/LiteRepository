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

namespace LiteRepository.Models
{
    public class Entity
    {
        public long Id { get; set; }
        public string Text { get; set; }
    }

    public class IdKey
    {
        public long Id { get; set; }
    }
}
