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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRepository.Database
{
    public sealed class StorageProcedureSqlGenerator : ISqlGenerator
    {
        public string InsertSql
        {
            get; private set;
        }

        public string UpdateSql
        {
            get; private set;
        }

        public string DeleteSql
        {
            get; private set;
        }

        public string DeleteAllSql
        {
            get; private set;
        }

        public string SelectAllSql
        {
            get; private set;
        }

        public string SelectSql
        {
            get; private set;
        }

        public string CountSql
        {
            get; private set;
        }

        public bool IsIdentity
        {
            get; private set;
        }

        public CommandType CommandType
        {
            get { return CommandType.StoredProcedure; }
        }

        public StorageProcedureSqlGenerator(
            string insertSql,
            string updateSql, 
            string deleteSql, 
            string deleteAllSql, 
            string selectSql, 
            string selectAllSql, 
            string countSql, 
            bool isIdentity)
        {

        }
    }
}
