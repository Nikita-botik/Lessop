using System.Diagnostics;
using System.Text.Json;
using AutoMeetings;
using Lessop.Resources.Strings;

namespace Lessop;

public partial class ViewLessonPage : ContentPage
{
    private Dictionary<string, ConfLessonSave> lessons = new();

    public ViewLessonPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ReloadLessons();
        saveLesson.LessonsUpdated += OnLessonsUpdated;
    }

    protected override void OnDisappearing()
    {
        saveLesson.LessonsUpdated -= OnLessonsUpdated;
        base.OnDisappearing();
    }

    private void OnLessonsUpdated() => MainThread.BeginInvokeOnMainThread(ReloadLessons);

    private void ReloadLessons()
    {
        string? selection = RegionSelect.SelectedItem as string;
        try
        {
            lessons = saveLesson.LoadFromFile();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Debug.WriteLine(ex);
            lessons = new();
            _ = DisplayAlertAsync(AppResources.ErrorTitle, AppResources.LoadDataError, "OK");
        }

        RegionSelect.ItemsSource = lessons.Keys.ToArray();
        RegionSelect.SelectedItem = selection is not null && lessons.ContainsKey(selection) ? selection : null;
        UpdateDetails();
    }

    private void OnSelectedLessonChanged(object sender, EventArgs e) => UpdateDetails();

    private void UpdateDetails()
    {
        ConfLessonSave? lesson = RegionSelect.SelectedItem is string name ? lessons.GetValueOrDefault(name) : null;
        LessonURL.Text = lesson?.URL ?? string.Empty;
        DescriptionLesson.Text = lesson?.Description ?? string.Empty;
        Delete.IsVisible = lesson is not null;
    }

    private async void Delete_Clicked(object sender, EventArgs e)
    {
        if (RegionSelect.SelectedItem is not string name ||
            !await DisplayAlertAsync(AppResources.ConfirmationTitle, AppResources.ConfirmLessonDelete, AppResources.Yes, AppResources.No))
            return;

        try
        {
            saveLesson.Delete(name);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Debug.WriteLine(ex);
            await DisplayAlertAsync(AppResources.ErrorTitle, AppResources.SaveDataError, "OK");
        }
    }
}
