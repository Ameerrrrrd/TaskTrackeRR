using Microsoft.Extensions.Logging.Abstractions;
using System.Text.RegularExpressions;

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
            int? userId = await DataBaseInit_Users.CheckUserLoginAsync(input, password);
            if (userId != null)
            {
                await DisplayAlert("Success", "Login successfully", "OK");
                await Navigation.PushAsync(new MainPage());
            }

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
}