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

using Xunit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using LiteRepository.Sql.Models;

namespace LiteRepository.Sql
{
    public class SqlExpression_Part_Where_Tests
    {
        [Fact]
        public void NullExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(null);
            Assert.Equal(string.Empty, sql);
        }

        [Fact]
        public void String_EqExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.FirstName == "Ivan");
            Assert.Equal(" WHERE first_name = 'Ivan'", sql);
        }

        [Fact]
        public void String_NotEqExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.FirstName != "Ivan");
            Assert.Equal(" WHERE first_name <> 'Ivan'", sql);
        }

        [Fact]
        public void String_ToLowerExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.FirstName.ToLower() == "ivan");
            Assert.Equal(" WHERE lower(first_name) = 'ivan'", sql);
        }

        [Fact]
        public void String_ToUpperExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.FirstName.ToUpper() == "IVAN");
            Assert.Equal(" WHERE upper(first_name) = 'IVAN'", sql);
        }

        [Fact]
        public void String_StartsExpression_Constant_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.FirstName.StartsWith("Iv"));
            Assert.Equal(" WHERE first_name like 'Iv%'", sql);
        }

        [Fact]
        public void String_StartsExpression_Parameter_Test()
        {
            var name = "Ivan";

            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.FirstName.StartsWith(name));
            Assert.Equal(" WHERE first_name like @name", sql);
        }

        [Fact]
        public void String_EndsExpression_Constant_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.FirstName.EndsWith("ov"));
            Assert.Equal(" WHERE first_name like '%ov'", sql);
        }

        [Fact]
        public void String_EndsExpression_Parameter_Test()
        {
            var name = "Ivan";

            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.FirstName.EndsWith(name));
            Assert.Equal(" WHERE first_name like @name", sql);
        }

        [Fact]
        public void String_ContainsExpression_Constant_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.FirstName.Contains("van"));
            Assert.Equal(" WHERE first_name like '%van%'", sql);
        }

        [Fact]
        public void String_ContainsExpression_Parameter_Test()
        {
            var name = "Ivan";

            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.FirstName.Contains(name));
            Assert.Equal(" WHERE first_name like @name", sql);
        }

        [Fact]
        public void DateTime_EqExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Birthday == new DateTime(1991, 12, 3));
            Assert.Equal(" WHERE birthday = '1991-12-03 00:00:00'", sql);
        }

        [Fact]
        public void DateTime_EqExpression_Parameter_Test()
        {
            var date = new DateTime(1991, 12, 3);

            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Birthday == date);
            Assert.Equal(" WHERE birthday = @date", sql);
        }

        [Fact]
        public void Int_EqExpression_Parameter_Test()
        {
            var cr = 4L;

            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource == cr);
            Assert.Equal(" WHERE cource = @cr", sql);
        }

        [Fact]
        public void Int_EqExpression_Parameter_Convert_Test()
        {
            var cr = 4;

            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource == cr);
            Assert.Equal(" WHERE cource = @cr", sql);
        }

        [Fact]
        public void Int_EqExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource == 3);
            Assert.Equal(" WHERE cource = 3", sql);
        }

        [Fact]
        public void Int_NotEqExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource != 3);
            Assert.Equal(" WHERE cource <> 3", sql);
        }

        [Fact]
        public void Int_LessExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource < 3);
            Assert.Equal(" WHERE cource < 3", sql);
        }

        [Fact]
        public void Int_LessOrEqExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource <= 3);
            Assert.Equal(" WHERE cource <= 3", sql);
        }

        [Fact]
        public void Int_GreaterExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource > 3);
            Assert.Equal(" WHERE cource > 3", sql);
        }

        [Fact]
        public void Int_GreaterOrEqExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource >= 3);
            Assert.Equal(" WHERE cource >= 3", sql);
        }

        [Fact]
        public void Decimal_EqExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource == 12.2m);
            Assert.Equal(" WHERE cource = 12.2", sql);
        }

        [Fact]
        public void Int_NotExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => !(e.Cource == 12));
            Assert.Equal(" WHERE NOT cource = 12", sql);
        }

        [Fact]
        public void Int_EqExpression_Reverse_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => 12 == e.Cource);
            Assert.Equal(" WHERE 12 = cource", sql);
        }

        [Fact]
        public void Char_EqExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Letter == 'A');
            Assert.Equal(" WHERE letter = 'A'", sql);
        }

        [Fact]
        public void Char_EqExpression_Reverse_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => 'A' == e.Letter);
            Assert.Equal(" WHERE 'A' = letter", sql);
        }

        [Fact]
        public void Group_AndExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource == 1 && e.Letter == 'A');
            Assert.Equal(" WHERE cource = 1 AND letter = 'A'", sql);
        }

        [Fact]
        public void Group_OrExpression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource == 1 || e.Letter == 'A');
            Assert.Equal(" WHERE cource = 1 OR letter = 'A'", sql);
        }

        [Fact]
        public void Group_TwoAnd_Expression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource == 1 && e.Letter == 'A' && e.SecondName == "Ivan");
            Assert.Equal(" WHERE cource = 1 AND letter = 'A' AND second_name = 'Ivan'", sql);
        }

        [Fact]
        public void Group_TwoOr_Expression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource == 1 || e.Letter == 'A' || e.SecondName == "Ivan");
            Assert.Equal(" WHERE cource = 1 OR letter = 'A' OR second_name = 'Ivan'", sql);
        }

        [Fact]
        public void Group_ExpOrAndGroup_Expression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource == 1 || (e.Letter == 'A' && e.SecondName == "Ivan"));
            Assert.Equal(" WHERE cource = 1 OR (letter = 'A' AND second_name = 'Ivan')", sql);
        }

        [Fact]
        public void Group_ExpAndOrGroup_Expression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => e.Cource == 1 && (e.Letter == 'A' || e.SecondName == "Ivan"));
            Assert.Equal(" WHERE cource = 1 AND (letter = 'A' OR second_name = 'Ivan')", sql);
        }

        [Fact]
        public void Group_AndGroupOrExp_Expression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => (e.Cource == 1 && e.Letter == 'A') || e.SecondName == "Ivan");
            Assert.Equal(" WHERE (cource = 1 AND letter = 'A') OR second_name = 'Ivan'", sql);
        }

        [Fact]
        public void Group_OrGroupAndExp_Expression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => (e.Cource == 1 || e.Letter == 'A') && e.SecondName == "Ivan");
            Assert.Equal(" WHERE (cource = 1 OR letter = 'A') AND second_name = 'Ivan'", sql);
        }

        [Fact]
        public void Group_Complex_Expression_Test()
        {
            var dialect = Substitute.For<ISqlDialect>();
            var exp = new SqlExpression<Entity>(dialect);
            var sql = exp.GetWherePartSql(e => ((e.Cource == 1 || e.Cource == 2) || e.Letter == 'A') && (e.SecondName == "Ivan" || (e.SecondName == "Petrov" && e.FirstName.StartsWith("P"))));
            Assert.Equal(" WHERE (cource = 1 OR cource = 2 OR letter = 'A') AND (second_name = 'Ivan' OR (second_name = 'Petrov' AND first_name like 'P%'))", sql);
        }
    }
}
