namespace WinLockTimer.Data;

using System;
using System.IO;
using System.Data.SQLite;

public static class DatabaseManager
{
    private static readonly string DbFileName = "WinLockTimer.db";
    private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbFileName);
    private static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

    public static void InitializeDatabase()
    {
        try
        {
            // Ensure the file exists
            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string createAccountsTable = @"
                    CREATE TABLE IF NOT EXISTS Accounts (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL,
                        AvatarPath TEXT,
                        PasswordHash TEXT,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";

                string createRecordsTable = @"
                    CREATE TABLE IF NOT EXISTS TimerRecords (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AccountId INTEGER,
                        StartTime DATETIME NOT NULL,
                        EndTime DATETIME NOT NULL,
                        DurationSeconds REAL,
                        FOREIGN KEY(AccountId) REFERENCES Accounts(Id)
                    );";

                using (var command = new SQLiteCommand(createAccountsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createRecordsTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            // Log failure but don't crash the app
            System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
        }
    }

    public static SQLiteConnection GetConnection()
    {
        return new SQLiteConnection(ConnectionString);
    }
    
    public static bool IsDatabaseAvailable()
    {
         return File.Exists(DbPath);
    }
}
