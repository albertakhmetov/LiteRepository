CREATE TABLE [dbo].[students]
(
	[cource] BIGINT NOT NULL, 
    [letter] NCHAR(1) NOT NULL, 
    [first_name] NVARCHAR(80) NOT NULL, 
    [second_name] NVARCHAR(80) NOT NULL, 
    [birthday] DATE NOT NULL, 
    CONSTRAINT [PK_students] PRIMARY KEY ([cource], [letter])
)
