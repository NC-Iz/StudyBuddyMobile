using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    public partial class AddResourcePage : ContentPage
    {
        private readonly StudyResourceService _resourceService;
        private FileResult? _selectedFile;
        private int _currentUserId;

        public AddResourcePage()
        {
            InitializeComponent();
            _resourceService = new StudyResourceService();

            // Set default resource type
            ResourceTypePicker.SelectedIndex = 0;
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

        private async void OnSelectFileClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a study resource file"
                });

                if (result != null)
                {
                    _selectedFile = result;

                    // Update UI to show selected file
                    SelectedFileLabel.Text = $"📎 {result.FileName}";
                    SelectedFileLabel.TextColor = Color.FromArgb("#10b981");

                    // Auto-detect resource type
                    var extension = Path.GetExtension(result.FileName).ToLower();
                    if (extension == ".pdf")
                        ResourceTypePicker.SelectedItem = "PDF";
                    else if (extension == ".mp4" || extension == ".avi" || extension == ".mov")
                        ResourceTypePicker.SelectedItem = "Video";
                    else
                        ResourceTypePicker.SelectedItem = "Document";

                    // Auto-fill title if empty
                    if (string.IsNullOrWhiteSpace(TitleEntry.Text))
                    {
                        TitleEntry.Text = Path.GetFileNameWithoutExtension(result.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to select file: {ex.Message}", "OK");
            }
        }

        private async void OnSaveResourceClicked(object sender, EventArgs e)
        {
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

            if (ResourceTypePicker.SelectedItem == null)
            {
                await DisplayAlert("Validation Error", "Please select a resource type", "OK");
                return;
            }

            if (_selectedFile == null)
            {
                await DisplayAlert("Validation Error", "Please select a file", "OK");
                return;
            }

            // Show loading
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            SaveResourceButton.IsEnabled = false;
            SelectFileButton.IsEnabled = false;

            try
            {
                // Read file content
                byte[] fileContent;
                using (var stream = await _selectedFile.OpenReadAsync())
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    fileContent = memoryStream.ToArray();
                }

                // Validate file size (limit to 10MB for mobile)
                if (fileContent.Length > 10 * 1024 * 1024)
                {
                    await DisplayAlert("File Too Large", "Please select a file smaller than 10MB", "OK");
                    return;
                }

                // Get content type
                var contentType = GetContentType(_selectedFile.FileName);

                // Save resource
                var result = await _resourceService.CreateResourceAsync(
                    TitleEntry.Text.Trim(),
                    DescriptionEditor.Text?.Trim() ?? string.Empty,
                    SubjectEntry.Text.Trim(),
                    ResourceTypePicker.SelectedItem?.ToString() ?? "Document",
                    _selectedFile.FileName,
                    fileContent,
                    contentType,
                    _currentUserId
                );

                if (result.Success)
                {
                    await DisplayAlert("Success", result.Message, "OK");

                    // Clear form
                    ClearForm();

                    // Navigate back to resources list
                    await Shell.Current.GoToAsync("///MainTabs/StudyResourcesPage");
                }
                else
                {
                    await DisplayAlert("Error", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save resource: {ex.Message}", "OK");
            }
            finally
            {
                // Hide loading
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                SaveResourceButton.IsEnabled = true;
                SelectFileButton.IsEnabled = true;
            }
        }

        private void ClearForm()
        {
            TitleEntry.Text = string.Empty;
            SubjectEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
            ResourceTypePicker.SelectedIndex = 0;
            _selectedFile = null;
            SelectedFileLabel.Text = "No file selected";
            SelectedFileLabel.TextColor = Color.FromArgb("#6b7280");
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".mp4" => "video/mp4",
                ".avi" => "video/avi",
                ".mov" => "video/quicktime",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            bool confirm = true;

            // Check if user has entered any data
            if (!string.IsNullOrWhiteSpace(TitleEntry.Text) ||
                !string.IsNullOrWhiteSpace(SubjectEntry.Text) ||
                !string.IsNullOrWhiteSpace(DescriptionEditor.Text) ||
                _selectedFile != null)
            {
                confirm = await DisplayAlert("Discard Changes",
                    "Are you sure you want to discard your changes?",
                    "Yes", "No");
            }

            if (confirm)
            {
                await Shell.Current.GoToAsync("///MainTabs/StudyResourcesPage");
            }
        }
    }
}