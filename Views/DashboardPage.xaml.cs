using StudyBuddyMobile.Services;

namespace StudyBuddyMobile.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly StudyResourceService _resourceService;

        public DashboardPage()
        {
            InitializeComponent();
            _resourceService = new StudyResourceService();
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
                    var stats = await _resourceService.GetResourceStatsAsync(userId);
                    ResourcesCountLabel.Text = stats.TotalResources.ToString();
                }
                catch
                {
                    ResourcesCountLabel.Text = "0";
                }
            }

            // For modules not yet implemented
            GroupsCountLabel.Text = "0";
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
            // TODO: Navigate to Study Groups page
            await DisplayAlert("Coming Soon", "Study Groups module coming soon!", "OK");
        }

        private async void OnPlansTapped(object sender, EventArgs e)
        {
            // TODO: Navigate to Study Plans page
            await DisplayAlert("Coming Soon", "Study Plans module coming soon!", "OK");
        }
    }
}