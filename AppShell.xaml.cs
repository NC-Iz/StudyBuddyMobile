using StudyBuddyMobile.Views;

namespace StudyBuddyMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation

            // Study Resources routes
            Routing.RegisterRoute("AddResourcePage", typeof(AddResourcePage));
            Routing.RegisterRoute("EditResourcePage", typeof(EditResourcePage));

            // Study Groups routes
            Routing.RegisterRoute("CreateGroupPage", typeof(CreateGroupPage));
            Routing.RegisterRoute("MyGroupsPage", typeof(MyGroupsPage));
            Routing.RegisterRoute("GroupDetailsPage", typeof(GroupDetailsPage));
            Routing.RegisterRoute("EditGroupPage", typeof(EditGroupPage));
        }
    }
}