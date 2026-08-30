# Todo REST API

Small ASP.NET Core 8 CRUD API based on the Microsoft SQL Server Samples Todo API.

## Endpoints

- `GET /api/Todo`
- `GET /api/Todo/{id}`
- `POST /api/Todo`
- `PUT /api/Todo/{id}`
- `PATCH /api/Todo/{id}`
- `DELETE /api/Todo/{id}`
- `GET /health`

## Azure SQL authentication

The application does not contain a SQL username or password.

The connection string uses Microsoft Entra authentication:

```text
Authentication=Active Directory Default
```

For Azure App Service, the App Service system-assigned managed identity supplies the Azure SQL token.

Set these environment variables:

```text
AZURE_SQL_SERVER=<server>.database.windows.net
AZURE_SQL_DATABASE=<database-name>
```

For local development, authenticate with Azure CLI (`az login`) or another credential supported by `DefaultAzureCredential`.

## Database

Run `setup/setup.sql` against the target Azure SQL database to create the `dbo.Todo` table.

## Run

```powershell
dotnet restore
dotnet build
dotnet run
```

Swagger is available in Development at `/swagger`.
