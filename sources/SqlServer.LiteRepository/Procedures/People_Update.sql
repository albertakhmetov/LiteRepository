-- Insert or Update entity
CREATE PROCEDURE [dbo].[UpdatePeople]
	@Id			INT, 
    @FirstName	NVARCHAR, 
    @SecondName	NVARCHAR, 
    @Birthday	DATE 
AS
	UPDATE people 
	SET first_name = @FirstName, second_name = @SecondName, birthday = @Birthday
	WHERE id = @Id;
