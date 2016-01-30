CREATE PROCEDURE [dbo].[people_get]
	@Id INT
AS
	SELECT id as Id, first_name as FirstName, second_name as SecondName, birthday as Birthday FROM people WHERE id = @Id;