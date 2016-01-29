CREATE PROCEDURE [dbo].[people_insert]
    @FirstName	NVARCHAR, 
    @SecondName	NVARCHAR, 
    @Birthday	DATE 
AS
	INSERT INTO people (first_name, second_name, birthday) VALUES (@FirstName, @SecondName, @Birthday);
	SELECT SCOPE_IDENTITY();
