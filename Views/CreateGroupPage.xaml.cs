using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    public partial class CreateGroupPage : ContentPage
    {
        private readonly StudyGroupService _groupService;
        private int _currentUserId;

        public CreateGroupPage()
        {
            InitializeComponent();
            _groupService = new StudyGroupService();

            // Set default values
            MaxMembersPicker.SelectedIndex = 1; // Default to 10 members
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Get current user ID
            _currentUserId = Preferences.Get("UserId", 0);
            if (_currentUserId == 0)
            {
                Shell.Current.GoToAsync("///LoginPage");
                return;
            }
        }

        private async void OnCreateGroupClicked(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(GroupNameEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please enter a group name", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(SubjectEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please enter a subject", "OK");
                return;
            }

            if (MaxMembersPicker.SelectedItem == null)
            {
                await DisplayAlert("Validation Error", "Please select maximum members", "OK");
                return;
            }

            // Show loading
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            CreateGroupButton.IsEnabled = false;
            CancelButton.IsEnabled = false;

            try
            {
                var maxMembers = int.Parse(MaxMembersPicker.SelectedItem.ToString());
                var isPublic = PublicRadio.IsChecked;

                // Create the group
                var result = await _groupService.CreateGroupAsync(
                    GroupNameEntry.Text.Trim(),
                    DescriptionEditor.Text?.Trim() ?? string.Empty,
                    SubjectEntry.Text.Trim(),
                    maxMembers,
                    isPublic,
                    _currentUserId
                );

                if (result.Success)
                {
                    await DisplayAlert("Success", result.Message, "OK");

                    // Clear form
                    ClearForm();

                    // Navigate back to groups list
                    await Shell.Current.GoToAsync("///MainTabs/StudyGroupsPage");
                }
                else
                {
                    await DisplayAlert("Error", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to create group: {ex.Message}", "OK");
            }
            finally
            {
                // Hide loading
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                CreateGroupButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
        }

        private void ClearForm()
        {
            GroupNameEntry.Text = string.Empty;
            SubjectEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
            MaxMembersPicker.SelectedIndex = 1;
            PublicRadio.IsChecked = true;
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            bool confirm = true;

            // Check if user has entered any data
            if (!string.IsNullOrWhiteSpace(GroupNameEntry.Text) ||
                !string.IsNullOrWhiteSpace(SubjectEntry.Text) ||
                !string.IsNullOrWhiteSpace(DescriptionEditor.Text))
            {
                confirm = await DisplayAlert("Discard Changes",
                    "Are you sure you want to discard your changes?",
                    "Yes", "No");
            }

            if (confirm)
            {
                await Shell.Current.GoToAsync("///MainTabs/StudyGroupsPage");
            }
        }
    }
}