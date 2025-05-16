using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using BCrypt.Net;
using System.Diagnostics;
using System.Globalization;

namespace TaskTrackeRR
{
    class DataBaseInit_Users
    {
        private static readonly MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
        {
            Server = "192.168.199.240",
            UserID = "root",
            Password = "root",
            Database = "troll",
        };

        //// REGISTER
        public static async Task<bool> RegisterUserAsync(string login, string email, string password)
        {

            var sw = Stopwatch.StartNew();
            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();
            sw.Stop();
            Console.WriteLine($"Connect time: {sw.ElapsedMilliseconds}ms");

            using var transaction = await conn.BeginTransactionAsync();
            try
            {
                var userCmd = new MySqlCommand(@"
                                                INSERT INTO user_login (login, email)
                                                VALUES (@login, @email);
                                                SELECT LAST_INSERT_ID();", conn, (MySqlTransaction)transaction);
                userCmd.Parameters.AddWithValue("@login", login);
                userCmd.Parameters.AddWithValue("@email", email);

                sw.Restart();
                var userId = Convert.ToInt32(await userCmd.ExecuteScalarAsync());
                Console.WriteLine($"[STEP] ExecuteScalarAsync: {sw.ElapsedMilliseconds}ms");

                sw.Restart();
                string salt = BCrypt.Net.BCrypt.GenerateSalt(8); 
                string hashedPassword = await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(password, salt));
                Console.WriteLine($"[STEP] Hashing: {sw.ElapsedMilliseconds}ms");

                var passCmd = new MySqlCommand(@"
                                                INSERT INTO user_pwd (user_id, password_hash, salt)
                                                VALUES (@user_id, @hashedPassword, @salt);", conn, (MySqlTransaction)transaction);
                passCmd.Parameters.AddWithValue("@user_id", userId);
                passCmd.Parameters.AddWithValue("@hashedPassword", hashedPassword);
                passCmd.Parameters.AddWithValue("@salt", salt);
                sw.Restart();
                await passCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[STEP] Insert password: {sw.ElapsedMilliseconds}ms");
                await transaction.CommitAsync();

                Preferences.Set("current_user_id", userId);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public static async Task<string> GetEmailIfExistsAsync(string email)
        {
            const string query = "SELECT email FROM user_login WHERE email = @Email LIMIT 1";

            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    var result = await cmd.ExecuteScalarAsync();
                    return result?.ToString();
                }
            }
        }


        //// LOGIN

        public static async Task<int?> CheckUserLoginAsync(string identifier, string password)
        {
            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();

            const string selectUserQuery = @"
                                            SELECT u.user_id, u.email, u.login, p.password_hash
                                            FROM user_login u
                                            JOIN user_pwd p ON u.user_id = p.user_id
                                            WHERE u.email = @identifier OR u.login = @identifier
                                            LIMIT 1";

            using var cmd = new MySqlCommand(selectUserQuery, conn);
            cmd.Parameters.AddWithValue("@identifier", identifier);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                await Shell.Current.DisplayAlert("Ошибка", "Такой email или логин не зарегистрирован.", "OK");
                return null;
            }

            string storedHash = reader.GetString("password_hash");
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, storedHash);
            if (!isPasswordValid)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Неправильный пароль.", "OK");
                return null;
            }

            int userId = reader.GetInt32("user_ID");

            Preferences.Set("current_user_id", userId);

            return userId;
        }



        // get user recent task
        public static string GetNearestDeadline(int userId)
        {

            using var conn = new MySqlConnection(builder.ConnectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                                SELECT dueDate
                                FROM user_tasks
                                WHERE user_id = @user_id
                                  AND dueDate IS NOT NULL
                                  AND dueDate NOT IN ('Everyday', 'None', '')
                                ORDER BY STR_TO_DATE(dueDate, '%d.%m.%Y') ASC
                                LIMIT 1;
                              ";
            cmd.Parameters.AddWithValue("@user_id", userId);

            var result = cmd.ExecuteScalar() as string;

            if (string.IsNullOrEmpty(result))
                return "Нет активных задач";

            if (DateTime.TryParseExact(result, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                if (dt.Date == DateTime.Today) return "Today";
                if (dt.Date == DateTime.Today.AddDays(1)) return "Tomorrow";
                return dt.ToString("dd MMMM yyyy", new CultureInfo("en-EN"));
            }

            return result;
        }
    }
}
