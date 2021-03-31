IF OBJECT_ID('Products') > 0 DROP TABLE [Products]
GO
-- Create Products table
CREATE TABLE [Products]
(
	[ProductID] [int] IDENTITY(1,1) NOT NULL ,
	[Name] [nvarchar](50) NOT NULL ,
	[Category] [nvarchar](50) NOT NULL ,
	[Price] [smallmoney] NOT NULL
		CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
	(
		[ProductID]
	)
)
GO
-- Create stored procedures
IF OBJECT_ID('GetProduct') > 0 DROP PROCEDURE [GetProduct]
GO
CREATE PROCEDURE GetProduct
	@ProductID int
AS
SELECT [ProductID], [Name], [Category], [Price]
FROM [Products]
WHERE [ProductID] = @ProductID
GO
IF OBJECT_ID('GetProducts') > 0 DROP PROCEDURE [GetProducts]
GO
CREATE PROCEDURE GetProducts
AS
SELECT [ProductID], [Name], [Category], [Price]
FROM [Products] 
GO
IF OBJECT_ID('GetProductsByCategory') > 0 DROP PROCEDURE [GetProductsByCategory]
GO
CREATE PROCEDURE GetProductsByCategory
	@Category [nvarchar](50)
AS
SELECT [ProductID], [Name], [Category], [Price]
FROM [Products]
WHERE [Category] = @Category
GO
IF OBJECT_ID('AddProduct') > 0 DROP PROCEDURE [AddProduct]
GO
CREATE PROCEDURE AddProduct
	@ProductID int OUTPUT,
	@Name [nvarchar](50),
	@Category [nvarchar](50),
	@Price [smallmoney]
AS
INSERT INTO Products
VALUES
	(@Name, @Category, @Price)
SET @ProductID = @@IDENTITY
GO
IF OBJECT_ID('UpdateProduct') > 0 DROP PROCEDURE [UpdateProduct]
GO
CREATE PROCEDURE UpdateProduct
	@ProductID int,
	@Name [nvarchar](50),
	@Category [nvarchar](50),
	@Price [smallmoney]
AS
UPDATE Products 
SET [Name] = @Name,
    [Category] = @Category,
	[Price] = @Price
WHERE [ProductID] = @ProductID
GO
IF OBJECT_ID('DeleteProduct') > 0 DROP PROCEDURE [DeleteProduct]
GO
CREATE PROCEDURE DeleteProduct
	@ProductID int
AS
DELETE [Products]
WHERE [ProductID] = @ProductID
GO
-- Create test data
SET NOCOUNT ON
GO
INSERT INTO Products
VALUES
	(N'Tomato soup', N'Groceries', 1.39)
GO
INSERT INTO Products
VALUES
	(N'Babo', N'Toys', 19.99)
GO
INSERT INTO Products
VALUES
	(N'Hammer', N'Hardware', 16.49)
GO