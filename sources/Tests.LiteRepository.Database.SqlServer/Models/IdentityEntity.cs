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
    [StoreAs("people")]
    public class IdentityEntity
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
            return Equals(obj as IdentityEntity);
        }

        public bool Equals(IdentityEntity e)
        {
            return Id == e.Id && FirstName == e.FirstName && SecondName == e.SecondName && Birthday == e.Birthday;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ (FirstName ?? string.Empty).GetHashCode() ^ (SecondName ?? string.Empty).GetHashCode() ^ Birthday.GetHashCode();
        }
    }

    public class IdentityId
    {
        public long Id { get; set; }
    }
}
