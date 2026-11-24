namespace Lessop;

public partial class SundayPage : DiaryDayPage
{
    public SundayPage()
    {
        InitializeComponent();
        Configure(DayOfWeek.Sunday, HOChange, HOChange2, PickerStack);
    }
}
