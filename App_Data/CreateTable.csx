using System;
using Microsoft.Data.Sqlite;

var connStr = "Data Source=C:\\me_workspace\\App_Data\\me_workspace.db";
using var conn = new SqliteConnection(connStr);
conn.Open();
using var cmd = conn.CreateCommand();
cmd.CommandText = System.IO.File.ReadAllText("C:\\me_workspace\\App_Data\\create_notifications.sql");
cmd.ExecuteNonQuery();
Console.WriteLine("? Table created!");
