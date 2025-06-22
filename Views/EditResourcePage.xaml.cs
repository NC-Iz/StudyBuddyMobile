using StudyBuddyMobile.Models;
using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    [QueryProperty(nameof(ResourceId), "resourceId")]
    public partial class EditResourcePage : ContentPage
    {
        private readonly StudyResourceService _resourceService;
        private StudyResource? _currentResource;
        private int _currentUserId;
        private int _resourceId;

        public int ResourceId
        {
            get => _resourceId;
            set
            {
                _resourceId = value;
                // Don't call LoadResource here, do it in OnAppearing
            }
        }

        public EditResourcePage()
        {
            InitializeComponent();
            _resourceService = new StudyResourceService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Get current user ID
            _currentUserId = Preferences.Get("UserId", 0);
            if (_currentUserId == 0)
            {
                await Shell.Current.GoToAsync("///LoginPage");
                return;
            }

            // ALWAYS reload the resource data when page appears
            if (_resourceId > 0)
            {
                await LoadResourceAsync();
            }
        }

        private async Task LoadResourceAsync()
        {
            if (_resourceId <= 0 || _currentUserId <= 0) return;

            try
            {
                // Always get fresh data from database
                _currentResource = await _resourceService.GetResourceByIdAsync(_resourceId, _currentUserId);

                if (_currentResource != null)
                {
                    // Clear and repopulate form with fresh data
                    TitleEntry.Text = string.Empty;
                    SubjectEntry.Text = string.Empty;
                    DescriptionEditor.Text = string.Empty;

                    // Set fresh data
                    TitleEntry.Text = _currentResource.Title;
                    SubjectEntry.Text = _currentResource.Subject;
                    DescriptionEditor.Text = _currentResource.Description ?? "";
                    CurrentFileLabel.Text = _currentResource.FileName ?? "Unknown file";
                }
                else
                {
                    await DisplayAlert("Error", "Resource not found", "OK");
                    await Shell.Current.GoToAsync("///MainTabs/StudyResourcesPage");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load resource: {ex.Message}", "OK");
            }
        }

        private async void OnUpdateResourceClicked(object sender, EventArgs e)
        {
            if (_currentResource == null)
            {
                await DisplayAlert("Error", "No resource loaded", "OK");
                return;
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(TitleEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please enter a resource title", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(SubjectEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please enter a subject", "OK");
                return;
            }

            // Show loading
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            UpdateResourceButton.IsEnabled = false;

            try
            {
                var result = await _resourceService.UpdateResourceAsync(
                    _currentResource.Id,
                    _currentUserId,
                    TitleEntry.Text.Trim(),
                    DescriptionEditor.Text?.Trim(),
                    SubjectEntry.Text.Trim()
                );

                if (result.Success)
                {
                    await DisplayAlert("Success", "Resource updated successfully!", "OK");

                    // Navigate back to resources list
                    await Shell.Current.GoToAsync("///MainTabs/StudyResourcesPage");
                }
                else
                {
                    await DisplayAlert("Update Failed", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update resource: {ex.Message}", "OK");
            }
            finally
            {
                // Hide loading
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                UpdateResourceButton.IsEnabled = true;
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///MainTabs/StudyResourcesPage");
        }
    }
}