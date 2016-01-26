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

namespace LiteRepository.Database
{
    public interface ISqlGenerator
    {
        string SelectSql { get; }
        string SelectAllSql { get; }
        string CountSql { get; }
        string InsertSql { get; }
        string UpdateSql { get; }
        string DeleteSql { get; }
        string DeleteAllSql { get; }
    }
}
