namespace StudyBuddyMobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Check if user is already logged in
            var userId = Preferences.Get("UserId", 0);
            if (userId > 0)
            {
                // User is logged in, go to main tabs
                MainPage = new AppShell();
                Shell.Current.GoToAsync("///MainTabs/DashboardPage");
            }
            else
            {
                // User not logged in, show login page
                MainPage = new AppShell();
                Shell.Current.GoToAsync("///LoginPage");
            }
        }
    }
}