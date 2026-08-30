IF OBJECT_ID('dbo.Todo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Todo
    (
        Id INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Todo PRIMARY KEY,

        Title NVARCHAR(30) NOT NULL,

        Description NVARCHAR(4000) NULL,

        Completed BIT NOT NULL
            CONSTRAINT DF_Todo_Completed DEFAULT 0,

        DueDate DATETIME2 NULL
    );
END
GO
