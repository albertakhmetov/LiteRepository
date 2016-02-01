CREATE PROCEDURE [dbo].[people_update]
	@Id			INT, 
    @FirstName	NVARCHAR(80), 
    @SecondName	NVARCHAR(80), 
    @Birthday	DATE 
AS
	UPDATE people 
	SET first_name = @FirstName, second_name = @SecondName, birthday = @Birthday
	WHERE id = @Id;
