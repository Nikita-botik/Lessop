using System.Diagnostics;
using System.Text.Json;
using AutoMeetings;
using 
    .Resources.Strings;

namespace Lessop;

public class DiaryDayPage : ContentPage
{
    private readonly List<(TimePicker Start, TimePicker End, Picker Lesson)> rows = new();
    private DayOfWeek day;
    private Layout starts = null!;
    private Layout ends = null!;
    private Layout lessons = null!;
    private string[] lessonNames = [];
    private bool loaded;

    protected void Configure(DayOfWeek day, Layout starts, Layout ends, Layout lessons)
    {
        this.day = day;
        this.starts = starts;
        this.ends = ends;
        this.lessons = lessons;
        if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
        {
            starts.HorizontalOptions = LayoutOptions.End;
            ends.HorizontalOptions = LayoutOptions.Start;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        saveLesson.LessonsUpdated += UpdatePickers;
        try
        {
            lessonNames = saveLesson.LoadFromFile().Keys.ToArray();
            if (loaded)
            {
                RefreshPickers();
                return;
            }

            foreach (var entry in DiarySchedule.LoadDay(day))
                AddRow(entry);

            loaded = true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Debug.WriteLine(ex);
            await DisplayAlertAsync(AppResources.ErrorTitle, AppResources.LoadDataError, "OK");
        }
    }

    protected override void OnDisappearing()
    {
        saveLesson.LessonsUpdated -= UpdatePickers;
        base.OnDisappearing();
    }

    protected void OnAddTimePickerClicked(object sender, EventArgs e)
    {
        if (loaded)
            AddRow(new ScheduledLesson(null, null, null));
    }

    protected void OnRemoveTimePickerClicked(object sender, EventArgs e)
    {
        if (rows.Count == 0)
            return;

        var row = rows[^1];
        starts.Children.Remove(row.Start);
        ends.Children.Remove(row.End);
        lessons.Children.Remove(row.Lesson);
        rows.RemoveAt(rows.Count - 1);
    }

    protected async void SaveClicked(object sender, EventArgs e) => await SaveAsync(false);

    protected async void OnApplyToAllTimeClicked(object sender, EventArgs e) => await SaveAsync(true);

    private void AddRow(ScheduledLesson entry)
    {
        int number = rows.Count + 1;
        var start = CreateTimePicker(entry.Start, $"{day}_{number}");
        var end = CreateTimePicker(entry.End, $"{day}End_{number}");
        var lesson = new Picker
        {
            Margin = new Thickness(0, 10, 0, 0),
            AutomationId = $"{day}{number}Picker",
            ItemsSource = lessonNames,
            SelectedItem = entry.Name is not null && lessonNames.Contains(entry.Name) ? entry.Name : null
        };

        rows.Add((start, end, lesson));
        starts.Children.Add(start);
        ends.Children.Add(end);
        lessons.Children.Add(lesson);
    }

    private static TimePicker CreateTimePicker(TimeSpan? time, string automationId) =>
        new()
        {
            AutomationId = automationId,
            Format = "HH:mm",
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 10, 0, 0),
            Time = time,
            WidthRequest = DeviceInfo.Idiom == DeviceIdiom.Desktop ? 300 : -1
        };

    private void UpdatePickers() =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                lessonNames = saveLesson.LoadFromFile().Keys.ToArray();
                RefreshPickers();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                Debug.WriteLine(ex);
            }
        });

    private void RefreshPickers()
    {
        foreach (var row in rows)
        {
            string? selected = row.Lesson.SelectedItem as string;
            row.Lesson.ItemsSource = lessonNames;
            row.Lesson.SelectedItem = selected is not null && lessonNames.Contains(selected) ? selected : null;
        }
    }

    private async Task SaveAsync(bool applyTimesToAllDays)
    {
        if (!loaded)
            return;

        var schedule = rows.Select(row => new ScheduledLesson(
            row.Lesson.SelectedItem as string, row.Start.Time, row.End.Time)).ToList();
        if (schedule.Any(lesson => !lesson.IsBlank && !lesson.HasValidTime))
        {
            await DisplayAlertAsync(AppResources.ErrorTitle, AppResources.ErrorTime, "OK");
            return;
        }

        try
        {
            DiarySchedule.SaveDay(day, schedule, applyTimesToAllDays);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Debug.WriteLine(ex);
            await DisplayAlertAsync(AppResources.ErrorTitle, AppResources.SaveDataError, "OK");
        }
    }
}
