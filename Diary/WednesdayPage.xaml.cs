namespace Lessop;

public partial class WednesdayPage : DiaryDayPage
{
    public WednesdayPage()
    {
        InitializeComponent();
        Configure(DayOfWeek.Wednesday, HOChange, HOChange2, PickerStack);
    }
}
