using Microsoft.Data.Sqlite;
using Storyforge.Models;

namespace Storyforge.Services;

public class SqliteStoryRepository : IStoryRepository
{
    private readonly string _connectionString;

    public SqliteStoryRepository(DatabaseSettings settings)
    {
        var dir = System.IO.Path.GetDirectoryName(settings.Path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        _connectionString = $"Data Source={settings.Path}";
    }

    public async Task InitDatabaseAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Stories (
                Id TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                Badge TEXT NOT NULL DEFAULT '',
                Paragraphs TEXT NOT NULL DEFAULT '[]',
                CreatedAt TEXT NOT NULL
            )
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SaveAsync(Story story)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Stories (Id, Title, Badge, Paragraphs, CreatedAt)
            VALUES ($id, $title, $badge, $paragraphs, $createdAt)
            """;
        cmd.Parameters.AddWithValue("$id", story.Id.ToString());
        cmd.Parameters.AddWithValue("$title", story.Title);
        cmd.Parameters.AddWithValue("$badge", story.Badge);
        cmd.Parameters.AddWithValue("$paragraphs", story.SerializeParagraphs());
        cmd.Parameters.AddWithValue("$createdAt", story.CreatedAt.ToString("O"));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<StorySummary>> GetAllAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Badge FROM Stories ORDER BY CreatedAt DESC";

        var summaries = new List<StorySummary>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            summaries.Add(new StorySummary(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1)));
        }

        return summaries;
    }

    public async Task<Story?> GetByIdAsync(Guid id)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Title, Badge, Paragraphs, CreatedAt FROM Stories WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id.ToString());

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Story
            {
                Id = Guid.Parse(reader.GetString(0)),
                Title = reader.GetString(1),
                Badge = reader.GetString(2),
                Paragraphs = Story.DeserializeParagraphs(reader.GetString(3)),
                CreatedAt = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind)
            };
        }

        return null;
    }
}
