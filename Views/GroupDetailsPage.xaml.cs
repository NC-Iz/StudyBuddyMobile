using StudyBuddyMobile.Models;
using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    [QueryProperty(nameof(GroupId), "groupId")]
    public partial class GroupDetailsPage : ContentPage
    {
        private readonly StudyGroupService _groupService;
        private StudyGroup? _currentGroup;
        private int _currentUserId;
        private int _groupId;
        private string _currentTab = "Chat";
        private bool _isMember = false;
        private bool _isCreator = false;

        public int GroupId
        {
            get => _groupId;
            set => _groupId = value;
        }

        public GroupDetailsPage()
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
                    DisplayGroupInfo();
                    SetupUserActions();
                    DisplayMessages();
                    DisplayMembers();
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

        private void DisplayGroupInfo()
        {
            if (_currentGroup == null) return;

            // Update header
            GroupNameLabel.Text = _currentGroup.Name;
            SubjectLabel.Text = $"📚 {_currentGroup.Subject}";
            MemberCountLabel.Text = $"{_currentGroup.Members?.Count ?? 0}/{_currentGroup.MaxMembers}";
            MessageCountLabel.Text = (_currentGroup.Messages?.Count ?? 0).ToString();

            // Check user membership
            _isMember = _currentGroup.Members?.Any(m => m.UserId == _currentUserId) ?? false;
            _isCreator = _currentGroup.CreatedBy == _currentUserId;

            // Update status
            if (_isCreator)
                StatusLabel.Text = "👑 Creator";
            else if (_isMember)
                StatusLabel.Text = "👤 Member";
            else
                StatusLabel.Text = "👋 Visitor";

            // Show description in members tab
            if (!string.IsNullOrEmpty(_currentGroup.Description))
            {
                DescriptionFrame.IsVisible = true;
                DescriptionLabel.Text = _currentGroup.Description;
            }
        }

        private void SetupUserActions()
        {
            // Hide all action containers first
            JoinButton.IsVisible = false;
            MemberActions.IsVisible = false;
            CreatorActions.IsVisible = false;

            if (_isCreator)
            {
                CreatorActions.IsVisible = true;
            }
            else if (_isMember)
            {
                MemberActions.IsVisible = true;
            }
            else
            {
                // Non-member - show join button if group isn't full
                var memberCount = _currentGroup?.Members?.Count ?? 0;
                var maxMembers = _currentGroup?.MaxMembers ?? 0;

                if (memberCount < maxMembers)
                {
                    JoinButton.IsVisible = true;
                }
            }
        }

        private void DisplayMessages()
        {
            MessagesContainer.Children.Clear();

            if (_currentGroup?.Messages?.Any() == true && _isMember)
            {
                EmptyChatFrame.IsVisible = false;

                foreach (var message in _currentGroup.Messages.OrderBy(m => m.SentDate))
                {
                    var messageCard = CreateMessageCard(message);
                    MessagesContainer.Children.Add(messageCard);
                }

                // Add empty chat frame at the end (but keep it invisible)
                MessagesContainer.Children.Add(EmptyChatFrame);

                // Scroll to bottom
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(100);
                    await MessagesScrollView.ScrollToAsync(0, MessagesContainer.Height, false);
                });
            }
            else
            {
                MessagesContainer.Children.Add(EmptyChatFrame);
                EmptyChatFrame.IsVisible = true;
            }
        }

        private Frame CreateMessageCard(GroupMessage message)
        {
            var isMyMessage = message.UserId == _currentUserId;

            var frame = new Frame
            {
                BackgroundColor = isMyMessage ? Color.FromArgb("#4f46e5") : Color.FromArgb("#f3f4f6"),
                CornerRadius = 15,
                Padding = 10,
                HasShadow = false,
                HorizontalOptions = isMyMessage ? LayoutOptions.End : LayoutOptions.Start,
                WidthRequest = -1,
                Margin = new Thickness(isMyMessage ? 50 : 0, 5, isMyMessage ? 0 : 50, 5)
            };

            var layout = new VerticalStackLayout { Spacing = 3 };

            // Sender name (only for other people's messages)
            if (!isMyMessage)
            {
                var senderLabel = new Label
                {
                    Text = message.User?.Name ?? "Unknown",
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#6b7280")
                };
                layout.Children.Add(senderLabel);
            }

            // Message text
            var messageLabel = new Label
            {
                Text = message.Message,
                FontSize = 14,
                TextColor = isMyMessage ? Colors.White : Color.FromArgb("#1f2937"),
                LineBreakMode = LineBreakMode.WordWrap
            };
            layout.Children.Add(messageLabel);

            // Timestamp
            var timeLabel = new Label
            {
                Text = message.SentDate.ToString("MMM dd, HH:mm"),
                FontSize = 10,
                TextColor = isMyMessage ? Color.FromArgb("rgba(255,255,255,0.7)") : Color.FromArgb("#9ca3af"),
                HorizontalOptions = LayoutOptions.End
            };
            layout.Children.Add(timeLabel);

            frame.Content = layout;
            return frame;
        }

        private void DisplayMembers()
        {
            // Clear existing members (keep description frame)
            var itemsToRemove = MembersContainer.Children
                .Where(child => child != DescriptionFrame)
                .ToList();

            foreach (var item in itemsToRemove)
            {
                MembersContainer.Children.Remove(item);
            }

            if (_currentGroup?.Members?.Any() == true)
            {
                foreach (var member in _currentGroup.Members.OrderBy(m => m.Role == "Creator" ? 0 : 1))
                {
                    var memberCard = CreateMemberCard(member);
                    MembersContainer.Children.Add(memberCard);
                }
            }
        }

        private Frame CreateMemberCard(StudyGroupMember member)
        {
            var frame = new Frame
            {
                BackgroundColor = Colors.White,
                CornerRadius = 10,
                Padding = 15,
                HasShadow = false,
                Margin = new Thickness(0, 5)
            };

            var layout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 15
            };

            // Avatar
            var avatarFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#4f46e5"),
                CornerRadius = 25,
                Padding = 0,
                WidthRequest = 50,
                HeightRequest = 50,
                HasShadow = false
            };

            var avatarLabel = new Label
            {
                Text = member.User?.Name?.Substring(0, 1).ToUpper() ?? "?",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            avatarFrame.Content = avatarLabel;

            // Member info
            var infoLayout = new VerticalStackLayout
            {
                Spacing = 3,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            var nameLabel = new Label
            {
                Text = member.User?.Name ?? "Unknown User",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1f2937")
            };

            var roleLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 8
            };

            var roleBadge = new Frame
            {
                BackgroundColor = member.Role == "Creator" ? Color.FromArgb("#fef3c7") : Color.FromArgb("#dbeafe"),
                CornerRadius = 12,
                Padding = new Thickness(8, 4),
                HasShadow = false
            };

            var roleLabel = new Label
            {
                Text = member.Role == "Creator" ? "👑 Creator" : "👤 Member",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = member.Role == "Creator" ? Color.FromArgb("#92400e") : Color.FromArgb("#1e40af")
            };

            roleBadge.Content = roleLabel;

            var joinedLabel = new Label
            {
                Text = $"Joined {member.JoinedDate:MMM dd, yyyy}",
                FontSize = 11,
                TextColor = Color.FromArgb("#6b7280"),
                VerticalOptions = LayoutOptions.Center
            };

            roleLayout.Children.Add(roleBadge);
            roleLayout.Children.Add(joinedLabel);

            infoLayout.Children.Add(nameLabel);
            infoLayout.Children.Add(roleLayout);

            layout.Children.Add(avatarFrame);
            layout.Children.Add(infoLayout);

            frame.Content = layout;
            return frame;
        }

        private void OnTabClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var tab = button?.Text?.Contains("Chat") == true ? "Chat" : "Members";

            _currentTab = tab;

            // Update tab button styles
            if (tab == "Chat")
            {
                ChatTabButton.BackgroundColor = Color.FromArgb("#4f46e5");
                ChatTabButton.TextColor = Colors.White;
                MembersTabButton.BackgroundColor = Color.FromArgb("#e5e7eb");
                MembersTabButton.TextColor = Color.FromArgb("#6b7280");

                ChatTabContent.IsVisible = true;
                MembersTabContent.IsVisible = false;
            }
            else
            {
                MembersTabButton.BackgroundColor = Color.FromArgb("#4f46e5");
                MembersTabButton.TextColor = Colors.White;
                ChatTabButton.BackgroundColor = Color.FromArgb("#e5e7eb");
                ChatTabButton.TextColor = Color.FromArgb("#6b7280");

                ChatTabContent.IsVisible = false;
                MembersTabContent.IsVisible = true;
            }
        }

        private async void OnSendMessageClicked(object sender, EventArgs e)
        {
            var messageText = MessageEntry.Text?.Trim();

            if (string.IsNullOrEmpty(messageText) || !_isMember)
                return;

            // Disable send button temporarily
            SendButton.IsEnabled = false;

            try
            {
                var result = await _groupService.SendMessageAsync(_groupId, _currentUserId, messageText);

                if (result.Success)
                {
                    MessageEntry.Text = string.Empty;
                    await LoadGroupDetails(); // Refresh to show new message
                }
                else
                {
                    await DisplayAlert("Error", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to send message: {ex.Message}", "OK");
            }
            finally
            {
                SendButton.IsEnabled = true;
            }
        }

        private async void OnJoinGroupClicked(object sender, EventArgs e)
        {
            var result = await _groupService.JoinGroupAsync(_groupId, _currentUserId);

            if (result.Success)
            {
                await DisplayAlert("Success", result.Message, "OK");
                await LoadGroupDetails(); // Refresh to update member status
            }
            else
            {
                await DisplayAlert("Error", result.Message, "OK");
            }
        }

        private async void OnLeaveGroupClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Leave Group",
                $"Are you sure you want to leave '{_currentGroup?.Name}'?",
                "Leave", "Cancel");

            if (confirm)
            {
                var result = await _groupService.LeaveGroupAsync(_groupId, _currentUserId);

                if (result.Success)
                {
                    await DisplayAlert("Success", result.Message, "OK");
                    await Shell.Current.GoToAsync(".."); // Go back to previous page
                }
                else
                {
                    await DisplayAlert("Error", result.Message, "OK");
                }
            }
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadGroupDetails();
        }

        private async void OnManageGroupClicked(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Manage Group", "Cancel", null,
                "Edit Group Info", "View Group Settings");

            switch (action)
            {
                case "Edit Group Info":
                    await OnEditGroupClicked();
                    break;
                case "View Group Settings":
                    await DisplayAlert("Group Settings",
                        $"Created: {_currentGroup?.CreatedDate:MMM dd, yyyy}\n" +
                        $"Visibility: {(_currentGroup?.IsPublic == true ? "Public" : "Private")}\n" +
                        $"Max Members: {_currentGroup?.MaxMembers}", "OK");
                    break;
            }
        }

        private async Task OnEditGroupClicked()
        {
            if (_currentGroup == null) return;

            // Navigate to edit group page
            await Shell.Current.GoToAsync($"EditGroupPage?groupId={_currentGroup.Id}");
        }

        private async void OnDeleteGroupClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Delete Group",
                $"Are you sure you want to delete '{_currentGroup?.Name}'? This will remove all members and messages permanently.",
                "Delete", "Cancel");

            if (confirm)
            {
                var result = await _groupService.DeleteGroupAsync(_groupId, _currentUserId);

                if (result.Success)
                {
                    await DisplayAlert("Success", result.Message, "OK");
                    await Shell.Current.GoToAsync(".."); // Go back
                }
                else
                {
                    await DisplayAlert("Error", result.Message, "OK");
                }
            }
        }
    }
}