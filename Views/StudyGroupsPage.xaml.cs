using StudyBuddyMobile.Models;
using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    public partial class StudyGroupsPage : ContentPage
    {
        private readonly StudyGroupService _groupService;
        private List<StudyGroup> _allGroups = new();
        private int _currentUserId;

        public StudyGroupsPage()
        {
            InitializeComponent();
            _groupService = new StudyGroupService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadGroups();
        }

        private async Task LoadGroups()
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
                // Load public groups and statistics
                _allGroups = await _groupService.GetPublicGroupsAsync();
                var stats = await _groupService.GetGroupStatsAsync(_currentUserId);

                // Update statistics
                TotalGroupsLabel.Text = stats.TotalGroups.ToString();
                MyGroupsCountLabel.Text = stats.MyGroups.ToString();
                CreatedGroupsLabel.Text = stats.CreatedGroups.ToString();

                // Display groups
                DisplayGroups(_allGroups);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load groups: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private void DisplayGroups(List<StudyGroup> groups)
        {
            // Clear existing children
            GroupsContainer.Children.Clear();

            if (groups.Any())
            {
                // Add group cards
                foreach (var group in groups)
                {
                    var groupCard = CreateGroupCard(group);
                    GroupsContainer.Children.Add(groupCard);
                }

                // Hide empty state
                EmptyStateFrame.IsVisible = false;
                GroupsContainer.Children.Add(EmptyStateFrame);
            }
            else
            {
                // Show empty state
                GroupsContainer.Children.Add(EmptyStateFrame);
                EmptyStateFrame.IsVisible = true;
            }
        }

        private Frame CreateGroupCard(StudyGroup group)
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

            // Header with name and subject badge
            var headerLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10
            };

            var nameLabel = new Label
            {
                Text = group.Name,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1f2937"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            var subjectBadge = new Frame
            {
                BackgroundColor = Color.FromArgb("#dbeafe"),
                CornerRadius = 15,
                Padding = new Thickness(10, 5),
                HasShadow = false
            };

            var subjectLabel = new Label
            {
                Text = $"📚 {group.Subject}",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1e40af")
            };

            subjectBadge.Content = subjectLabel;
            headerLayout.Children.Add(nameLabel);
            headerLayout.Children.Add(subjectBadge);

            // Description
            if (!string.IsNullOrEmpty(group.Description))
            {
                var descriptionLabel = new Label
                {
                    Text = group.Description.Length > 100
                        ? group.Description.Substring(0, 100) + "..."
                        : group.Description,
                    FontSize = 14,
                    TextColor = Color.FromArgb("#6b7280"),
                    Margin = new Thickness(0, 5)
                };
                mainLayout.Children.Add(descriptionLabel);
            }

            // Group details
            var detailsLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 15,
                Margin = new Thickness(0, 5)
            };

            var membersLabel = new Label
            {
                Text = $"👥 {group.Members?.Count ?? 0}/{group.MaxMembers}",
                FontSize = 12,
                TextColor = Color.FromArgb("#10b981"),
                FontAttributes = FontAttributes.Bold
            };

            var createdLabel = new Label
            {
                Text = $"📅 {group.CreatedDate:MMM dd}",
                FontSize = 12,
                TextColor = Color.FromArgb("#6b7280")
            };

            var creatorLabel = new Label
            {
                Text = $"👤 {group.Creator?.Name ?? "Unknown"}",
                FontSize = 12,
                TextColor = Color.FromArgb("#6b7280")
            };

            detailsLayout.Children.Add(membersLabel);
            detailsLayout.Children.Add(createdLabel);
            detailsLayout.Children.Add(creatorLabel);

            // Progress bar for member capacity
            var progressFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#f3f4f6"),
                CornerRadius = 4,
                Padding = 0,
                HeightRequest = 6,
                HasShadow = false
            };

            var progressBar = new Frame
            {
                BackgroundColor = Color.FromArgb("#10b981"),
                CornerRadius = 4,
                Padding = 0,
                HeightRequest = 6,
                HasShadow = false,
                HorizontalOptions = LayoutOptions.Start
            };

            var memberProgress = (double)(group.Members?.Count ?? 0) / group.MaxMembers;
            progressBar.WidthRequest = 100 * memberProgress; // Assume 100 is full width

            progressFrame.Content = progressBar;

            // Action buttons
            var buttonsLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var viewButton = new Button
            {
                Text = "👁️ View Details",
                FontSize = 12,
                BackgroundColor = Color.FromArgb("#4f46e5"),
                TextColor = Colors.White,
                CornerRadius = 6,
                HeightRequest = 35,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            viewButton.Clicked += async (s, e) => await OnViewGroupClicked(group);

            // Check if user is already a member
            var isMember = group.Members?.Any(m => m.UserId == _currentUserId) ?? false;
            var isCreator = group.CreatedBy == _currentUserId;

            Button actionButton;
            if (isMember)
            {
                actionButton = new Button
                {
                    Text = isCreator ? "👑 Creator" : "✅ Member",
                    FontSize = 12,
                    BackgroundColor = Color.FromArgb("#10b981"),
                    TextColor = Colors.White,
                    CornerRadius = 6,
                    HeightRequest = 35,
                    IsEnabled = false,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
            }
            else if ((group.Members?.Count ?? 0) >= group.MaxMembers)
            {
                actionButton = new Button
                {
                    Text = "🚫 Full",
                    FontSize = 12,
                    BackgroundColor = Color.FromArgb("#6b7280"),
                    TextColor = Colors.White,
                    CornerRadius = 6,
                    HeightRequest = 35,
                    IsEnabled = false,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
            }
            else
            {
                actionButton = new Button
                {
                    Text = "➕ Join Group",
                    FontSize = 12,
                    BackgroundColor = Color.FromArgb("#10b981"),
                    TextColor = Colors.White,
                    CornerRadius = 6,
                    HeightRequest = 35,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                actionButton.Clicked += async (s, e) => await OnJoinGroupClicked(group);
            }

            buttonsLayout.Children.Add(viewButton);
            buttonsLayout.Children.Add(actionButton);

            // Assemble the card
            mainLayout.Children.Add(headerLayout);
            mainLayout.Children.Add(detailsLayout);
            mainLayout.Children.Add(progressFrame);
            mainLayout.Children.Add(buttonsLayout);

            frame.Content = mainLayout;
            return frame;
        }

        private async Task OnViewGroupClicked(StudyGroup group)
        {
            // Navigate to group details page
            await Shell.Current.GoToAsync($"GroupDetailsPage?groupId={group.Id}");
        }

        private async Task OnJoinGroupClicked(StudyGroup group)
        {
            bool confirm = await DisplayAlert("Join Group",
                $"Do you want to join '{group.Name}'?",
                "Join", "Cancel");

            if (confirm)
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                var result = await _groupService.JoinGroupAsync(group.Id, _currentUserId);

                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;

                if (result.Success)
                {
                    await DisplayAlert("Success", result.Message, "OK");
                    await LoadGroups(); // Refresh the list
                }
                else
                {
                    await DisplayAlert("Error", result.Message, "OK");
                }
            }
        }

        private async void OnCreateGroupClicked(object sender, EventArgs e)
        {
            // Navigate to create group page
            await Shell.Current.GoToAsync("CreateGroupPage");
        }

        private async void OnMyGroupsClicked(object sender, EventArgs e)
        {
            // Navigate to my groups page
            await Shell.Current.GoToAsync("MyGroupsPage");
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.Trim() ?? "";

            if (string.IsNullOrEmpty(searchText))
            {
                DisplayGroups(_allGroups);
            }
            else
            {
                var filteredGroups = _allGroups
                    .Where(g => g.Subject.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                               g.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                DisplayGroups(filteredGroups);
            }
        }
    }
}