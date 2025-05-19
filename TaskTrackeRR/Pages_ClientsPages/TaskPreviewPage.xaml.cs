using System.Globalization;
using System.Xml.Serialization;
using MySqlConnector;

namespace TaskTrackeRR;

public partial class TaskPreviewPage : ContentPage
{
    private readonly TaskModel _task;

    private readonly int _taskId;
    private readonly int _userId;

    string name;
    string description;
    string dueDate;
    string difficulty;
    string storyPoints;

    private string originalName;
    private string originalDescription;
    private string originalDueDate;
    private string originalDifficulty;
    private string originalStoryPoints;

    public bool datePickerFlag;

    private static readonly MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
    {
        Server = "192.168.217.240",
        UserID = "root",
        Password = "root",
        Database = "troll",
    };

    public TaskPreviewPage(int taskId)
    {
        InitializeComponent();

        _taskId = taskId;
        _userId = Preferences.Get("current_user_id", -1);

    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTaskFromDbAsync();
    }

    private async Task LoadTaskFromDbAsync()
    {
        try
        {
            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();

            var query =new MySqlCommand(@"
                             SELECT name, description, dueDate, difficulty, storyPoints 
                             FROM user_tasks 
                             WHERE user_id = @userId AND task_id = @taskId", conn);

            query.Parameters.AddWithValue("@userId", _userId);
            query.Parameters.AddWithValue("@taskId", _taskId);

            using var reader = await query.ExecuteReaderAsync();
            if (reader.Read())
            {
                name = reader["name"].ToString();
                description = reader["description"].ToString();
                dueDate = reader["dueDate"].ToString();
                difficulty = reader["difficulty"].ToString();
                storyPoints = reader["storyPoints"].ToString();

                originalName = name;
                originalDescription = description;
                originalDueDate = dueDate;
                originalDifficulty = difficulty;
                originalStoryPoints = storyPoints;

                taskNameEntry.Text = name;
                taskDescEditor.Text = description;

                if (DateTime.TryParseExact(dueDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue)) 
                    DatePickerField.Date = dateValue;
                else if (dueDate == "Everyday") OnEverydayClicked(EverydayDateButton, EventArgs.Empty);
                else if (dueDate == "None") OnNoneDateClicked(NoneDateButton, EventArgs.Empty);

                DifficultyPicker.SelectedItem = difficulty;
                StoryPointsPicker.SelectedItem = storyPoints;
            }
            else
            {
                await DisplayAlert("Ошибка", "Задача не найдена", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка при загрузке задачи: {ex.Message}", "OK");
        }
    }
    private void OnEditClicked(object sender, EventArgs e)
    {
        taskNameEntry.IsReadOnly = false;
        taskDescEditor.IsReadOnly = false;
        DatePickerField.IsEnabled = true;
        EverydayDateButton.IsEnabled = true;
        NoneDateButton.IsEnabled = true;
        DifficultyPicker.IsEnabled = true;
        StoryPointsPicker.IsEnabled = true;
        ConfirmButton.IsVisible = true;
        header.Text = "Editing:";
    }

    private void OnNoneDateClicked(object sender, EventArgs e)
    {
        dueDate = "None";
        HighlightDateButton((Button)sender);
        datePickerFlag = false;
    }

    private void OnEverydayClicked(object sender, EventArgs e)
    {
        dueDate = "Everyday";
        HighlightDateButton((Button)sender);
        datePickerFlag = false;
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        dueDate = e.NewDate.ToShortDateString();
        HighlightDateButton(null);
        datePickerFlag = true;
    }

    private void HighlightDateButton(Button selected)
    {
        NoneDateButton.BackgroundColor = Colors.LightGray;
        EverydayDateButton.BackgroundColor = Colors.LightGray;
        NoneDateButton.TextColor = Colors.Black;
        EverydayDateButton.TextColor = Colors.Black;
        if (selected != null)
        {
            selected.BackgroundColor = Color.FromArgb("#007AFF");
            selected.TextColor = Colors.White;
        }
    }


    private async void OnConfirmButtonClicked(object sender, EventArgs e)
    {
        var updates = new List<string>();
        var parameters = new Dictionary<string, object>();

        DateTime today = DateTime.Today;
        DateTime selected = DatePickerField.Date;


        if (datePickerFlag && today > selected)
        {
            await DisplayAlert("Validation Error", "DueDate must be > current date", "OK");
            return;
        }
        if (string.IsNullOrEmpty(taskNameEntry.Text))
        {
            await DisplayAlert("Validation Error", "Task Name is required.", "OK");
            return;
        }

        if (!AddingTaskPage.IsTaskNameValid(taskNameEntry.Text))
        {
            await DisplayAlert("Error", "Name can't exceed 30 characters.", "OK");
            return;
        }

        if (taskNameEntry.Text != originalName)
        {
            updates.Add("name = @name");
            parameters["@name"] = taskNameEntry.Text;
        }

        if (taskDescEditor.Text != originalDescription)
        {
            updates.Add("description = @description");
            parameters["@description"] = taskDescEditor.Text;
        }

        string currentDueDate = GetCurrentDueDate();
        if (currentDueDate != originalDueDate)
        {
            updates.Add("dueDate = @dueDate");
            parameters["@dueDate"] = currentDueDate;
        }

        if (DifficultyPicker.SelectedItem?.ToString() != originalDifficulty)
        {
            updates.Add("difficulty = @difficulty");
            parameters["@difficulty"] = DifficultyPicker.SelectedItem?.ToString();
        }

        if (StoryPointsPicker.SelectedItem?.ToString() != originalStoryPoints)
        {
            updates.Add("storyPoints = @storyPoints");
            parameters["@storyPoints"] = StoryPointsPicker.SelectedItem?.ToString();
        }

        if (updates.Count == 0)
        {
            await DisplayAlert("Нет изменений", "Данные не были изменены.", "OK");
            return;
        }

        string updateClause = string.Join(", ", updates);

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();

            var command = new MySqlCommand(
                $"UPDATE user_tasks SET {updateClause} WHERE user_id = @userId AND task_id = @taskId", conn);

            command.Parameters.AddWithValue("@userId", _userId);
            command.Parameters.AddWithValue("@taskId", _taskId);

            foreach (var param in parameters)
                command.Parameters.AddWithValue(param.Key, param.Value);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            //await DisplayAlert("Успех", $"Обновлено: {rowsAffected} полей", "OK");

            int? currentUserId = Preferences.Get("current_user_id", -1);
            if (currentUserId != -1)
            {
                var tasks = await DataBaseInit_tasks.ShowUserTasks(currentUserId.Value);

                var mainPage = new MainPage();
                foreach (var task in tasks)
                    mainPage.Tasks.Add(task);

                await Shell.Current.GoToAsync("//MainPage");
            }
        }

        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка при обновлении: {ex.Message}", "OK");
        }

        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }

    }

    private void OnNameTextChanged(object sender, TextChangedEventArgs e)
    {
        if (taskNameEntry.Text.Length > 30) NameErrorLabel.IsVisible = true;
        else NameErrorLabel.IsVisible = false;
    }

    private string GetCurrentDueDate()
    {
        if (DatePickerField.IsEnabled)
            return DatePickerField.Date.ToString("dd.MM.yyyy");

        if (EverydayDateButton.BackgroundColor == Colors.Green)
            return "Everyday";

        if (NoneDateButton.BackgroundColor == Colors.Red)
            return "None";

        return originalDueDate;
    }
}