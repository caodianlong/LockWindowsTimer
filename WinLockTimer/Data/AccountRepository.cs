namespace WinLockTimer.Data;

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WinLockTimer.Models;

public class AccountRepository
{
    public List<Account> GetAllAccounts()
    {
        var accounts = new List<Account>();
        if (!DatabaseManager.IsDatabaseAvailable()) return accounts;

        try
        {
            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Accounts ORDER BY Id";
                using (var cmd = new SQLiteCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        accounts.Add(new Account
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Username = reader["Username"].ToString() ?? "",
                            AvatarPath = reader["AvatarPath"].ToString() ?? "",
                            PasswordHash = reader["PasswordHash"].ToString() ?? "",
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAllAccounts failed: {ex.Message}");
        }

        return accounts;
    }

    public void AddAccount(Account account)
    {
        using (var conn = DatabaseManager.GetConnection())
        {
            conn.Open();
            string query = "INSERT INTO Accounts (Username, AvatarPath, PasswordHash, CreatedAt) VALUES (@Username, @AvatarPath, @PasswordHash, @CreatedAt)";
            using (var cmd = new SQLiteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Username", account.Username);
                cmd.Parameters.AddWithValue("@AvatarPath", account.AvatarPath);
                cmd.Parameters.AddWithValue("@PasswordHash", account.PasswordHash);
                cmd.Parameters.AddWithValue("@CreatedAt", account.CreatedAt);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void UpdateAccount(Account account)
    {
        using (var conn = DatabaseManager.GetConnection())
        {
            conn.Open();
            string query = "UPDATE Accounts SET Username = @Username, AvatarPath = @AvatarPath, PasswordHash = @PasswordHash WHERE Id = @Id";
            using (var cmd = new SQLiteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Username", account.Username);
                cmd.Parameters.AddWithValue("@AvatarPath", account.AvatarPath);
                cmd.Parameters.AddWithValue("@PasswordHash", account.PasswordHash);
                cmd.Parameters.AddWithValue("@Id", account.Id);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void DeleteAccount(int id)
    {
        using (var conn = DatabaseManager.GetConnection())
        {
            conn.Open();
            // First delete associated records or handle cascade?
            // For simplicity, we just delete the account. SQLite foreign key might restrict if enforced.
            // Let's manually delete records first to be safe or assuming Cascade Delete isn't set up yet.
            
            string deleteRecords = "DELETE FROM TimerRecords WHERE AccountId = @Id";
            using (var cmd = new SQLiteCommand(deleteRecords, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }

            string query = "DELETE FROM Accounts WHERE Id = @Id";
            using (var cmd = new SQLiteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
    
    public Account? GetAccountById(int id)
    {
         using (var conn = DatabaseManager.GetConnection())
        {
            conn.Open();
            string query = "SELECT * FROM Accounts WHERE Id = @Id";
            using (var cmd = new SQLiteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Account
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Username = reader["Username"].ToString() ?? "",
                            AvatarPath = reader["AvatarPath"].ToString() ?? "",
                            PasswordHash = reader["PasswordHash"].ToString() ?? "",
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        };
                    }
                }
            }
        }
        return null;
    }
}
