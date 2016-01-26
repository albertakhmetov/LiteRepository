﻿/***************************************** 
 *   Copyright © 2016, Albert Akhmetov   *
 *   email: akhmetov@live.com            *
 *                                       *
 *****************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Database
{
    public interface IDb
    {
        IDbConnection OpenConnection();
        void CloseConnection(IDbConnection dbConnection);

        ISqlGenerator GetSqlGenerator<E>() where E : class;
        ISqlExecutor GetSqlExecutor();
    }
}
