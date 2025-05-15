using System;
using System.Collections.Generic;
using System.Data;
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

        public static async Task InsertTaskByEntry (int currentUserId, string newTask)
        {
            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                var inserttask = new MySqlCommand(@"
                                                    INSERT INTO user_tasks (user_id, name) 
                                                    VALUES (@id, @name);", conn, (MySqlTransaction)transaction);
                inserttask.Parameters.AddWithValue("@id", currentUserId);
                inserttask.Parameters.AddWithValue("@name", newTask);

                await inserttask.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRORRR!! {ex.Message}");
                await transaction.RollbackAsync();
            }
        }

        public static async Task<List<TaskModel>> ShowUserTasks (int userId)
        {
            var tasks = new List<TaskModel>();

            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();
            var selectCommand = new MySqlCommand(
                "SELECT name, description FROM user_tasks WHERE user_id = @id", conn, (MySqlTransaction)transaction);
            selectCommand.Parameters.AddWithValue("@id", userId);

            using var reader = await selectCommand.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string taskName = reader.IsDBNull("name") ? string.Empty : reader.GetString("name");
                string taskDescription = reader.IsDBNull("description") ? string.Empty : reader.GetString("description");

                tasks.Add(new TaskModel
                {
                    Name = taskName,
                    Description = TextTruncator.Truncate(taskDescription)
                });
            }

            return tasks;
        }
    }
    public static class TextTruncator
    {
        public static string Truncate(string input, int maxLength = 25)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Length <= maxLength ? input : input.Substring(0, maxLength) + "...";
        }
    }
    public class TaskModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
