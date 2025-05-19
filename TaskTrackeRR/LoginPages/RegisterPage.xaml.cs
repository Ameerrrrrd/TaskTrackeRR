using System.Text.RegularExpressions;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace TaskTrackeRR;

public partial class RegisterPage : ContentPage
{

    public RegisterPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) => await Navigation.PopAsync(); 
        this.Resources["LoginTap"] = tap;
    }

    private async void OnRegisterButClicked(object sender, EventArgs e)
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        bool result = false;
        string email = emailEntry.Text?.Trim();
        string password = passwordEntry.Text;
        string login = loginEntry.Text;

        try
        {
            // �������� ������������� �����
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Required fields are not filled in", "OK");
                return;
            }

            // �������� ������� email
            if (!Regex.IsMatch(email, @"^[\w\.-]+@[\w\.-]+\.[a-z]{2,4}$", RegexOptions.IgnoreCase))
            {
                await DisplayAlert("Error", "Email isn't in correct format", "OK");
                return;
            }

            // �������� ������������� email
            string existingEmail = await DataBaseInit_Users.GetEmailIfExistsAsync(email);
            if (!string.IsNullOrEmpty(existingEmail) && existingEmail == email)
            {
                await DisplayAlert("Error", "����� email ��� ����������", "OK");
                return;
            }

            // ���������� ������ ������������ (������������� ������������ SecureStorage ������ Preferences ��� ������)
            Preferences.Set("user_email", email);
            Preferences.Set("user_password", password); // ��������: ��� �� ���������, ����� ������������ SecureStorage
            Preferences.Set("user_login", login);

            // ����������� ������������
            await DataBaseInit_Users.RegisterUserAsync(login, email, password);
            result = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error (Exception): {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }

        if (result)
        {
            // ����������� �� �������� �����������
            await DisplayAlert("Success", "Account created", "OK");

            // �������� ����������� �������������� ��������������
            var isBiometricAvailable = await CrossFingerprint.Current.IsAvailableAsync();
            if (isBiometricAvailable)
            {
                // ����������� �������� �������������� ����
                var enableBiometric = await DisplayAlert("�������� ���� �� ���������",
                    "������ �������� ���� �� ��������� ��� ����� �������� �������?",
                    "��", "���");

                if (enableBiometric)
                {
                    var config = new AuthenticationRequestConfiguration(
                                                                        "������������� ���������",
                                                                        "����������� ��������� ��� ��������� �������"
                                                                       );
                    var authResult = await CrossFingerprint.Current.AuthenticateAsync(config);

                    if (authResult.Authenticated)
                    {
                        Console.WriteLine("��������� ������� ������������!");
                    }
                    else
                    {
                        Console.WriteLine("������ ��������������.");
                    }
                    if (authResult.Authenticated)
                    {
                        // ���������� ��������, ��� �������������� ���� �������
                        Preferences.Set("biometric_login_enabled", true);
                        await DisplayAlert("Success", "���� �� ��������� �������", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "�� ������� ����������� ���������", "OK");
                    }
                }
            }

            // ������� �� ������� ��������
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await DisplayAlert("Error", "Failed to create account", "OK");
        }
    }


    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }
}