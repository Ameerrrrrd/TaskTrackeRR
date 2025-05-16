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
            Server = "192.168.199.240",
            UserID = "root",
            Password = "root",
            Database = "troll",
        };

        public static async Task InsertTaskInDB (int currentUserId, string taskName, string taskDescription, string taskDueDate, string taskDifficulty, string taskStoryPoints)
        {
            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();
            
            try
            {
                var insertCommand = new MySqlCommand(@"
                INSERT INTO user_tasks (user_id, name, description, dueDate, difficulty, storyPoints)
                VALUES (@id, @name, @desc, @date, @diff, @sp);", conn);

                insertCommand.Parameters.AddWithValue("@id", currentUserId);
                insertCommand.Parameters.AddWithValue("@name", taskName);
                insertCommand.Parameters.AddWithValue("@desc", taskDescription);
                insertCommand.Parameters.AddWithValue("@date", taskDueDate);
                insertCommand.Parameters.AddWithValue("@diff", taskDifficulty);
                insertCommand.Parameters.AddWithValue("@sp", taskStoryPoints.ToString());

                await insertCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА!!! {ex.Message}");
            }

        }

        public static async Task InsertTaskByEntry (int currentUserId, string newTask)
        {
            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();

            try
            {
                var inserttask = new MySqlCommand(@"
                                                    INSERT INTO user_tasks (user_id, name) 
                                                    VALUES (@id, @name);", conn);
                inserttask.Parameters.AddWithValue("@id", currentUserId);
                inserttask.Parameters.AddWithValue("@name", newTask);

                await inserttask.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRORRR!! {ex.Message}");
            }
        }

        public static async Task<List<TaskModel>> ShowUserTasks (int userId)
        {
            var tasks = new List<TaskModel>();

            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();

            var selectCommand = new MySqlCommand(
                "SELECT task_id, name, description FROM user_tasks WHERE user_id = @id", conn);
            selectCommand.Parameters.AddWithValue("@id", userId);

            using var reader = await selectCommand.ExecuteReaderAsync();
            int idOrdinal = reader.GetOrdinal("task_id");
            int nameOrdinal = reader.GetOrdinal("name");
            int descOrdinal = reader.GetOrdinal("description");

            while (await reader.ReadAsync())
            {
                var task = new TaskModel
                {
                    TaskId = reader.IsDBNull(idOrdinal) ? 0 : reader.GetInt32(idOrdinal),
                    Name = reader.IsDBNull(nameOrdinal) ? string.Empty : reader.GetString(nameOrdinal),
                    Description = TextTruncator.Truncate(
                        reader.IsDBNull(descOrdinal) ? string.Empty : reader.GetString(descOrdinal), 25)
                };

                tasks.Add(task);
            }

            return tasks;
        }
        public static async Task DeleteTasksByTaskIdAsync(int taskId)
        {
            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();

            try
            {
                var deleteCommand = new MySqlCommand("DELETE FROM user_tasks WHERE task_id = @taskId", conn);
                deleteCommand.Parameters.AddWithValue("@taskId", taskId);

                await deleteCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error due deleting task: {ex.Message}");
            }
        }
    }
    public static class TextTruncator
    {
        public static string Truncate(string input, int maxLength = 30)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Length <= maxLength ? input : input.Substring(0, maxLength) + "...";
        }
    }
    public class TaskModel
    {
        public int TaskId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public static class SelectedTaskContext
    {
        public static int TaskId { get; set; }
        public static string TaskName { get; set; } = string.Empty;
        public static string TaskDescription { get; set; } = string.Empty;
        public static string TaskDueDate { get; set; } = string.Empty;
        public static string TaskDifficulty { get; set; } = string.Empty;
        public static string TaskStoryPoints { get; set; } = string.Empty;
    }
}
