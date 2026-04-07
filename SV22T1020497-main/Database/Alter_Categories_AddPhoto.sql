IF COL_LENGTH('dbo.Categories', 'Photo') IS NULL
BEGIN
    ALTER TABLE dbo.Categories
    ADD Photo NVARCHAR(255) NULL;
END
GO
