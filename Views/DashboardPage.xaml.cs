using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly StudyResourceService _resourceService;
        private readonly StudyGroupService _groupService;

        public DashboardPage()
        {
            InitializeComponent();
            _resourceService = new StudyResourceService();
            _groupService = new StudyGroupService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDashboardData();
        }

        private async Task LoadDashboardData()
        {
            // Get user name from preferences
            var userName = Preferences.Get("UserName", "Student");
            WelcomeLabel.Text = $"Welcome back, {userName}!";

            // Get user ID and load stats
            var userId = Preferences.Get("UserId", 0);
            if (userId > 0)
            {
                try
                {
                    // Load resource stats
                    var resourceStats = await _resourceService.GetResourceStatsAsync(userId);
                    ResourcesCountLabel.Text = resourceStats.TotalResources.ToString();

                    // Load group stats
                    var groupStats = await _groupService.GetGroupStatsAsync(userId);
                    GroupsCountLabel.Text = groupStats.MyGroups.ToString();
                }
                catch
                {
                    ResourcesCountLabel.Text = "0";
                    GroupsCountLabel.Text = "0";
                }
            }

            // For modules not yet implemented
            PlansCountLabel.Text = "0";
            GoalsCountLabel.Text = "0";
        }

        private async void OnResourcesTapped(object sender, EventArgs e)
        {
            // Navigate to Study Resources page
            await Shell.Current.GoToAsync("///MainTabs/StudyResourcesPage");
        }

        private async void OnGroupsTapped(object sender, EventArgs e)
        {
            // Navigate to Study Groups page
            await Shell.Current.GoToAsync("///MainTabs/StudyGroupsPage");
        }

        private async void OnPlansTapped(object sender, EventArgs e)
        {
            // TODO: Navigate to Study Plans page
            await DisplayAlert("Coming Soon", "Study Plans module coming soon!", "OK");
        }
    }
}