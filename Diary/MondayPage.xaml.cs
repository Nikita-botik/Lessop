namespace Lessop;

public partial class MondayPage : DiaryDayPage
{
    public MondayPage()
    {
        InitializeComponent();
        Configure(DayOfWeek.Monday, HOChange, HOChange2, PickerStack);
    }
}
