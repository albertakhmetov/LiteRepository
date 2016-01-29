CREATE PROCEDURE [dbo].[people_delete]
	@Id INT
AS
	DELETE FROM people WHERE id = @Id;
