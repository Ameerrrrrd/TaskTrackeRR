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
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Required fields are not filled in", "OK");
                return;
            }

            if (!Regex.IsMatch(email, @"^[\w\.-]+@[\w\.-]+\.[a-z]{2,4}$", RegexOptions.IgnoreCase))
            {
                await DisplayAlert("Error", "Email isn't in correct format", "OK");
                return;
            }

            string existingEmail = await DataBaseInit_Users.GetEmailIfExistsAsync(email);
            if (!string.IsNullOrEmpty(existingEmail) && existingEmail == email)
            {
                await DisplayAlert("Error", "Такой email уже существует", "OK");
                return;
            }

            Preferences.Set("user_email", email);
            Preferences.Set("user_password", password); 
            Preferences.Set("user_login", login);

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
            await DisplayAlert("Success", "Account created", "OK");

            var isBiometricAvailable = await CrossFingerprint.Current.IsAvailableAsync();
            if (isBiometricAvailable)
            {
                var enableBiometric = await DisplayAlert("log in by fingerprint",
                    "Wanna enable log in by fingerprint",
                    "Yes", "No");

                if (enableBiometric)
                {
                    var config = new AuthenticationRequestConfiguration(
                                                                        "Confirming",
                                                                        "Confirm your fingerprint"
                                                                       );
                    var authResult = await CrossFingerprint.Current.AuthenticateAsync(config);

                    if (authResult.Authenticated)
                    {
                        Console.WriteLine("Биометрия успешно подтверждена");
                    }
                    else
                    {
                        Console.WriteLine("Ошибка аутентификации.");
                    }
                    if (authResult.Authenticated)
                    {
                        // Сохранение признака, что биометрический вход включен
                        Preferences.Set("biometric_login_enabled", true);
                        await DisplayAlert("Success", "Log in by fingerprint enabled", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Error due log in by fingerprint", "OK");
                    }
                }
            }
            var mainPage = new MainPage();
            await Navigation.PushAsync(mainPage);
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