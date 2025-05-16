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
        GreetingText();
    }

    // конуструктор для подгрузки со смежных страниц для оптимизации
    public MainPage(ObservableCollection<TaskModel> tasks)
    {
        InitializeComponent();
        Tasks = tasks;
        BindingContext = this;
        GreetingText();
    }

    public async void GreetingText()
    {
        string user_name = Preferences.Get("user_login", "-1");
        SupText.Text = $"👤 Sup, {user_name}";

        int user_id = Preferences.Get("current_user_id", -1);
        string deadlineValue = DataBaseInit_Users.GetNearestDeadline(user_id);
        DeadlineText.Text = $"🕒 Hottest deadline: {deadlineValue}";

    }

    // запуск асинхронных загрузок
    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = LoadTasksAsync();
    }

    // загрузка тасков из БД
    private async Task LoadTasksAsync()
    {
        if (Tasks.Count > 0) return;

        IsBusy = true;

        try
        {
            int currentUserId = Preferences.Get("current_user_id", -1);
            var tasksFromDb = await DataBaseInit_tasks.ShowUserTasks(currentUserId);

            MainThread.BeginInvokeOnMainThread(() => // Работа с потоками
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

    // обработчик кнопки добавления тасков
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

    // просмотр таска по нажатию
    private async void OnTaskTapped(object sender, EventArgs e)
    {

        if (sender is Label label && label.BindingContext is TaskModel selectedTask)
        {
            await Navigation.PushAsync(new TaskPreviewPage(selectedTask.TaskId));
            SelectedTaskContext.TaskId = selectedTask.TaskId;
        }
    }


    private async void OnDeleteSwipeItemInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItemView swipeItem)
        {

            bool confirmDelete = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this task?", "Yes", "No");
            if (!confirmDelete) return;

            var task = (TaskModel)swipeItem.BindingContext;
            
            var taskId = task.TaskId;

            Console.WriteLine($"ASLDSLAD:AS:DAS:D:ASD:LASLDAS:D::::::: {taskId}");
            await DataBaseInit_tasks.DeleteTasksByTaskIdAsync(taskId);
            Tasks.Remove(task);
        }
    }
}
