using Microsoft.Data.Sqlite;

var dbPath = args.Length > 0 ? args[0] : Path.Combine("..", "..", "App_Data", "me_workspace.db");
var sqlScript = args.Length > 1 ? args[1] : Path.Combine(".", "Phase6-Migration.sql");

dbPath = Path.GetFullPath(dbPath);
sqlScript = Path.GetFullPath(sqlScript);

Console.WriteLine("=== Phase 6 Database Migration ===");
Console.WriteLine();
Console.WriteLine($"Database: {dbPath}");
Console.WriteLine($"Script: {sqlScript}");
Console.WriteLine();

if (!File.Exists(dbPath))
{
    Console.WriteLine("[ERROR] Database not found");
    return 1;
}

if (!File.Exists(sqlScript))
{
    Console.WriteLine("[ERROR] SQL script not found");
    return 1;
}

var sqlContent = await File.ReadAllTextAsync(sqlScript);
var statements = sqlContent.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

var connectionString = $"Data Source={dbPath}";

try
{
    using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();
    
    // Disable foreign key constraints during migration
    using (var cmd = connection.CreateCommand())
    {
        cmd.CommandText = "PRAGMA foreign_keys = OFF;";
        await cmd.ExecuteNonQueryAsync();
    }
    
    Console.WriteLine("[Step 1] Applying migration...");
    
    var successCount = 0;
    var skipCount = 0;
    
    foreach (var statement in statements)
    {
        if (string.IsNullOrWhiteSpace(statement) || statement.StartsWith("--"))
            continue;
        
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = statement + ";";  // Re-add semicolon
            Console.WriteLine($"  Executing: {statement.Substring(0, Math.Min(50, statement.Length))}...");
            await command.ExecuteNonQueryAsync();
            successCount++;
        }
        catch (SqliteException ex) when (ex.Message.Contains("already exists") || ex.Message.Contains("duplicate column name"))
        {
            // Table or column already exists, skip
            skipCount++;
        }
        catch (SqliteException ex)
        {
            Console.WriteLine($"  [ERROR] {ex.Message}");
            Console.WriteLine($"  Statement: {statement.Substring(0, Math.Min(200, statement.Length))}...");
            throw;
        }
    }
    
    Console.WriteLine($"[OK] Applied {successCount} statements, skipped {skipCount} existing items");
    Console.WriteLine();
    
    // Add new columns to Sources table if they don't exist
    Console.WriteLine("[Step 2] Updating Sources table...");
    
    var columnsToAdd = new Dictionary<string, string>
    {
        { "ProjectId", "TEXT NULL" },
        { "Author", "TEXT NULL" },
        { "ISBN", "TEXT NULL" },
        { "URL", "TEXT NULL" },
        { "Publisher", "TEXT NULL" },
        { "PublicationYear", "INTEGER NULL" },
        { "BorrowingSource", "TEXT NULL" },
        { "AccessExpiryUtc", "TEXT NULL" }
    };
    
    foreach (var column in columnsToAdd)
    {
        try
        {
            // Check if column exists
            using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Sources') WHERE name=@name;";
            checkCmd.Parameters.AddWithValue("@name", column.Key);
            var exists = (long)(await checkCmd.ExecuteScalarAsync() ?? 0L) > 0;
            
            if (!exists)
            {
                using var alterCmd = connection.CreateCommand();
                alterCmd.CommandText = $"ALTER TABLE \"Sources\" ADD COLUMN \"{column.Key}\" {column.Value};";
                await alterCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"  [OK] Added column: {column.Key}");
            }
            else
            {
                Console.WriteLine($"  [SKIP] Column already exists: {column.Key}");
            }
        }
        catch (SqliteException ex)
        {
            Console.WriteLine($"  [WARNING] Could not add {column.Key}: {ex.Message}");
        }
    }
    
    // Create indexes on Sources table
    try
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "CREATE INDEX IF NOT EXISTS \"IX_Sources_Status\" ON \"Sources\" (\"Status\");";
        await cmd.ExecuteNonQueryAsync();
        cmd.CommandText = "CREATE INDEX IF NOT EXISTS \"IX_Sources_AccessExpiryUtc\" ON \"Sources\" (\"AccessExpiryUtc\");";
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("  [OK] Indexes created");
    }
    catch (SqliteException ex)
    {
        Console.WriteLine($"  [WARNING] Index creation: {ex.Message}");
    }
    
    Console.WriteLine();
    
    // Verify tables
    Console.WriteLine("[Step 3] Verifying new tables...");
    
    var tablesToCheck = new[] { "Projects", "Summaries", "DocumentAnchors", "ProjectTasks", "AgentLogs", "TrendAnalyses" };
    var allExist = true;
    
    foreach (var table in tablesToCheck)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@name;";
        command.Parameters.AddWithValue("@name", table);
        
        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            Console.WriteLine($"  [OK] {table}");
        }
        else
        {
            Console.WriteLine($"  [ERROR] {table} not found");
            allExist = false;
        }
    }
    
    Console.WriteLine();
    
    // Re-enable foreign key constraints
    using (var cmd = connection.CreateCommand())
    {
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        await cmd.ExecuteNonQueryAsync();
    }
    
    if (allExist)
    {
        Console.WriteLine("[SUCCESS] Phase 6 migration complete!");
        Console.WriteLine();
        Console.WriteLine("New tables: Projects, Summaries, DocumentAnchors, ProjectTasks, AgentLogs, TrendAnalyses");
        Console.WriteLine("Enhanced Sources table with: ProjectId, Author, ISBN, URL, Publisher, etc.");
        return 0;
    }
    else
    {
        Console.WriteLine("[ERROR] Some tables were not created");
        return 1;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] {ex.Message}");
    return 1;
}
