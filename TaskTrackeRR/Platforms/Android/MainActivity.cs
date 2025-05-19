using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using Plugin.CurrentActivity;

namespace TaskTrackeRR
{
    [Activity(Theme = "@style/AppTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            CrossFingerprint.SetCurrentActivityResolver(() => CrossCurrentActivity.Current.Activity);
        }

    }
}
