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

using NSubstitute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Sql.Commands
{
    public static class DbMocks
    {
        public static DbCommand CreateCommand(IList<DbParameter> dbParameters)
        {
            var dbCommand = Substitute.For<DbCommand, IDbCommand>();
            ((IDbCommand)dbCommand).CreateParameter().Returns(x => dbParameters.Last());
            ((IDbCommand)dbCommand)
                .When(x => x.CreateParameter())
                .Do(x => dbParameters.Add(Substitute.For<DbParameter>()));

            return dbCommand;
        }

        public static DbConnection CreateConnection(DbCommand dbCommand, ConnectionState connectionState = ConnectionState.Open)
        {
            var dbConnection = Substitute.For<DbConnection>();
            dbConnection.State.Returns(connectionState);
            dbConnection.CreateCommand().Returns(dbCommand);

            return dbConnection;
        }
    }
}
