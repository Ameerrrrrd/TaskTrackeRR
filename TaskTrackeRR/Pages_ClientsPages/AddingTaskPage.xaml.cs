using System;
using Microsoft.Maui.Controls;

namespace TaskTrackeRR;

public partial class AddingTaskPage : ContentPage
{
    private static string taskName = string.Empty;
    private static string taskDescription = string.Empty;
    private string taskDueDate = string.Empty;
    private string taskDifficulty = string.Empty;
    private int taskStoryPoints = 0;

    public bool datePickerFlag;


    private Button noneDateBtn;
    private Button everydayBtn;
    private DatePicker datePickerField;

    public AddingTaskPage()
    {
        InitializeComponent();
        NameEntry.TextChanged += OnNameTextChanged;
    }

    private void OnNameTextChanged(object sender, TextChangedEventArgs e)
    {
        if (NameEntry.Text.Length > 30) NameErrorLabel.IsVisible = true;
        else NameErrorLabel.IsVisible = false;
    }

    private void OnNoneDateClicked(object sender, EventArgs e)
    {
        taskDueDate = string.Empty;
        HighlightDateButton((Button)sender);

        datePickerFlag = false;
    }

    private void OnEverydayClicked(object sender, EventArgs e)
    {
        taskDueDate = "Everyday";
        HighlightDateButton((Button)sender);
        datePickerFlag = false;
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        taskDueDate = e.NewDate.ToShortDateString();
        HighlightDateButton(null);
        datePickerFlag = true;
    }

    private void HighlightDateButton(Button selected)
    {
        NoneDateButton.BackgroundColor = Colors.LightGray;
        NoneDateButton.TextColor = Colors.Black;

        EverydayDateButton.BackgroundColor = Colors.LightGray;
        EverydayDateButton.TextColor = Colors.Black;
        
        if (selected != null)
        {
            selected.BackgroundColor = Color.FromArgb("#007AFF");
            selected.TextColor = Colors.LightGray;
        }
    }

    private void OnDifficultySelected(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        taskDifficulty = picker.SelectedIndex == 0 ? string.Empty : (string)picker.SelectedItem;
    }

    private void OnStoryPointsSelected(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        taskStoryPoints = picker.SelectedIndex == 0 ? 0 : (int)picker.SelectedItem;
    }

    private async void OnAddTaskButtonClicked(object sender, EventArgs e)
    {
        taskName = NameEntry.Text?.Trim();
        taskDescription = DescriptionEditor.Text?.Trim();

        DateTime today = DateTime.Today;
        DateTime selectedDate = DatePickerField.Date;

        if(datePickerFlag && selectedDate < today)
        {
            await DisplayAlert("Validation Error", "DueDate must be > current date", "OK");
            return;
        }
        if (string.IsNullOrEmpty(taskName))
        {
            await DisplayAlert("Validation Error", "Task Name is required.", "OK");
            return;
        }

        if (NameEntry.Text.Length > 30)
        {
            await DisplayAlert("Error", "Name can't exceed 30 characters.", "OK");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            int? currentUserId = Preferences.Get("current_user_id", -1);
            if (currentUserId != -1)
            {
                var tasks = await DataBaseInit_tasks.ShowUserTasks(currentUserId.Value);

                var mainPage = new MainPage();
                foreach (var task in tasks)
                    mainPage.Tasks.Add(task);

                await DataBaseInit_tasks.InsertTaskInDB(currentUserId.Value, taskName, taskDescription, taskDueDate, taskDifficulty, taskStoryPoints.ToString());
                await DisplayAlert("Task Added", $"Name: {taskName}\nDescription: {taskDescription}\nDue: {taskDueDate}\nDifficulty: {taskDifficulty}\nStory Points: {taskStoryPoints}", "OK");

                await Shell.Current.GoToAsync("//MainPage");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK"); return;
        }

        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }
}
