namespace TaskTrackeRR;

public partial class AdminPanel : ContentPage
{
    public AdminPanel()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        var users = await DataBaseInit_Users.GetUsersAsync();
        UsersLayout.Children.Clear();

        foreach (var user in users)
        {
            var frame = new Frame
            {
                CornerRadius = 12,
                Padding = 10,
                Margin = new Thickness(0),
                BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark
                                  ? Color.FromArgb("#2c2c2c")
                                  : Color.FromArgb("#e0e0e0"),
                Content = new Label
                {
                    Text = $"user_id: {user.UserId}\nemail: {user.Email}\nlogin: {user.Login}",
                    FontSize = 14
                }
            };

            UsersLayout.Children.Add(frame);
        }
    }

    private async void OnExportButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var users = await DataBaseInit_Users.GetUsersAsync();

            string fileName = Path.Combine(FileSystem.Current.AppDataDirectory, "users_export.txt");
            await DataBaseInit_Users.ExportUsersToTxtAsync(users, fileName);

            await DisplayAlert("Success", $"Exported to:\n{fileName}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnChartButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UsersChartPage());
    }
}