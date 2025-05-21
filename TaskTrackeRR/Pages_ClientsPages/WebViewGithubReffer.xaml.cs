namespace TaskTrackeRR;

public partial class WebViewGithubReffer : ContentPage
{
    public bool IsLoading { get; set; } = true;

    public WebViewGithubReffer()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void OnNavigating(object sender, WebNavigatingEventArgs e)
    {
        IsLoading = true;
        OnPropertyChanged(nameof(IsLoading));
    }

    private void OnNavigated(object sender, WebNavigatedEventArgs e)
    {
        IsLoading = false;
        OnPropertyChanged(nameof(IsLoading));
    }
}