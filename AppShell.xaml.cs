using StudyBuddyMobile.Views;

namespace StudyBuddyMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute("AddResourcePage", typeof(AddResourcePage));
            Routing.RegisterRoute("EditResourcePage", typeof(EditResourcePage));
        }
    }
}