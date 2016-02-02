CREATE TABLE [dbo].[users]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY(0, 1), 
    [first_name] NVARCHAR(80) NOT NULL, 
    [second_name] NVARCHAR(80) NOT NULL, 
    [birthday] DATE NOT NULL 
)
