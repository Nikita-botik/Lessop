namespace Lessop;

public partial class TuesdayPage : DiaryDayPage
{
    public TuesdayPage()
    {
        InitializeComponent();
        Configure(DayOfWeek.Tuesday, HOChange, HOChange2, PickerStack);
    }
}
