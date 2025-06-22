using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly UserService _userService;

        public RegisterPage()
        {
            InitializeComponent();
            _userService = new UserService();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("Error", "Please enter your full name", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                await DisplayAlert("Error", "Please enter your email address", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a password", "OK");
                return;
            }

            if (PasswordEntry.Text.Length < 6)
            {
                await DisplayAlert("Error", "Password must be at least 6 characters long", "OK");
                return;
            }

            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Error", "Passwords do not match", "OK");
                return;
            }

            // Validate email format (basic check)
            if (!EmailEntry.Text.Contains("@") || !EmailEntry.Text.Contains("."))
            {
                await DisplayAlert("Error", "Please enter a valid email address", "OK");
                return;
            }

            // Show loading
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            RegisterButton.IsEnabled = false;

            try
            {
                var result = await _userService.RegisterAsync(
                    EmailEntry.Text.Trim(),
                    PasswordEntry.Text,
                    NameEntry.Text.Trim(),
                    StudyInterestsEditor.Text?.Trim()
                );

                if (result.Success)
                {
                    await DisplayAlert("Success", result.Message, "OK");

                    // Clear form
                    NameEntry.Text = string.Empty;
                    EmailEntry.Text = string.Empty;
                    PasswordEntry.Text = string.Empty;
                    ConfirmPasswordEntry.Text = string.Empty;
                    StudyInterestsEditor.Text = string.Empty;

                    // Navigate back to login page
                    await Shell.Current.GoToAsync("///LoginPage");
                }
                else
                {
                    await DisplayAlert("Registration Failed", result.Message, "OK");
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
                RegisterButton.IsEnabled = true;
            }
        }

        private async void OnLoginTapped(object sender, EventArgs e)
        {
            // Navigate back to login page
            await Shell.Current.GoToAsync("///LoginPage");
        }
    }
}