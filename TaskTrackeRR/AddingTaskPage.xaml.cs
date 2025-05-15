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
        if (NameEntry.Text.Length > 30)
        {
            NameErrorLabel.IsVisible = true;
        }
        else
        {
            NameErrorLabel.IsVisible = false;
        }
    }

    private void OnNoneDateClicked(object sender, EventArgs e)
    {
        taskDueDate = string.Empty;
        HighlightDateButton((Button)sender);
    }

    private void OnEverydayClicked(object sender, EventArgs e)
    {
        taskDueDate = "Everyday";
        HighlightDateButton((Button)sender);
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        taskDueDate = e.NewDate.ToShortDateString();
        HighlightDateButton(null);
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
        try
        {
            int currentUserId = Preferences.Get("current_user_id", -1);
            if (currentUserId != -1)
            {
                await DataBaseInit_tasks.InsertTaskInDB(currentUserId, taskName, taskDescription, taskDueDate, taskDifficulty, taskStoryPoints.ToString());
                await DisplayAlert("Task Added", $"Name: {taskName}\nDescription: {taskDescription}\nDue: {taskDueDate}\nDifficulty: {taskDifficulty}\nStory Points: {taskStoryPoints}", "OK");
                await Navigation.PushAsync(new MainPage());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK"); return;
        }
    }
}
