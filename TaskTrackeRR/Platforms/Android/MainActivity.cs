using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views; // 👈 Добавить

namespace TaskTrackeRR
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // ⛔ Скрыть статус-бар
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                SystemUiFlags.LayoutStable |
                SystemUiFlags.LayoutFullscreen |
                SystemUiFlags.Fullscreen);

            // ⛔ Убрать action bar
            ActionBar?.Hide();
        }

    }
}
