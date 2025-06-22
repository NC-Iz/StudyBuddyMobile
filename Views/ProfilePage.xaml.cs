using StudyBuddyMobile.Services;
using StudyBuddyMobile.Models;

namespace StudyBuddyMobile.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly UserService _userService;
        private User? _currentUser;

        public ProfilePage()
        {
            InitializeComponent();
            _userService = new UserService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadUserProfile();
        }

        private async Task LoadUserProfile()
        {
            try
            {
                // Get user ID from preferences
                var userId = Preferences.Get("UserId", 0);
                if (userId == 0)
                {
                    // User not logged in, redirect to login
                    await Shell.Current.GoToAsync("///LoginPage");
                    return;
                }

                // Load user data
                _currentUser = await _userService.GetUserByIdAsync(userId);
                if (_currentUser != null)
                {
                    // Update UI with user data
                    UserNameLabel.Text = _currentUser.Name;
                    UserEmailLabel.Text = _currentUser.Email;
                    NameEntry.Text = _currentUser.Name;
                    StudyInterestsEditor.Text = _currentUser.StudyInterests ?? "";

                    // Update stats
                    MemberSinceLabel.Text = _currentUser.CreatedDate.ToString("MMM yyyy");

                    if (_currentUser.LastLoginDate.HasValue)
                    {
                        var daysSince = (DateTime.Now - _currentUser.LastLoginDate.Value).Days;
                        LastLoginLabel.Text = daysSince == 0 ? "Today" :
                                            daysSince == 1 ? "Yesterday" :
                                            $"{daysSince} days ago";
                    }
                    else
                    {
                        LastLoginLabel.Text = "First time";
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load profile: {ex.Message}", "OK");
            }
        }

        private async void OnUpdateProfileClicked(object sender, EventArgs e)
        {
            if (_currentUser == null) return;

            // Validate input
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("Error", "Please enter your name", "OK");
                return;
            }

            // Show loading
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            UpdateProfileButton.IsEnabled = false;

            try
            {
                var result = await _userService.UpdateUserAsync(
                    _currentUser.Id,
                    NameEntry.Text.Trim(),
                    StudyInterestsEditor.Text?.Trim()
                );

                if (result.Success)
                {
                    // Update preferences
                    Preferences.Set("UserName", NameEntry.Text.Trim());

                    // Update local user object
                    _currentUser.Name = NameEntry.Text.Trim();
                    _currentUser.StudyInterests = StudyInterestsEditor.Text?.Trim();

                    // Update UI
                    UserNameLabel.Text = _currentUser.Name;

                    await DisplayAlert("Success", result.Message, "OK");
                }
                else
                {
                    await DisplayAlert("Update Failed", result.Message, "OK");
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
                UpdateProfileButton.IsEnabled = true;
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");

            if (confirm)
            {
                // Clear user preferences
                Preferences.Remove("UserId");
                Preferences.Remove("UserName");
                Preferences.Remove("UserEmail");

                // Navigate to login page
                await Shell.Current.GoToAsync("///LoginPage");
            }
        }

        private async void OnDeactivateClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Deactivate Account",
                "Are you sure you want to deactivate your account? This action cannot be undone and you will be logged out.",
                "Yes, Deactivate", "Cancel");

            if (confirm && _currentUser != null)
            {
                var result = await _userService.DeactivateUserAsync(_currentUser.Id);

                if (result.Success)
                {
                    await DisplayAlert("Account Deactivated", result.Message, "OK");

                    // Clear preferences and logout
                    Preferences.Remove("UserId");
                    Preferences.Remove("UserName");
                    Preferences.Remove("UserEmail");

                    // Navigate to login
                    await Shell.Current.GoToAsync("///LoginPage");
                }
                else
                {
                    await DisplayAlert("Error", result.Message, "OK");
                }
            }
        }
    }
}