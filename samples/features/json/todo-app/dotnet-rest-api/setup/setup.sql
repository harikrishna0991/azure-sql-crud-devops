-- Azure SQL schema for the Todo API.
-- This script creates only the application table.
-- Authentication is handled separately through Microsoft Entra ID.

IF OBJECT_ID(N'dbo.Todo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Todo
    (
        Id INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Todo PRIMARY KEY,
        Title NVARCHAR(30) NOT NULL,
        Description NVARCHAR(4000) NULL,
        Completed BIT NOT NULL
            CONSTRAINT DF_Todo_Completed DEFAULT (0),
        DueDate DATETIME2 NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Todo)
BEGIN
    INSERT INTO dbo.Todo (Title, Description, Completed, DueDate)
    VALUES
        (N'First Todo', N'Created for Azure SQL CRUD validation', 0, DATEADD(DAY, 1, SYSUTCDATETIME()));
END;
GO
