namespace TaskTrackeRR
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();


        }
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//register");
        }

    }
}
