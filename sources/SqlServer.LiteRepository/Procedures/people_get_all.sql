CREATE PROCEDURE [dbo].[people_get_all]
AS
	SELECT id as Id, first_name as FirstName, second_name as SecondName, birthday as Birthday FROM people;
