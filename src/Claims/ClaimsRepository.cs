using Microsoft.Data.Sqlite;

namespace Claims;

public class ClaimsRepository
{
    private readonly SqliteConnection _db;

    public ClaimsRepository(SqliteConnection db) => _db = db;

    // Search the claims of one member. Called by the claims page of the member website and by the support tool.
    public IReadOnlyList<Claim> Search(int memberId, string? text, string? sortBy = "date", string? sortDir = "desc", int page = 0)
    {
        // CWE-89: identifiers cannot be parameters. Only these constants are interpolated.
        var column = ResolveSortColumn(sortBy);
        var direction = ResolveSortDirection(sortDir);

        using var cmd = _db.CreateCommand();
        cmd.CommandText =
            "SELECT id, provider, description, amount, status, submitted_at FROM claims " +
            "WHERE member_id = @member AND (description LIKE @text ESCAPE '\\' OR provider LIKE @text ESCAPE '\\') " +
            $"ORDER BY {column} {direction} LIMIT 20 OFFSET @offset";
        cmd.Parameters.AddWithValue("@member", memberId);
        cmd.Parameters.AddWithValue("@text", ToContainsPattern(text));
        cmd.Parameters.AddWithValue("@offset", page * 20);

        var results = new List<Claim>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add(new Claim(reader.GetInt64(0), reader.GetString(1), reader.GetString(2),
                                  reader.GetDouble(3), reader.GetString(4), reader.GetString(5)));
        return results;
    }

    // Null keeps the signature default ("date" / "desc"). Any other value is rejected.
    private static string ResolveSortColumn(string? sortBy)
    {
        var key = sortBy ?? "date";
        if (key.Equals("date", StringComparison.OrdinalIgnoreCase))
            return "submitted_at";
        if (key.Equals("amount", StringComparison.OrdinalIgnoreCase))
            return "amount";
        if (key.Equals("status", StringComparison.OrdinalIgnoreCase))
            return "status";

        throw new ArgumentException("sortBy must be date, amount, or status.", nameof(sortBy));
    }

    private static string ResolveSortDirection(string? sortDir)
    {
        var key = sortDir ?? "desc";
        if (key.Equals("asc", StringComparison.OrdinalIgnoreCase))
            return "ASC";
        if (key.Equals("desc", StringComparison.OrdinalIgnoreCase))
            return "DESC";

        throw new ArgumentException("sortDir must be asc or desc.", nameof(sortDir));
    }

    private static string ToContainsPattern(string? text)
    {
        var value = text ?? string.Empty;
        var escaped = value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
        return "%" + escaped + "%";
    }
}
