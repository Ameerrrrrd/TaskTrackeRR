using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TaskTrackeRR;

public partial class MainPage : ContentPage
{
    public ObservableCollection<string> Tasks { get; set; } = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Tasks.Count == 0)
        {
            int currentUserId = Preferences.Get("current_user_id", -1);
            var tasksFromDb = await DataBaseInit_tasks.ShowUserTasks(currentUserId);
            foreach (var task in tasksFromDb)
                Tasks.Add(task);
        }
    }

    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        var newTask = NewTaskEntry.Text?.Trim();

        if (string.IsNullOrEmpty(newTask))
            await Navigation.PushAsync(new AddingTaskPage());
        else
        {
            try
            {
                int currentUserId = Preferences.Get("current_user_id", -1);
                if (currentUserId != -1)
                {
                    await DataBaseInit_tasks.InsertTaskByEntry(currentUserId, newTask);

                    Tasks.Add(newTask);
                    NewTaskEntry.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
            }
        }
    }
}
