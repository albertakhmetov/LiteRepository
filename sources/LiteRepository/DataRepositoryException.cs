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

namespace LiteRepository
{
    public class DataRepositoryException : Exception
    {
        public DataRepositoryException()
        { }

        public DataRepositoryException(string message)
            : base(message)
        { }

        public DataRepositoryException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
