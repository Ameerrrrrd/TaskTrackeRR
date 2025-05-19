using Microsoft.Extensions.Logging.Abstractions;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace TaskTrackeRR;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) => await Navigation.PopAsync();
        this.Resources["LoginTap"] = tap;
    }

    private async void OnLoginButClicked(object sender, EventArgs e)
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        string input = emailEntry.Text?.Trim();
        string password = passwordEntry.Text;


        try
        {
            if (emailEntry.Text == "admin" && passwordEntry.Text == "admin")
            {
                var ap = new AdminPanel();
                await Navigation.PushAsync(ap);
                return;
            }

            int? userId = await DataBaseInit_Users.CheckUserLoginAsync(input, password);
            if (userId != null)
            {
                Preferences.Set("current_user_id", userId.Value);
                Preferences.Set("user_login", emailEntry.Text);
                if (RememberMeCheckbox.IsChecked)
                {
                    Preferences.Set("remember_me", true);
                    await SecureStorage.SetAsync("user_login", emailEntry.Text);
                }
                else
                    Preferences.Set("remember_me", false);
                //var tasks = await DataBaseInit_tasks.ShowUserTasks(userId.Value);
                //var mainPage = new MainPage();
                //foreach (var task in tasks)
                //    mainPage.Tasks.Add(task);

                await DisplayAlert("Success", "Login  successfully", "OK");
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
                await DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
    private async void ShowPassCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        passwordEntry.IsPassword = !e.Value;
    }
}