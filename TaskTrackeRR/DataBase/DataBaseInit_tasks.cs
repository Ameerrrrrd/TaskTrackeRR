using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using MySqlConnector;

namespace TaskTrackeRR
{
    public class DataBaseInit_tasks
    {
        private static readonly MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
        {
            Server = "192.168.0.19",
            UserID = "root",
            Password = "root",
            Database = "troll",
        };

        public static async Task InsertTaskInDB (int currentUserId, string taskName, string taskDescription, string taskDueDate, string taskDifficulty, string taskStoryPoints)
        {
            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();
            // Вставка задачи
            using var transaction = await conn.BeginTransactionAsync();
            try
            {
                var insertCommand = new MySqlCommand(@"
                INSERT INTO user_tasks (user_id, name, description, dueDate, difficulty, storyPoints)
                VALUES (@id, @name, @desc, @date, @diff, @sp);", conn, (MySqlTransaction)transaction);

                insertCommand.Parameters.AddWithValue("@id", currentUserId);
                insertCommand.Parameters.AddWithValue("@name", taskName);
                insertCommand.Parameters.AddWithValue("@desc", taskDescription);
                insertCommand.Parameters.AddWithValue("@date", taskDueDate);
                insertCommand.Parameters.AddWithValue("@diff", taskDifficulty);
                insertCommand.Parameters.AddWithValue("@sp", taskStoryPoints.ToString());

                await insertCommand.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА!!! {ex.Message}");
                await transaction.RollbackAsync();
            }

        }
    }
}
