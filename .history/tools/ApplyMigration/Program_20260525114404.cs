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
            command.CommandText = statement;
            await command.ExecuteNonQueryAsync();
            successCount++;
        }
        catch (SqliteException ex) when (ex.Message.Contains("already exists") || ex.Message.Contains("duplicate column name"))
        {
            // Table or column already exists, skip
            skipCount++;
        }
    }
    
    Console.WriteLine($"[OK] Applied {successCount} statements, skipped {skipCount} existing items");
    Console.WriteLine();
    
    // Verify tables
    Console.WriteLine("[Step 2] Verifying new tables...");
    
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
