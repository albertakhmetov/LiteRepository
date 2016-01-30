CREATE PROCEDURE [dbo].[people_update]
	@Id			INT, 
    @FirstName	NVARCHAR, 
    @SecondName	NVARCHAR, 
    @Birthday	DATE 
AS
	UPDATE people 
	SET first_name = @FirstName, second_name = @SecondName, birthday = @Birthday
	WHERE id = @Id;
