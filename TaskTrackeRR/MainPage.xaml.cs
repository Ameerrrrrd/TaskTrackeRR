using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TaskTrackeRR;

public partial class MainPage : ContentPage
{
    public ObservableCollection<TaskModel> Tasks { get; set; } = new();


    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    
    }
    public MainPage(ObservableCollection<TaskModel> tasks)
    {
        InitializeComponent();
        Tasks = tasks;
        BindingContext = this;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // НЕ ждем — просто запускаем асинхронную загрузку
        _ = LoadTasksAsync();
    }

    private async Task LoadTasksAsync()
    {
        if (Tasks.Count > 0) return;

        IsBusy = true;

        try
        {
            int currentUserId = Preferences.Get("current_user_id", -1);
            var tasksFromDb = await DataBaseInit_tasks.ShowUserTasks(currentUserId);

            // Прямое добавление, без UI блокировки
            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var task in tasksFromDb)
                    Tasks.Add(task);
            });
        }
        finally
        {
            IsBusy = false;
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

                    Tasks.Add(new TaskModel
                    {
                        Name = newTask,
                        Description = ""
                    });
                    NewTaskEntry.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
            }

        }
    }

    private async void OnTaskSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is TaskModel selectedTask)
        {
            SelectedTaskContext.TaskId = selectedTask.TaskId;
            SelectedTaskContext.TaskName = selectedTask.Name;

            await Navigation.PushAsync(new TaskPreview());
        }
    }
}
