using StudyBuddyMobile.Models;
using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    [QueryProperty(nameof(GroupId), "groupId")]
    public partial class EditGroupPage : ContentPage
    {
        private readonly StudyGroupService _groupService;
        private StudyGroup? _currentGroup;
        private int _currentUserId;
        private int _groupId;

        public int GroupId
        {
            get => _groupId;
            set => _groupId = value;
        }

        public EditGroupPage()
        {
            InitializeComponent();
            _groupService = new StudyGroupService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _currentUserId = Preferences.Get("UserId", 0);
            if (_currentUserId == 0)
            {
                await Shell.Current.GoToAsync("///LoginPage");
                return;
            }

            if (_groupId > 0)
            {
                await LoadGroupDetails();
            }
        }

        private async Task LoadGroupDetails()
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                _currentGroup = await _groupService.GetGroupDetailsAsync(_groupId, _currentUserId);

                if (_currentGroup != null)
                {
                    // Check if user is creator
                    if (_currentGroup.CreatedBy != _currentUserId)
                    {
                        await DisplayAlert("Access Denied", "Only group creators can edit group details.", "OK");
                        await Shell.Current.GoToAsync("..");
                        return;
                    }

                    PopulateForm();
                }
                else
                {
                    await DisplayAlert("Error", "Group not found", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load group: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private void PopulateForm()
        {
            if (_currentGroup == null) return;

            // Update current info display
            CurrentInfoLabel.Text = $"Group: {_currentGroup.Name}\n" +
                                   $"Subject: {_currentGroup.Subject}\n" +
                                   $"Members: {_currentGroup.Members?.Count ?? 0}/{_currentGroup.MaxMembers}\n" +
                                   $"Created: {_currentGroup.CreatedDate:MMM dd, yyyy}";

            // Populate form fields
            GroupNameEntry.Text = _currentGroup.Name;
            SubjectEntry.Text = _currentGroup.Subject;
            DescriptionEditor.Text = _currentGroup.Description ?? "";

            // Set max members picker
            var maxMembersText = _currentGroup.MaxMembers.ToString();
            for (int i = 0; i < MaxMembersPicker.Items.Count; i++)
            {
                if (MaxMembersPicker.Items[i] == maxMembersText)
                {
                    MaxMembersPicker.SelectedIndex = i;
                    break;
                }
            }

            // Set visibility
            PublicRadio.IsChecked = _currentGroup.IsPublic;
            PrivateRadio.IsChecked = !_currentGroup.IsPublic;
        }

        private async void OnUpdateGroupClicked(object sender, EventArgs e)
        {
            if (_currentGroup == null) return;

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

            // Check if new max members is less than current member count
            var newMaxMembers = int.Parse(MaxMembersPicker.SelectedItem.ToString());
            var currentMemberCount = _currentGroup.Members?.Count ?? 0;

            if (newMaxMembers < currentMemberCount)
            {
                await DisplayAlert("Invalid Max Members",
                    $"Cannot set max members to {newMaxMembers} because the group already has {currentMemberCount} members.",
                    "OK");
                return;
            }

            // Show loading
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            UpdateGroupButton.IsEnabled = false;
            CancelButton.IsEnabled = false;

            try
            {
                var result = await _groupService.UpdateGroupAsync(
                    _currentGroup.Id,
                    _currentUserId,
                    GroupNameEntry.Text.Trim(),
                    DescriptionEditor.Text?.Trim() ?? string.Empty,
                    SubjectEntry.Text.Trim(),
                    newMaxMembers,
                    PublicRadio.IsChecked
                );

                if (result.Success)
                {
                    await DisplayAlert("Success", result.Message, "OK");

                    // Navigate back to group details
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await DisplayAlert("Error", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update group: {ex.Message}", "OK");
            }
            finally
            {
                // Hide loading
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                UpdateGroupButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            bool hasChanges = false;

            // Check if user made any changes
            if (_currentGroup != null)
            {
                hasChanges = GroupNameEntry.Text?.Trim() != _currentGroup.Name ||
                           SubjectEntry.Text?.Trim() != _currentGroup.Subject ||
                           DescriptionEditor.Text?.Trim() != (_currentGroup.Description ?? "") ||
                           PublicRadio.IsChecked != _currentGroup.IsPublic;
            }

            if (hasChanges)
            {
                bool confirm = await DisplayAlert("Discard Changes",
                    "Are you sure you want to discard your changes?",
                    "Yes", "No");

                if (!confirm) return;
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}