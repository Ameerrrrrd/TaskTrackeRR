using System.Text.RegularExpressions;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace TaskTrackeRR;

public partial class RegisterPage : ContentPage
{

    public RegisterPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) => await Navigation.PopAsync(); 
        this.Resources["LoginTap"] = tap;
    }

    private async void OnRegisterButClicked(object sender, EventArgs e)
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        bool result = false;
        string email = emailEntry.Text?.Trim();
        string password = passwordEntry.Text;
        string login = loginEntry.Text;

        try
        {
            // Проверка заполненности полей
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Required fields are not filled in", "OK");
                return;
            }

            // Проверка формата email
            if (!Regex.IsMatch(email, @"^[\w\.-]+@[\w\.-]+\.[a-z]{2,4}$", RegexOptions.IgnoreCase))
            {
                await DisplayAlert("Error", "Email isn't in correct format", "OK");
                return;
            }

            // Проверка существования email
            string existingEmail = await DataBaseInit_Users.GetEmailIfExistsAsync(email);
            if (!string.IsNullOrEmpty(existingEmail) && existingEmail == email)
            {
                await DisplayAlert("Error", "Такой email уже существует", "OK");
                return;
            }

            // Сохранение данных пользователя (рекомендуется использовать SecureStorage вместо Preferences для пароля)
            Preferences.Set("user_email", email);
            Preferences.Set("user_password", password); // Внимание: это не безопасно, лучше использовать SecureStorage
            Preferences.Set("user_login", login);

            // Регистрация пользователя
            await DataBaseInit_Users.RegisterUserAsync(login, email, password);
            result = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error (Exception): {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }

        if (result)
        {
            // Уведомление об успешной регистрации
            await DisplayAlert("Success", "Account created", "OK");

            // Проверка доступности биометрической аутентификации
            var isBiometricAvailable = await CrossFingerprint.Current.IsAvailableAsync();
            if (isBiometricAvailable)
            {
                // Предложение включить биометрический вход
                var enableBiometric = await DisplayAlert("Включить вход по биометрии",
                    "Хотите включить вход по биометрии для более удобного доступа?",
                    "Да", "Нет");

                if (enableBiometric)
                {
                    var config = new AuthenticationRequestConfiguration(
                                                                        "Подтверждение биометрии",
                                                                        "Подтвердите биометрию для включения функции"
                                                                       );
                    var authResult = await CrossFingerprint.Current.AuthenticateAsync(config);

                    if (authResult.Authenticated)
                    {
                        Console.WriteLine("Биометрия успешно подтверждена!");
                    }
                    else
                    {
                        Console.WriteLine("Ошибка аутентификации.");
                    }
                    if (authResult.Authenticated)
                    {
                        // Сохранение признака, что биометрический вход включен
                        Preferences.Set("biometric_login_enabled", true);
                        await DisplayAlert("Success", "Вход по биометрии включен", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Не удалось подтвердить биометрию", "OK");
                    }
                }
            }

            // Переход на главную страницу
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await DisplayAlert("Error", "Failed to create account", "OK");
        }
    }


    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }
}