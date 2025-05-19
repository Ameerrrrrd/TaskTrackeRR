using MySqlConnector;

namespace TaskTrackeRR;

public partial class UsersChartPage : ContentPage
{
	public UsersChartPage()
	{
		InitializeComponent();
        _ = LoadTaskCountsAsync();
    }
    private async Task LoadTaskCountsAsync()
    {
        List<UserTaskCount> users = new();
        try
        {
            await DataBaseInit_tasks.GetUserTaskCount(users);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Database error: {ex.Message}", "OK");
            return;
        }

        TasksContainer.Children.Clear();
        int counter = 1;
        foreach (var result in users)
        {
            var frame = new Frame
            {
                CornerRadius = 12,
                Padding = 10,
                Margin = new Thickness(0),
                BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#2c2c2c")
                    : Color.FromArgb("#e0e0e0"),
                Content = new VerticalStackLayout
                {
                    Children =
                        {
                            new Label { Text = $"- {counter}. {result.login}", FontSize = 14 },
                            new Label { Text = $"- Tasks count - {result.task_count}", FontSize = 14 }
                        }
                }
            };
            counter++;
            TasksContainer.Children.Add(frame);
        }
    }
}

public class UserTaskCount
{
    public int user_id { get; set; }
    public string login { get; set; }
    public int task_count { get; set; }
}