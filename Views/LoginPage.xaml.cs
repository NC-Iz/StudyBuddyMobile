using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly UserService _userService;

        public LoginPage()
        {
            InitializeComponent();
            _userService = new UserService();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Error", "Please enter both email and password", "OK");
                return;
            }

            // Show loading
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            LoginButton.IsEnabled = false;

            try
            {
                var result = await _userService.LoginAsync(EmailEntry.Text.Trim(), PasswordEntry.Text);

                if (result.Success && result.User != null)
                {
                    // Store user info in preferences for session management
                    Preferences.Set("UserId", result.User.Id);
                    Preferences.Set("UserName", result.User.Name);
                    Preferences.Set("UserEmail", result.User.Email);

                    await DisplayAlert("Success", $"Welcome back, {result.User.Name}!", "OK");

                    // Navigate to main dashboard
                    await Shell.Current.GoToAsync("///MainTabs/DashboardPage");
                }
                else
                {
                    await DisplayAlert("Login Failed", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                // Hide loading
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                LoginButton.IsEnabled = true;
            }
        }

        private async void OnRegisterTapped(object sender, EventArgs e)
        {
            // Navigate to register page - we'll create this next
            await Shell.Current.GoToAsync("///RegisterPage");
        }
    }
}