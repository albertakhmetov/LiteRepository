CREATE TABLE [dbo].[cash_check_item]
(
	[id] INT NOT NULL PRIMARY KEY, 
    [shop_id] INT NOT NULL, 
    [text] NVARCHAR(50) NOT NULL, 
    [price] DECIMAL(18, 2) NOT NULL
)
