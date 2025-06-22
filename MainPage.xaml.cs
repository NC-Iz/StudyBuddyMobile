namespace StudyBuddyMobile
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnGreetClicked(object sender, EventArgs e)
        {
            string name = NameEntry.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Hello!", "Hello there, Study Buddy!", "OK");
            }
            else
            {
                await DisplayAlert("Hello!", $"Hello {name}! Welcome to Study Buddy Mobile!", "OK");
            }
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterLabel.Text = $"Clicked {count} time";
            else
                CounterLabel.Text = $"Clicked {count} times";

            // Change button text after multiple clicks
            if (count >= 5)
            {
                CounterBtn.Text = "You're getting the hang of it!";
            }
        }
    }
}