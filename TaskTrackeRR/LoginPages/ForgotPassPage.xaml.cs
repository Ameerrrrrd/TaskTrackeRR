using MySqlConnector;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;

namespace TaskTrackeRR;

public partial class ForgotPassPage : ContentPage
{
    private static readonly MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
    {
        Server = "192.168.245.126",
        UserID = "root",
        Password = "root",
        Database = "troll",
    };

    public ForgotPassPage()
    {
        InitializeComponent();
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        string email = emailEntry.Text;

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Ошибка", "Пожалуйста, введите email.", "ОК");
            return;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            await DisplayAlert("Ошибка", "Некорректный формат email.", "ОК");
            return;
        }

        using var conn = new MySqlConnection(builder.ConnectionString);
        try
        {
            await conn.OpenAsync();
            var checkEmailQuery = "SELECT user_id, login FROM user_login WHERE email = @email";
            using var checkCmd = new MySqlCommand(checkEmailQuery, conn);
            checkCmd.Parameters.AddWithValue("@email", email);

            using var reader = await checkCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                await DisplayAlert("Ошибка", "Email не найден.", "ОК");
                return;
            }

            int userId = reader.GetInt32("user_id");
            string login = reader.GetString("login");
            await reader.CloseAsync();

            string newPassword = GenerateRandomPassword(8);
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, salt);

            string updatePwdQuery = "UPDATE user_pwd SET salt = @salt, password_hash = @password_hash WHERE user_ID = @userId";
            using var updateCmd = new MySqlCommand(updatePwdQuery, conn);
            updateCmd.Parameters.AddWithValue("@salt", salt);
            updateCmd.Parameters.AddWithValue("@password_hash", hashedPassword);
            updateCmd.Parameters.AddWithValue("@userId", userId);
            await updateCmd.ExecuteNonQueryAsync();

            await SendEmailAsync(email, login, newPassword);
            await DisplayAlert("Успешно", "Новый пароль отправлен на email.", "ОК");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "ОК");
        }
    }

    private static string GenerateRandomPassword(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var password = new char[length];
        for (int i = 0; i < length; i++)
            password[i] = chars[random.Next(chars.Length)];
        return new string(password);
    }

    private static async Task SendEmailAsync(string toEmail, string login, string password)
    {
        var smtpHost = "smtp.gmail.com";
        var smtpPort = 587;
        var smtpUsername = "tutameer@gmail.com";
        var smtpPassword = "wrqx wcdu rvre lxhr";

        var mail = new MailMessage
        {
            From = new MailAddress(smtpUsername),
            Subject = "Ваши учетные данные",
            Body = $"Логин: {login}\nПароль: {password}"
        };
        mail.To.Add(toEmail);

        using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUsername, smtpPassword),
            EnableSsl = true
        };

        try
        {
            await smtpClient.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка отправки письма: {ex.Message}");
        }
    }
}