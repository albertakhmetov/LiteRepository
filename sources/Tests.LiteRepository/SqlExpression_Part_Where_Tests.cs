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
using LiteRepository.Models;

namespace LiteRepository
{
    public class SqlExpression_Part_Where_Tests
    {
        [Fact]
        public void NullExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);

            exp.GetSelectSql(where: null);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), string.Empty, string.Empty);
        }

        [Fact]
        public void String_EqExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name = 'Ivan'";

            exp.GetSelectSql(where: e => e.FirstName == "Ivan");
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void String_NotEqExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name <> 'Ivan'";

            exp.GetSelectSql(where: e => e.FirstName != "Ivan");
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void String_ToLowerExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "lower(first_name) = 'ivan'";

            exp.GetSelectSql(where: e => e.FirstName.ToLower() == "ivan");
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void String_ToUpperExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "upper(first_name) = 'IVAN'";

            exp.GetSelectSql(where: e => e.FirstName.ToUpper() == "IVAN");
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void String_ToUpper_Parameter_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("Name").Returns("%Name");
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name = upper(%Name)";

            var p = new { Name = "" };

            exp.GetSelectSql(where: e => e.FirstName == p.Name.ToUpper(), param:p);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void String_StartsExpression_Constant_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name like 'Iv%'";

            exp.GetSelectSql(where: e => e.FirstName.StartsWith("Iv"));
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void String_StartsExpression_Parameter_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("name").Returns("%name");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name like %name";

            var p = new { name = "" };

            exp.GetSelectSql(where: e => e.FirstName.StartsWith(p.name), param: p);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void String_EndsExpression_Constant_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name like '%ov'";

            exp.GetSelectSql(where: e => e.FirstName.EndsWith("ov"));
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void String_EndsExpression_Parameter_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("name").Returns("%name");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name like %name";

            var p = new { name = "" };

            exp.GetSelectSql(where: e => e.FirstName.EndsWith(p.name), param: p);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void String_ContainsExpression_Constant_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name like '%van%'";

            exp.GetSelectSql(where: e => e.FirstName.Contains("van"));
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void String_ContainsExpression_Parameter_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("name").Returns("%name");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "first_name like %name";

            var p = new { name = "" };

            exp.GetSelectSql(where: e => e.FirstName.Contains(p.name), param: p);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void DateTime_EqExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "birthday = '1991-12-03 00:00:00'";

            exp.GetSelectSql(where: e => e.Birthday == new DateTime(1991, 12, 3));
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void DateTime_EqExpression_Parameter_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("date").Returns("%date");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "birthday = %date";

            var p = new { date = new DateTime(1991, 12, 3) };

            exp.GetSelectSql(where: e => e.Birthday == p.date, param: p);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Int_EqExpression_Parameter_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("cr").Returns("%cr");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = %cr";

            var p = new { cr = 4L };

            exp.GetSelectSql(where: e => e.Cource == p.cr, param: p);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Int_EqExpression_Parameter_Convert_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("cr").Returns("%cr");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = %cr";

            var p = new { cr = 4 };

            exp.GetSelectSql(where: e => e.Cource == p.cr, param: p);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Int_EqExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = 3";

            exp.GetSelectSql(where: e => e.Cource == 3);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Int_NotEqExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource <> 3";

            exp.GetSelectSql(where: e => e.Cource != 3);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Int_LessExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource < 3";

            exp.GetSelectSql(where: e => e.Cource < 3);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Int_LessOrEqExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource <= 3";

            exp.GetSelectSql(where: e => e.Cource <= 3);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Int_GreaterExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource > 3";

            exp.GetSelectSql(where: e => e.Cource > 3);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Int_GreaterOrEqExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource >= 3";

            exp.GetSelectSql(where: e => e.Cource >= 3);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Decimal_EqExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = 12.2";

            exp.GetSelectSql(where: e => e.Cource == 12.2m);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Int_NotExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "NOT cource = 12";

            exp.GetSelectSql(where: e => !(e.Cource == 12));
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Int_EqExpression_Reverse_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "12 = cource";

            exp.GetSelectSql(where: e => 12 == e.Cource);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Char_EqExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "letter = 'A'";

            exp.GetSelectSql(where: e => e.Letter == 'A');
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Char_EqExpression_Reverse_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "'A' = letter";

            exp.GetSelectSql(where: e => 'A' == e.Letter);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        #region Compilation

        [Fact]
        public void Compilation_MethodCall_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "birthday = '2006-06-06 00:00:00'";

            exp.GetSelectSql(where: i => i.Birthday == DateTime.Parse("2006-06-06"));
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Compilation_EvalVar_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            dialect.Parameter("p").Returns("%p");

            var exp = new SqlExpression<Entity>(dialect);
            var expected = "birthday = '2006-06-06 00:00:00'";

            var p = DateTime.Parse("2006-06-06");
            var param = new { p = DateTime.Parse("2006-01-01") };

            exp.GetSelectSql(where: i => i.Birthday == p, param: param);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Compilation_EvalVar_NoParam_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "birthday = '2006-06-06 00:00:00'";

            var p = DateTime.Parse("2006-06-06");

            exp.GetSelectSql(where: i => i.Birthday == p);
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        #endregion

        #region Group (AND, OR)

        [Fact]
        public void Group_AndExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = 1 AND letter = 'A'";

            exp.GetSelectSql(where: e => e.Cource == 1 && e.Letter == 'A');
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Group_OrExpression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = 1 OR letter = 'A'";

            exp.GetSelectSql(where: e => e.Cource == 1 || e.Letter == 'A');
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Group_TwoAnd_Expression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = 1 AND letter = 'A' AND second_name = 'Ivan'";

            exp.GetSelectSql(where: e => e.Cource == 1 && e.Letter == 'A' && e.SecondName == "Ivan");
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Group_TwoOr_Expression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = 1 OR letter = 'A' OR second_name = 'Ivan'";

            exp.GetSelectSql(where: e => e.Cource == 1 || e.Letter == 'A' || e.SecondName == "Ivan");
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Group_ExpOrAndGroup_Expression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = 1 OR (letter = 'A' AND second_name = 'Ivan')";

            exp.GetSelectSql(where: e => e.Cource == 1 || (e.Letter == 'A' && e.SecondName == "Ivan"));
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Group_ExpAndOrGroup_Expression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "cource = 1 AND (letter = 'A' OR second_name = 'Ivan')";

            exp.GetSelectSql(where: e => e.Cource == 1 && (e.Letter == 'A' || e.SecondName == "Ivan"));
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Group_AndGroupOrExp_Expression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "(cource = 1 AND letter = 'A') OR second_name = 'Ivan'";

            exp.GetSelectSql(where: e => (e.Cource == 1 && e.Letter == 'A') || e.SecondName == "Ivan");
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Group_OrGroupAndExp_Expression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "(cource = 1 OR letter = 'A') AND second_name = 'Ivan'";

            exp.GetSelectSql(where: e => (e.Cource == 1 || e.Letter == 'A') && e.SecondName == "Ivan");
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        [Fact]
        public void Group_Complex_Expression_Test()
        {
            var dialect = Substitute.For<SqlDialectBase>();
            var exp = new SqlExpression<Entity>(dialect);
            var expected = "(cource = 1 OR cource = 2 OR letter = 'A') AND (second_name = 'Ivan' OR (second_name = 'Petrov' AND first_name like 'P%'))";

            exp.GetSelectSql(where: e => (
                (e.Cource == 1 || e.Cource == 2) || e.Letter == 'A') &&
                (e.SecondName == "Ivan" || (e.SecondName == "Petrov" && e.FirstName.StartsWith("P"))
                ));
            dialect.Received(1).Select(Arg.Any<string>(), Arg.Any<string>(), expected, string.Empty);
        }

        #endregion
    }
}