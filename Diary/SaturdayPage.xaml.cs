namespace Lessop;

public partial class SaturdayPage : DiaryDayPage
{
    public SaturdayPage()
    {
        InitializeComponent();
        Configure(DayOfWeek.Saturday, HOChange, HOChange2, PickerStack);
    }
}
