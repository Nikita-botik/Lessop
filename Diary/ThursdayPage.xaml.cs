namespace Lessop;

public partial class ThursdayPage : DiaryDayPage
{
    public ThursdayPage()
    {
        InitializeComponent();
        Configure(DayOfWeek.Thursday, HOChange, HOChange2, PickerStack);
    }
}
