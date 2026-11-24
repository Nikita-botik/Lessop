using System.Diagnostics;
using System.Text.Json;
using AutoMeetings;
using Lessop.Resources.Strings;

namespace Lessop;

public partial class AddLessonPage : ContentPage
{
    public AddLessonPage()
    {
        InitializeComponent();
    }

    private async void Save(object sender, EventArgs e)
    {
        string? name = EnterTheName.Text?.Trim();
        string? url = EnterTheURL.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync(AppResources.ErrorTitle, AppResources.ErrorName, "OK");
            return;
        }
        if (string.IsNullOrEmpty(url))
        {
            await DisplayAlertAsync(AppResources.ErrorTitle, AppResources.ErrorUrl, "OK");
            return;
        }
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            await DisplayAlertAsync(AppResources.ErrorTitle, AppResources.ErrorNotUrl, "OK");
            return;
        }

        try
        {
            saveLesson.AddOrUpdateUser(name, new ConfLessonSave
            {
                URL = url,
                NameOfConf = name,
                Description = EnterTheDescription.Text ?? string.Empty
            });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Debug.WriteLine(ex);
            await DisplayAlertAsync(AppResources.ErrorTitle, AppResources.SaveDataError, "OK");
            return;
        }

        EnterTheURL.Text = string.Empty;
        EnterTheDescription.Text = string.Empty;
        EnterTheName.Text = string.Empty;
        await DisplayAlertAsync(AppResources.SaveButton, AppResources.SaveTheLesson, "OK");
    }
}
