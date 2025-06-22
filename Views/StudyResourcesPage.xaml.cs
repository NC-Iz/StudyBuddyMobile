using StudyBuddyMobile.Models;
using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    public partial class StudyResourcesPage : ContentPage
    {
        private readonly StudyResourceService _resourceService;
        private List<StudyResource> _allResources = new();
        private int _currentUserId;

        public StudyResourcesPage()
        {
            InitializeComponent();
            _resourceService = new StudyResourceService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Always reload resources when the page appears
            await LoadResources();
        }

        private async Task LoadResources()
        {
            // Get current user ID
            _currentUserId = Preferences.Get("UserId", 0);
            if (_currentUserId == 0)
            {
                await Shell.Current.GoToAsync("///LoginPage");
                return;
            }

            // Show loading
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                // Load resources and statistics
                _allResources = await _resourceService.GetUserResourcesAsync(_currentUserId);
                var stats = await _resourceService.GetResourceStatsAsync(_currentUserId);

                // Update statistics
                TotalResourcesLabel.Text = stats.TotalResources.ToString();
                PdfCountLabel.Text = stats.PdfCount.ToString();
                VideoCountLabel.Text = stats.VideoCount.ToString();
                SubjectsCountLabel.Text = stats.SubjectCount.ToString();

                // Display resources
                DisplayResources(_allResources);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load resources: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private void DisplayResources(List<StudyResource> resources)
        {
            // Clear ALL existing children first
            ResourcesContainer.Children.Clear();

            if (resources.Any())
            {
                // Add resource cards
                foreach (var resource in resources)
                {
                    var resourceCard = CreateResourceCard(resource);
                    ResourcesContainer.Children.Add(resourceCard);
                }

                // Make sure empty state is not visible
                EmptyStateFrame.IsVisible = false;

                // Add empty state frame at the end (but keep it invisible)
                ResourcesContainer.Children.Add(EmptyStateFrame);
            }
            else
            {
                // Add and show empty state
                ResourcesContainer.Children.Add(EmptyStateFrame);
                EmptyStateFrame.IsVisible = true;
            }
        }

        private Frame CreateResourceCard(StudyResource resource)
        {
            var frame = new Frame
            {
                BackgroundColor = Colors.White,
                CornerRadius = 12,
                Padding = 15,
                HasShadow = true,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var mainLayout = new VerticalStackLayout { Spacing = 10 };

            // Header with title and type badge
            var headerLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10
            };

            var titleLabel = new Label
            {
                Text = resource.Title,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1f2937"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            var typeIcon = resource.ResourceType switch
            {
                "PDF" => "📄",
                "Video" => "🎥",
                _ => "📄"
            };

            var typeBadge = new Frame
            {
                BackgroundColor = resource.ResourceType == "PDF"
                    ? Color.FromArgb("#fef3c7")
                    : Color.FromArgb("#ddd6fe"),
                CornerRadius = 15,
                Padding = new Thickness(10, 5),
                HasShadow = false
            };

            var badgeLabel = new Label
            {
                Text = $"{typeIcon} {resource.ResourceType}",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = resource.ResourceType == "PDF"
                    ? Color.FromArgb("#92400e")
                    : Color.FromArgb("#5b21b6")
            };

            typeBadge.Content = badgeLabel;
            headerLayout.Children.Add(titleLabel);
            headerLayout.Children.Add(typeBadge);

            // Description
            if (!string.IsNullOrEmpty(resource.Description))
            {
                var descriptionLabel = new Label
                {
                    Text = resource.Description.Length > 100
                        ? resource.Description.Substring(0, 100) + "..."
                        : resource.Description,
                    FontSize = 14,
                    TextColor = Color.FromArgb("#6b7280"),
                    Margin = new Thickness(0, 5)
                };
                mainLayout.Children.Add(descriptionLabel);
            }

            // Details
            var detailsLayout = new VerticalStackLayout { Spacing = 3 };

            var subjectLabel = new Label
            {
                Text = $"📚 {resource.Subject}",
                FontSize = 12,
                TextColor = Color.FromArgb("#4f46e5"),
                FontAttributes = FontAttributes.Bold
            };

            var fileLabel = new Label
            {
                Text = $"📎 {resource.FileName}",
                FontSize = 12,
                TextColor = Color.FromArgb("#6b7280")
            };

            var sizeLabel = new Label
            {
                Text = $"📊 {FormatFileSize(resource.FileSize)} • {resource.CreatedDate:MMM dd, yyyy}",
                FontSize = 12,
                TextColor = Color.FromArgb("#6b7280")
            };

            detailsLayout.Children.Add(subjectLabel);
            detailsLayout.Children.Add(fileLabel);
            detailsLayout.Children.Add(sizeLabel);

            // Action buttons
            var buttonsLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var downloadButton = new Button
            {
                Text = "📥 Download",
                FontSize = 12,
                BackgroundColor = Color.FromArgb("#10b981"),
                TextColor = Colors.White,
                CornerRadius = 6,
                HeightRequest = 35,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            downloadButton.Clicked += async (s, e) => await OnDownloadResourceClicked(resource);

            var editButton = new Button
            {
                Text = "✏️ Edit",
                FontSize = 12,
                BackgroundColor = Color.FromArgb("#f59e0b"),
                TextColor = Colors.White,
                CornerRadius = 6,
                HeightRequest = 35,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            editButton.Clicked += async (s, e) => await OnEditResourceClicked(resource);

            var deleteButton = new Button
            {
                Text = "🗑️",
                FontSize = 12,
                BackgroundColor = Color.FromArgb("#ef4444"),
                TextColor = Colors.White,
                CornerRadius = 6,
                HeightRequest = 35,
                WidthRequest = 45
            };
            deleteButton.Clicked += async (s, e) => await OnDeleteResourceClicked(resource);

            buttonsLayout.Children.Add(downloadButton);
            buttonsLayout.Children.Add(editButton);
            buttonsLayout.Children.Add(deleteButton);

            // Assemble the card
            mainLayout.Children.Add(headerLayout);
            mainLayout.Children.Add(detailsLayout);
            mainLayout.Children.Add(buttonsLayout);

            frame.Content = mainLayout;
            return frame;
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024:F1} KB";
            return $"{bytes / (1024 * 1024):F1} MB";
        }

        private async void OnAddResourceClicked(object sender, EventArgs e)
        {
            // Navigate to Add Resource page
            await Shell.Current.GoToAsync("AddResourcePage");
        }

        private async Task OnDownloadResourceClicked(StudyResource resource)
        {
            try
            {
                var fileData = await _resourceService.GetFileForDownloadAsync(resource.Id, _currentUserId);

                if (fileData.FileContent != null && fileData.FileName != null)
                {
                    // Try to save to Downloads folder
                    var result = await SaveToDownloadsFolder(fileData.FileContent, fileData.FileName);

                    if (result.Success)
                    {
                        await DisplayAlert("Download Complete",
                            $"File saved successfully!\n\nFile: {fileData.FileName}\n{result.Message}",
                            "OK");
                    }
                    else
                    {
                        await DisplayAlert("Download Failed", result.Message, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "File not found", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Download failed: {ex.Message}", "OK");
            }
        }

        private async Task<(bool Success, string Message)> SaveToDownloadsFolder(byte[] fileContent, string fileName)
        {
            try
            {
                // Try multiple download locations
                string[] downloadPaths = {
                    // Android Downloads folder
                    Path.Combine("/storage/emulated/0/Download", fileName),
                    // User Downloads folder (Windows/Mac)
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName),
                    // Documents folder fallback
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName),
                    // App documents folder as last resort
                    Path.Combine(FileSystem.AppDataDirectory, fileName)
                };

                foreach (var path in downloadPaths)
                {
                    try
                    {
                        var directory = Path.GetDirectoryName(path);
                        if (!string.IsNullOrEmpty(directory))
                        {
                            // Create directory if it doesn't exist
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            await File.WriteAllBytesAsync(path, fileContent);
                            return (true, $"Saved to: {path}");
                        }
                    }
                    catch
                    {
                        // Continue to next path if this one fails
                        continue;
                    }
                }

                return (false, "Could not save to any download location");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to save: {ex.Message}");
            }
        }

        private async Task<(bool Success, string Message)> SaveFileDirectly(byte[] fileContent, string fileName)
        {
            try
            {
                // Try saving to different locations (cross-platform)
                string[] possiblePaths = {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName),
                    Path.Combine(FileSystem.AppDataDirectory, fileName)
                };

                foreach (var path in possiblePaths)
                {
                    try
                    {
                        var directory = Path.GetDirectoryName(path);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        await File.WriteAllBytesAsync(path, fileContent);
                        return (true, $"File saved to: {path}");
                    }
                    catch
                    {
                        continue; // Try next path
                    }
                }

                // If all fail, save to app cache as last resort
                var cachePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllBytesAsync(cachePath, fileContent);
                return (true, $"File saved to app storage: {cachePath}");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to save: {ex.Message}");
            }
        }

        private async Task OnEditResourceClicked(StudyResource resource)
        {
            // Navigate to Edit Resource page with the resource ID
            await Shell.Current.GoToAsync($"EditResourcePage?resourceId={resource.Id}");
        }

        private async Task OnDeleteResourceClicked(StudyResource resource)
        {
            bool confirm = await DisplayAlert("Delete Resource",
                $"Are you sure you want to delete '{resource.Title}'? This action cannot be undone.",
                "Delete", "Cancel");

            if (confirm)
            {
                var result = await _resourceService.DeleteResourceAsync(resource.Id, _currentUserId);

                if (result.Success)
                {
                    await DisplayAlert("Success", result.Message, "OK");
                    await LoadResources(); // Refresh the list
                }
                else
                {
                    await DisplayAlert("Error", result.Message, "OK");
                }
            }
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.Trim() ?? "";

            if (string.IsNullOrEmpty(searchText))
            {
                DisplayResources(_allResources);
            }
            else
            {
                var filteredResources = _allResources
                    .Where(r => r.Subject.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                               r.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                DisplayResources(filteredResources);
            }
        }
    }
}