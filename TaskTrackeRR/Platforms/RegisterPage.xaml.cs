using System.Text.RegularExpressions;

namespace TaskTrackeRR;

public partial class RegisterPage : ContentPage
{

    public RegisterPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        var tap = new tapgesturerecognizer();
        tap.tapped += async (s, e) => await navigation.popasync();
        this.resources["logintap"] = tap;
    }

    private async void OnRegisterButtonClicked(object sender, EventArgs e)
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
            if (!Regex.IsMatch(email, @"^[\w\.-]+@[\w\.-]+\.[a-z]{2,4}$", RegexOptions.IgnoreCase)) // регулярное выражение
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
        catch(Exception ex)
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