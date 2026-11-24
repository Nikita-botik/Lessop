namespace Lessop;

public partial class FridayPage : DiaryDayPage
{
    public FridayPage()
    {
        InitializeComponent();
        Configure(DayOfWeek.Friday, HOChange, HOChange2, PickerStack);
    }
}
