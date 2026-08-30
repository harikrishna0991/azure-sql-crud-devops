IF NOT EXISTS (
    SELECT 1
    FROM sys.database_principals
    WHERE name = N'azurecrud-dev-app'
)
BEGIN
    CREATE USER [azurecrud-dev-app] FROM EXTERNAL PROVIDER;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members AS drm
    INNER JOIN sys.database_principals AS rp
        ON drm.role_principal_id = rp.principal_id
    INNER JOIN sys.database_principals AS up
        ON drm.member_principal_id = up.principal_id
    WHERE rp.name = N'db_datareader'
      AND up.name = N'azurecrud-dev-app'
)
BEGIN
    ALTER ROLE db_datareader ADD MEMBER [azurecrud-dev-app];
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members AS drm
    INNER JOIN sys.database_principals AS rp
        ON drm.role_principal_id = rp.principal_id
    INNER JOIN sys.database_principals AS up
        ON drm.member_principal_id = up.principal_id
    WHERE rp.name = N'db_datawriter'
      AND up.name = N'azurecrud-dev-app'
)
BEGIN
    ALTER ROLE db_datawriter ADD MEMBER [azurecrud-dev-app];
END
GO

IF OBJECT_ID(N'dbo.Todo', N'U') IS NULL
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
