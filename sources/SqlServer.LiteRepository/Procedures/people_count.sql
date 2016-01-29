CREATE PROCEDURE [dbo].[people_count]
AS
	SELECT count(1) FROM people;