



CREATE PROCEDURE [dbo].[InsertProduct]
    @Name NVARCHAR(100),
    @Brand NVARCHAR(100),
    @Category NVARCHAR(100),
    @Price DECIMAL(16, 2),
    @Description NVARCHAR(MAX),
    @ImageFileName NVARCHAR(100),
    @CreatedAt DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO MyProperty (Name, Brand, Category, Price, Description, ImageFileName, CreatedAt)
    VALUES (@Name, @Brand, @Category, @Price, @Description, @ImageFileName, @CreatedAt);
END;
