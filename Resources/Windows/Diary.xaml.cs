using AutoMeetings;
using Lessop.Resources.Strings;
using Syncfusion.Maui.Picker;
namespace Lessop
{
    public partial class Diary : ContentPage
    {
        public Diary()
        {
            InitializeComponent();
        }

        private async void MondayClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MondayPage());
        }
        private async void TuesdayClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TuesdayPage());
        }
        private async void WednesdayClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WednesdayPage());
        }
        private async void ThursdayClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ThursdayPage());
        }
        private async void FridayClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FridayPage());
        }
        private async void SaturdayClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SaturdayPage());
        }
        private async void SundayClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SundayPage());
        }
    }
}