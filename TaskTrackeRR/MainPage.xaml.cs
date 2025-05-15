using System.Collections.ObjectModel;

namespace TaskTrackeRR;

public partial class MainPage : ContentPage
{
    public ObservableCollection<string> Tasks { get; set; } = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    
    }


    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        var newTask = NewTaskEntry.Text?.Trim();

        if (string.IsNullOrEmpty(newTask))
        {
            // Navigate to a new page if task is empty
            await Navigation.PushAsync(new AddingTaskPage());
        }
        else
        {
            Tasks.Add(newTask);
            NewTaskEntry.Text = string.Empty;
        }
    }
}
