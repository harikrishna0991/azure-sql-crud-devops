using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TodoApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController(IConfiguration configuration) : ControllerBase
{
    private readonly string _connectionString = BuildConnectionString(configuration);

    private static readonly TokenCredential SqlCredential =
        new ManagedIdentityCredential();

    private static async Task<SqlConnection> CreateSqlConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(connectionString);

        AccessToken token = await SqlCredential.GetTokenAsync(
            new TokenRequestContext(
                new[] { "https://database.windows.net/.default" }),
            cancellationToken);

        connection.AccessToken = token.Token;

        return connection;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetAll(
        CancellationToken cancellationToken)
    {
        var results = new List<TodoItem>();

        await using var connection =
            await CreateSqlConnectionAsync(_connectionString, cancellationToken);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT Id, Title, Description, Completed, DueDate
            FROM dbo.Todo
            ORDER BY Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(Map(reader));
        }

        return Ok(results);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoItem>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await CreateSqlConnectionAsync(_connectionString, cancellationToken);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT Id, Title, Description, Completed, DueDate
            FROM dbo.Todo
            WHERE Id = @id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return NotFound();
        }

        return Ok(Map(reader));
    }

    [HttpPost]
    public async Task<ActionResult<TodoItem>> Create(
        [FromBody] TodoCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Title is required." });
        }

        await using var connection =
            await CreateSqlConnectionAsync(_connectionString, cancellationToken);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO dbo.Todo
                (Title, Description, Completed, DueDate)
            OUTPUT
                INSERTED.Id,
                INSERTED.Title,
                INSERTED.Description,
                INSERTED.Completed,
                INSERTED.DueDate
            VALUES
                (@title, @description, @completed, @dueDate);
            """;

        await using var command = new SqlCommand(sql, connection);

        AddTodoParameters(
            command,
            request.Title,
            request.Description,
            request.Completed,
            request.DueDate);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);

        var created = Map(reader);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TodoItem>> Update(
        int id,
        [FromBody] TodoUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Title is required." });
        }

        await using var connection =
            await CreateSqlConnectionAsync(_connectionString, cancellationToken);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE dbo.Todo
            SET
                Title = @title,
                Description = @description,
                Completed = @completed,
                DueDate = @dueDate
            OUTPUT
                INSERTED.Id,
                INSERTED.Title,
                INSERTED.Description,
                INSERTED.Completed,
                INSERTED.DueDate
            WHERE Id = @id;
            """;

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

        AddTodoParameters(
            command,
            request.Title,
            request.Description,
            request.Completed,
            request.DueDate);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return NotFound();
        }

        return Ok(Map(reader));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<TodoItem>> Patch(
        int id,
        [FromBody] TodoPatchRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await CreateSqlConnectionAsync(_connectionString, cancellationToken);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE dbo.Todo
            SET
                Title = COALESCE(@title, Title),
                Description = COALESCE(@description, Description),
                Completed = COALESCE(@completed, Completed),
                DueDate = COALESCE(@dueDate, DueDate)
            OUTPUT
                INSERTED.Id,
                INSERTED.Title,
                INSERTED.Description,
                INSERTED.Completed,
                INSERTED.DueDate
            WHERE Id = @id;
            """;

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

        command.Parameters.Add(
            "@title",
            SqlDbType.NVarChar,
            30).Value =
            (object?)request.Title ?? DBNull.Value;

        command.Parameters.Add(
            "@description",
            SqlDbType.NVarChar,
            4000).Value =
            (object?)request.Description ?? DBNull.Value;

        command.Parameters.Add(
            "@completed",
            SqlDbType.Bit).Value =
            (object?)request.Completed ?? DBNull.Value;

        command.Parameters.Add(
            "@dueDate",
            SqlDbType.DateTime2).Value =
            (object?)request.DueDate ?? DBNull.Value;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return NotFound();
        }

        return Ok(Map(reader));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await CreateSqlConnectionAsync(_connectionString, cancellationToken);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            DELETE FROM dbo.Todo
            WHERE Id = @id;
            """;

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

        var rows =
            await command.ExecuteNonQueryAsync(cancellationToken);

        return rows == 0
            ? NotFound()
            : NoContent();
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        var keyVaultConnectionString =
            configuration["AZURE_SQL_CONNECTIONSTRING"]
            ?? Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");

        if (!string.IsNullOrWhiteSpace(keyVaultConnectionString))
        {
            return keyVaultConnectionString;
        }

        var server =
            configuration["AZURE_SQL_SERVER"]
            ?? Environment.GetEnvironmentVariable("AZURE_SQL_SERVER");

        var database =
            configuration["AZURE_SQL_DATABASE"]
            ?? Environment.GetEnvironmentVariable("AZURE_SQL_DATABASE");

        if (string.IsNullOrWhiteSpace(server))
        {
            throw new InvalidOperationException(
                "AZURE_SQL_SERVER is not configured.");
        }

        if (string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException(
                "AZURE_SQL_DATABASE is not configured.");
        }

        return
            $"Server=tcp:{server},1433;" +
            $"Database={database};" +
            "Authentication=Active Directory Managed Identity;" +
            "Encrypt=True;" +
            "TrustServerCertificate=False;";
    }
    private static void AddTodoParameters(
        SqlCommand command,
        string title,
        string? description,
        bool completed,
        DateTime? dueDate)
    {
        command.Parameters.Add(
            "@title",
            SqlDbType.NVarChar,
            30).Value = title;

        command.Parameters.Add(
            "@description",
            SqlDbType.NVarChar,
            4000).Value =
            (object?)description ?? DBNull.Value;

        command.Parameters.Add(
            "@completed",
            SqlDbType.Bit).Value = completed;

        command.Parameters.Add(
            "@dueDate",
            SqlDbType.DateTime2).Value =
            (object?)dueDate ?? DBNull.Value;
    }

    private static TodoItem Map(SqlDataReader reader)
    {
        return new TodoItem(
            reader.GetInt32(reader.GetOrdinal("Id")),
            reader.GetString(reader.GetOrdinal("Title")),
            reader.IsDBNull(reader.GetOrdinal("Description"))
                ? null
                : reader.GetString(reader.GetOrdinal("Description")),
            reader.GetBoolean(reader.GetOrdinal("Completed")),
            reader.IsDBNull(reader.GetOrdinal("DueDate"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("DueDate")));
    }
}

public record TodoItem(
    int Id,
    string Title,
    string? Description,
    bool Completed,
    DateTime? DueDate);

public record TodoCreateRequest(
    string Title,
    string? Description,
    bool Completed = false,
    DateTime? DueDate = null);

public record TodoUpdateRequest(
    string Title,
    string? Description,
    bool Completed,
    DateTime? DueDate);

public record TodoPatchRequest(
    string? Title = null,
    string? Description = null,
    bool? Completed = null,
    DateTime? DueDate = null);
