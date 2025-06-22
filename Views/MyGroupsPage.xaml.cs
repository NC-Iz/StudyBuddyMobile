using StudyBuddyMobile.Models;
using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    public partial class MyGroupsPage : ContentPage
    {
        private readonly StudyGroupService _groupService;
        private List<StudyGroup> _allMyGroups = new();
        private List<StudyGroup> _filteredGroups = new();
        private int _currentUserId;
        private string _currentFilter = "All";

        public MyGroupsPage()
        {
            InitializeComponent();
            _groupService = new StudyGroupService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadMyGroups();
        }

        private async Task LoadMyGroups()
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
                // Load user's groups
                _allMyGroups = await _groupService.GetUserGroupsAsync(_currentUserId);

                // Calculate statistics
                var totalGroups = _allMyGroups.Count;
                var createdGroups = _allMyGroups.Count(g => g.CreatedBy == _currentUserId);
                var joinedGroups = totalGroups - createdGroups;

                // Update statistics
                TotalMyGroupsLabel.Text = totalGroups.ToString();
                CreatedGroupsLabel.Text = createdGroups.ToString();
                JoinedGroupsLabel.Text = joinedGroups.ToString();

                // Apply current filter and display
                ApplyFilter(_currentFilter);
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

        private void ApplyFilter(string filter)
        {
            _currentFilter = filter;

            // Update filter button styles
            ResetFilterButtons();

            switch (filter)
            {
                case "All":
                    _filteredGroups = _allMyGroups;
                    AllGroupsFilter.BackgroundColor = Color.FromArgb("#4f46e5");
                    AllGroupsFilter.TextColor = Colors.White;
                    break;

                case "Created":
                    _filteredGroups = _allMyGroups.Where(g => g.CreatedBy == _currentUserId).ToList();
                    CreatedFilter.BackgroundColor = Color.FromArgb("#f59e0b");
                    CreatedFilter.TextColor = Colors.White;
                    break;

                case "Joined":
                    _filteredGroups = _allMyGroups.Where(g => g.CreatedBy != _currentUserId).ToList();
                    JoinedFilter.BackgroundColor = Color.FromArgb("#10b981");
                    JoinedFilter.TextColor = Colors.White;
                    break;
            }

            DisplayGroups(_filteredGroups);
        }

        private void ResetFilterButtons()
        {
            // Reset all filter buttons to inactive state with better contrast
            AllGroupsFilter.BackgroundColor = Color.FromArgb("#f3f4f6"); // Light gray
            AllGroupsFilter.TextColor = Color.FromArgb("#374151"); // Dark gray text

            CreatedFilter.BackgroundColor = Color.FromArgb("#f3f4f6");
            CreatedFilter.TextColor = Color.FromArgb("#374151");

            JoinedFilter.BackgroundColor = Color.FromArgb("#f3f4f6");
            JoinedFilter.TextColor = Color.FromArgb("#374151");
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
                    var groupCard = CreateMyGroupCard(group);
                    GroupsContainer.Children.Add(groupCard);
                }

                // Hide empty state
                EmptyStateFrame.IsVisible = false;
                GroupsContainer.Children.Add(EmptyStateFrame);
            }
            else
            {
                // Update empty state message based on filter
                UpdateEmptyStateMessage();

                // Show empty state
                GroupsContainer.Children.Add(EmptyStateFrame);
                EmptyStateFrame.IsVisible = true;
            }
        }

        private void UpdateEmptyStateMessage()
        {
            switch (_currentFilter)
            {
                case "Created":
                    EmptyStateMessage.Text = "You haven't created any study groups yet. Create your first group to start building your learning community!";
                    break;
                case "Joined":
                    EmptyStateMessage.Text = "You haven't joined any study groups yet. Browse public groups to find communities that match your interests!";
                    break;
                default:
                    EmptyStateMessage.Text = "You haven't joined any study groups yet. Browse public groups or create your own!";
                    break;
            }
        }

        private Frame CreateMyGroupCard(StudyGroup group)
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

            // Header with name, role badge, and subject
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

            // Role badge
            var isCreator = group.CreatedBy == _currentUserId;
            var roleBadge = new Frame
            {
                BackgroundColor = isCreator ? Color.FromArgb("#fef3c7") : Color.FromArgb("#d1fae5"),
                CornerRadius = 15,
                Padding = new Thickness(8, 4),
                HasShadow = false
            };

            var roleLabel = new Label
            {
                Text = isCreator ? "👑 Creator" : "👤 Member",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = isCreator ? Color.FromArgb("#92400e") : Color.FromArgb("#065f46")
            };

            roleBadge.Content = roleLabel;
            headerLayout.Children.Add(nameLabel);
            headerLayout.Children.Add(roleBadge);

            // Subject badge
            var subjectBadge = new Frame
            {
                BackgroundColor = Color.FromArgb("#dbeafe"),
                CornerRadius = 15,
                Padding = new Thickness(10, 5),
                HasShadow = false,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 5)
            };

            var subjectLabel = new Label
            {
                Text = $"📚 {group.Subject}",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1e40af")
            };

            subjectBadge.Content = subjectLabel;

            // Description
            if (!string.IsNullOrEmpty(group.Description))
            {
                var descriptionLabel = new Label
                {
                    Text = group.Description.Length > 120
                        ? group.Description.Substring(0, 120) + "..."
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
                Margin = new Thickness(0, 8)
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
                Text = $"📅 {group.CreatedDate:MMM dd, yyyy}",
                FontSize = 12,
                TextColor = Color.FromArgb("#6b7280")
            };

            var statusLabel = new Label
            {
                Text = group.IsPublic ? "🌐 Public" : "🔒 Private",
                FontSize = 12,
                TextColor = Color.FromArgb("#6b7280")
            };

            detailsLayout.Children.Add(membersLabel);
            detailsLayout.Children.Add(createdLabel);
            detailsLayout.Children.Add(statusLabel);

            // Action buttons
            var buttonsLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var viewButton = new Button
            {
                Text = "👁️ View",
                FontSize = 12,
                BackgroundColor = Color.FromArgb("#4f46e5"),
                TextColor = Colors.White,
                CornerRadius = 6,
                HeightRequest = 35,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            viewButton.Clicked += async (s, e) => await OnViewGroupClicked(group);

            Button actionButton;
            if (isCreator)
            {
                actionButton = new Button
                {
                    Text = "⚙️ Manage",
                    FontSize = 12,
                    BackgroundColor = Color.FromArgb("#f59e0b"),
                    TextColor = Colors.White,
                    CornerRadius = 6,
                    HeightRequest = 35,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                actionButton.Clicked += async (s, e) => await OnManageGroupClicked(group);
            }
            else
            {
                actionButton = new Button
                {
                    Text = "🚪 Leave",
                    FontSize = 12,
                    BackgroundColor = Color.FromArgb("#ef4444"),
                    TextColor = Colors.White,
                    CornerRadius = 6,
                    HeightRequest = 35,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                actionButton.Clicked += async (s, e) => await OnLeaveGroupClicked(group);
            }

            buttonsLayout.Children.Add(viewButton);
            buttonsLayout.Children.Add(actionButton);

            // Assemble the card
            mainLayout.Children.Add(headerLayout);
            mainLayout.Children.Add(subjectBadge);
            mainLayout.Children.Add(detailsLayout);
            mainLayout.Children.Add(buttonsLayout);

            frame.Content = mainLayout;
            return frame;
        }

        private async Task OnViewGroupClicked(StudyGroup group)
        {
            // Navigate to group details page
            await Shell.Current.GoToAsync($"GroupDetailsPage?groupId={group.Id}");
        }

        private async Task OnManageGroupClicked(StudyGroup group)
        {
            // Show action sheet for group management
            var action = await DisplayActionSheet("Manage Group", "Cancel", null,
                "Edit Group Info", "View Members", "Delete Group");

            switch (action)
            {
                case "Edit Group Info":
                    await DisplayAlert("Coming Soon", "Edit group functionality coming soon!", "OK");
                    break;
                case "View Members":
                    await OnViewGroupClicked(group);
                    break;
                case "Delete Group":
                    await OnDeleteGroupClicked(group);
                    break;
            }
        }

        private async Task OnLeaveGroupClicked(StudyGroup group)
        {
            bool confirm = await DisplayAlert("Leave Group",
                $"Are you sure you want to leave '{group.Name}'? You can rejoin later if it's a public group.",
                "Leave", "Cancel");

            if (confirm)
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                var result = await _groupService.LeaveGroupAsync(group.Id, _currentUserId);

                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;

                if (result.Success)
                {
                    await DisplayAlert("Success", result.Message, "OK");
                    await LoadMyGroups(); // Refresh the list
                }
                else
                {
                    await DisplayAlert("Error", result.Message, "OK");
                }
            }
        }

        private async Task OnDeleteGroupClicked(StudyGroup group)
        {
            bool confirm = await DisplayAlert("Delete Group",
                $"Are you sure you want to delete '{group.Name}'? This action cannot be undone and will remove all members and messages.",
                "Delete", "Cancel");

            if (confirm)
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                var result = await _groupService.DeleteGroupAsync(group.Id, _currentUserId);

                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;

                if (result.Success)
                {
                    await DisplayAlert("Success", result.Message, "OK");
                    await LoadMyGroups(); // Refresh the list
                }
                else
                {
                    await DisplayAlert("Error", result.Message, "OK");
                }
            }
        }

        private void OnFilterClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var filter = button?.Text ?? "All";
            ApplyFilter(filter);
        }

        private async void OnCreateGroupClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("CreateGroupPage");
        }

        private async void OnBrowseGroupsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///MainTabs/StudyGroupsPage");
        }
    }
}