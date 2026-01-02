namespace WinLockTimer.Data;

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WinLockTimer.Models;

public class TimerRecordRepository
{
    public void AddRecord(TimerRecord record)
    {
        if (!DatabaseManager.IsDatabaseAvailable()) return;

        try 
        {
            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO TimerRecords (AccountId, StartTime, EndTime, DurationSeconds) VALUES (@AccountId, @StartTime, @EndTime, @DurationSeconds)";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    if (record.AccountId > 0)
                        cmd.Parameters.AddWithValue("@AccountId", record.AccountId);
                    else
                        cmd.Parameters.AddWithValue("@AccountId", DBNull.Value);

                    cmd.Parameters.AddWithValue("@StartTime", record.StartTime);
                    cmd.Parameters.AddWithValue("@EndTime", record.EndTime);
                    cmd.Parameters.AddWithValue("@DurationSeconds", record.DurationSeconds);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddRecord failed: {ex.Message}");
        }
    }

    public List<TimerRecord> GetRecords(int? accountId, DateTime? start, DateTime? end)
    {
        var records = new List<TimerRecord>();
        if (!DatabaseManager.IsDatabaseAvailable()) return records;

        try
        {
            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                var queryBuilder = new System.Text.StringBuilder("SELECT * FROM TimerRecords WHERE 1=1");
                
                if (accountId.HasValue && accountId.Value > 0)
                {
                    queryBuilder.Append(" AND AccountId = @AccountId");
                }
                if (start.HasValue)
                {
                    queryBuilder.Append(" AND StartTime >= @StartTime");
                }
                if (end.HasValue)
                {
                    queryBuilder.Append(" AND EndTime <= @EndTime");
                }
                
                queryBuilder.Append(" ORDER BY StartTime DESC");

                using (var cmd = new SQLiteCommand(queryBuilder.ToString(), conn))
                {
                    if (accountId.HasValue && accountId.Value > 0) cmd.Parameters.AddWithValue("@AccountId", accountId.Value);
                    if (start.HasValue) cmd.Parameters.AddWithValue("@StartTime", start.Value);
                    if (end.HasValue) cmd.Parameters.AddWithValue("@EndTime", end.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            records.Add(new TimerRecord
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                AccountId = Convert.ToInt32(reader["AccountId"]),
                                StartTime = Convert.ToDateTime(reader["StartTime"]),
                                EndTime = Convert.ToDateTime(reader["EndTime"]),
                                DurationSeconds = Convert.ToDouble(reader["DurationSeconds"])
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetRecords failed: {ex.Message}");
        }

        return records;
    }
}
