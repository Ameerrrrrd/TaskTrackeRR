using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TaskTrackeRR;

public interface ITaskProcessor
{
    public void ProcessTask(string task)
    {
        Console.WriteLine($"Обработка задачи: {task}");
    }

    public void RefferPages()
    {
        Console.WriteLine("Совершен перехож по странице");
    }
}

public partial class MainPage : ContentPage,ITaskProcessor
{
    public ObservableCollection<TaskModel> Tasks { get; set; } = new();

    public ITaskProcessor logging;

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

    //обработка блока с логином и дедлайном
    public async void GreetingText()
    {
        string user_name = Preferences.Get("user_login", "-1");
        SupText.Text = $"👤 Sup, {user_name}";

        int user_id = Preferences.Get("current_user_id", -1);
        string deadlineValue = DataBaseInit_Users.GetNearestDeadline(user_id);
        DeadlineText.Text = $"🕒 Hottest: {deadlineValue}";

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
        {
            await Navigation.PushAsync(new AddingTaskPage());
            //logging.RefferPages();
        }
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
            
            //logging.RefferPages();
            //logging.ProcessTask(selectedTask.Name);
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

    private async void OnAboutTapped(object sender, EventArgs e)
    {
        await DisplayAlert("/", "You'll reffer to github with readme", "OK");
        var uri = new Uri("https://github.com/Ameerrrrrd/TaskTrackeRR");
        await Launcher.Default.OpenAsync(uri);
        //logging.RefferPages();
    }
}
