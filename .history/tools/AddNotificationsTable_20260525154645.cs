using Microsoft.Data.Sqlite;

var dbPath = @"C:\me_workspace\App_Data\me_workspace.db";
var sql = File.ReadAllText(@"C:\me_workspace\App_Data\add-notifications-table.sql");

using var conn = new SqliteConnection($"Data Source={dbPath}");
conn.Open();

using var cmd = conn.CreateCommand();
cmd.CommandText = sql;
cmd.ExecuteNonQuery();

Console.WriteLine("✅ ProcessingNotifications table created successfully!");
