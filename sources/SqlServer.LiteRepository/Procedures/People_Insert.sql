CREATE PROCEDURE [dbo].[people_insert]
    @Id			INT,
	@FirstName	NVARCHAR(80), 
    @SecondName	NVARCHAR(80), 
    @Birthday	DATE 
AS
	INSERT INTO people (first_name, second_name, birthday) VALUES (@FirstName, @SecondName, @Birthday);
	SELECT SCOPE_IDENTITY();
