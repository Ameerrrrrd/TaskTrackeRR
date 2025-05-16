namespace TaskTrackeRR;

public partial class TaskPreview : ContentPage
{
    public TaskPreview(TaskModel task)
    {
        InitializeComponent();
        BindingContext = task;
    }
}