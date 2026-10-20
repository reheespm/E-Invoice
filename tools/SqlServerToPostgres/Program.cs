// Copies every row from the old SQL Server database into the new PostgreSQL (Supabase) database.
//
// 1. Create the schema in PostgreSQL first:
//      dotnet ef database update --project E-Invoice.Infrastructure --startup-project E-Invoice.Web --connection "<postgres>"
// 2. Run this tool (target tables must be empty):
//      dotnet run --project tools/SqlServerToPostgres -- "<sql server>" "<postgres>"
//    or set SOURCE_DB / TARGET_DB environment variables and run without arguments.

using Microsoft.Data.SqlClient;
using Npgsql;

var source = args.ElementAtOrDefault(0) ?? Environment.GetEnvironmentVariable("SOURCE_DB")
    ?? "Server=.\\SQLEXPRESS;Database=EInvoice;Trusted_Connection=True;TrustServerCertificate=True";
var target = args.ElementAtOrDefault(1) ?? Environment.GetEnvironmentVariable("TARGET_DB");

if (string.IsNullOrWhiteSpace(target))
{
    Console.Error.WriteLine("Usage: dotnet run -- \"<sql server connection>\" \"<postgres connection>\"  (or set TARGET_DB)");
    return 1;
}

// Parents before children so foreign keys are satisfied.
string[] tables =
[
    "Countries", "Cities", "Companies", "Customers",
    "Roles", "Users", "RoleClaims", "UserRoles", "UserClaims", "UserLogins", "UserTokens",
    "UserCompanies", "UserDashboardWidgets", "UserQuickLinks", "DataProtectionKeys",
];

await using var sql = new SqlConnection(source);
await sql.OpenAsync();
await using var pg = new NpgsqlConnection(target);
await pg.OpenAsync();

foreach (var table in tables)
{
    await using var check = new NpgsqlCommand($"SELECT COUNT(*) FROM \"{table}\"", pg);
    if ((long)(await check.ExecuteScalarAsync())! > 0)
    {
        Console.Error.WriteLine($"Target table \"{table}\" already has rows. Use an empty database (run the migration only, no seeding).");
        return 1;
    }
}

await using var tx = await pg.BeginTransactionAsync();

foreach (var table in tables)
{
    var targetColumns = new List<string>();
    var identityColumns = new List<string>();
    await using (var cols = new NpgsqlCommand(
        "SELECT column_name, is_identity FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = @t", pg, tx))
    {
        cols.Parameters.AddWithValue("t", table);
        await using var r = await cols.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            targetColumns.Add(r.GetString(0));
            if (r.GetString(1) == "YES") identityColumns.Add(r.GetString(0));
        }
    }

    await using var select = new SqlCommand($"SELECT * FROM [{table}]", sql);
    await using var reader = await select.ExecuteReaderAsync();

    // Columns the current model no longer has (e.g. removed from the entities without a SQL Server migration) are skipped.
    var sourceColumns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
    var columns = sourceColumns.Intersect(targetColumns).ToList();
    var skipped = sourceColumns.Except(targetColumns).ToList();
    if (skipped.Count > 0)
    {
        Console.WriteLine($"\"{table}\": skipping columns not in PostgreSQL: {string.Join(", ", skipped)}");
    }

    var insertSql = $"INSERT INTO \"{table}\" ({string.Join(", ", columns.Select(c => $"\"{c}\""))}) " +
                    $"VALUES ({string.Join(", ", columns.Select((_, i) => $"@p{i}"))})";
    var count = 0;

    while (await reader.ReadAsync())
    {
        await using var insert = new NpgsqlCommand(insertSql, pg, tx);
        for (var i = 0; i < columns.Count; i++)
        {
            insert.Parameters.AddWithValue($"p{i}", reader.GetValue(reader.GetOrdinal(columns[i])) switch
            {
                // SQL Server returns Kind=Unspecified; values are stored as UTC and Npgsql requires Kind=Utc.
                DateTime d => DateTime.SpecifyKind(d, DateTimeKind.Utc),
                DateTimeOffset o => o.ToUniversalTime(),
                var v => v,
            });
        }
        await insert.ExecuteNonQueryAsync();
        count++;
    }

    // Move identity sequences past the copied ids so new rows don't collide.
    foreach (var column in identityColumns)
    {
        await using var seq = new NpgsqlCommand(
            $"SELECT setval(pg_get_serial_sequence('\"{table}\"', '{column}'), COALESCE((SELECT MAX(\"{column}\") FROM \"{table}\"), 0) + 1, false)",
            pg, tx);
        await seq.ExecuteNonQueryAsync();
    }

    Console.WriteLine($"{table,-22} {count} rows");
}

await tx.CommitAsync();
Console.WriteLine("Done.");
return 0;
