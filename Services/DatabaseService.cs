using System.Text.Json;
using Microsoft.Extensions.Options;
using Npgsql;

public class PostgreSqlService
{
    private readonly string _connectionString;

    // Inject settings via constructor
    public PostgreSqlService(IOptions<PostgreSqlSettings> settings)
    {
        _connectionString = settings.Value.PostgresDb;
        Console.WriteLine($"PostgreSQL Connection String: {_connectionString}"); // Log or debug the connection string
    }

    public async Task<object> ExecuteQueryAsync(string query, Dictionary<string, object> parameters = null, bool isScalar = false)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        AddParameters(command, parameters);

        if (isScalar)
        {
            // For scalar results (e.g., COUNT, MAX)
            return await command.ExecuteScalarAsync();
        }

        // For queries that return multiple rows
        var results = new List<Dictionary<string, object>>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i);
            }
            results.Add(row);
        }

        return results;
    }

}


