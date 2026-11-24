using System.Diagnostics;
using Lessop;
using Lessop.Resources.Strings;

namespace AutoMeetings;

internal sealed class Programs
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(20);
    private readonly ReadFromURL alarmReader = new();
    private readonly HashSet<LessonOccurrence> opened = new();
    private DateOnly openedDate;
    private CancellationToken settingsVersion;
    private bool? lastAlarm;
    private LessonOccurrence? currentLesson;
    private bool? permission;
    private CancellationTokenSource? questionCancellation;
    private Task<bool>? question;
    private bool reportedAlarmError;

    public async Task RunAsync(CancellationToken stopping)
    {
        try
        {
            while (!stopping.IsCancellationRequested)
            {
                CancellationToken waitVersion = default;
                try
                {
                    var (settings, version) = SaveSettings.GetAutomationSnapshot();
                    waitVersion = version;
                    using var check = CancellationTokenSource.CreateLinkedTokenSource(stopping, version);
                    await CheckLessonAsync(settings, version, check.Token, stopping);
                }
                catch (OperationCanceledException)
                {
                    // Settings changed or the window is closing.
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                if (question is { IsCompleted: false } pending)
                {
                    using var delayCancellation = CancellationTokenSource.CreateLinkedTokenSource(stopping, waitVersion);
                    await Task.WhenAny(pending, Task.Delay(PollInterval, delayCancellation.Token));
                    delayCancellation.Cancel();
                }
                else
                {
                    using var delayCancellation = CancellationTokenSource.CreateLinkedTokenSource(stopping, waitVersion);
                    try
                    {
                        await Task.Delay(PollInterval, delayCancellation.Token);
                    }
                    catch (OperationCanceledException) when (!stopping.IsCancellationRequested)
                    {
                        // Recheck immediately after the user changes settings.
                    }
                }
            }
        }
        finally
        {
            CancelQuestion();
        }
    }

    private async Task CheckLessonAsync(
        SaveSettingse settings, CancellationToken version, CancellationToken check, CancellationToken stopping)
    {
        if (version != settingsVersion)
        {
            CancelQuestion();
            settingsVersion = version;
            currentLesson = null;
            lastAlarm = null;
            permission = null;
            reportedAlarmError = false;
        }

        if (!settings.isStart)
            return;

        bool alarm = false;
        if (settings.isRaidButton)
        {
            try
            {
                if (!int.TryParse(settings.SelectedRegion.Tag, out int region))
                    throw new InvalidDataException("Invalid alert region.");

                alarm = await alarmReader.ReadAsync(region, check);
                reportedAlarmError = false;
            }
            catch (Exception ex) when (!check.IsCancellationRequested)
            {
                Debug.WriteLine(ex);
#if WINDOWS
                if (!reportedAlarmError)
                {
                    reportedAlarmError = true;
                    App.Notifications.ShowWindowsToast(AppResources.ErrorTitle, AppResources.AlarmStatusError);
                }
#endif
                return;
            }
        }

        check.ThrowIfCancellationRequested();
        DateTime now = DateTime.Now;
        var occurrence = FindCurrentLesson(now);
        bool alarmEnded = lastAlarm == true && !alarm;
        bool alarmChanged = lastAlarm != alarm;
        lastAlarm = alarm;

        if (occurrence != currentLesson || alarmChanged)
        {
            CancelQuestion();
            currentLesson = occurrence;
            permission = !alarm && !alarmEnded ? true : null;
        }

        if (occurrence is not { } lesson)
            return;

        if (openedDate != lesson.Date)
        {
            opened.Clear();
            openedDate = lesson.Date;
        }

        if (opened.Contains(lesson))
            return;

        if (permission is null)
        {
            if (question is null)
            {
                questionCancellation = CancellationTokenSource.CreateLinkedTokenSource(stopping, version);
                question = AskToJoinAsync(alarm, questionCancellation.Token);
                return;
            }

            if (!question.IsCompleted)
                return;

            var answer = question;
            try
            {
                permission = await answer;
            }
            finally
            {
                CancelQuestion();
            }
        }

        if (permission == true && await OpenLessonAsync(lesson, check))
            opened.Add(lesson);
    }

    private static LessonOccurrence? FindCurrentLesson(DateTime now)
    {
        var rows = DiarySchedule.LoadDay(now.DayOfWeek);
        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row is { HasValidTime: true, Start: { } start, End: { } end })
            {
                if (start <= now.TimeOfDay && now.TimeOfDay < end && !string.IsNullOrEmpty(row.Name))
                    return new LessonOccurrence(DateOnly.FromDateTime(now), i, start, end, row.Name);
            }
        }
        return null;
    }

    private static async Task<bool> OpenLessonAsync(LessonOccurrence lesson, CancellationToken check)
    {
        if (!saveLesson.LoadFromFile().TryGetValue(lesson.Name, out var saved) || saved is null)
            return false;

        try
        {
            if (!Uri.TryCreate(saved.URL, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
                throw new InvalidDataException("Invalid lesson URL.");

            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                check.ThrowIfCancellationRequested();
                var now = DateTime.Now;
                if (FindCurrentLesson(now) != lesson || !SaveSettings.Load().isStart)
                    return false;

                if (!await Launcher.OpenAsync(uri))
                    throw new InvalidOperationException("No application opened the lesson URL.");

                return true;
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Debug.WriteLine(ex);
#if WINDOWS
            App.Notifications.ShowWindowsToast(AppResources.ErrorTitle, AppResources.TheUrlOpenError);
#endif
            return false;
        }
    }

    private static Task<bool> AskToJoinAsync(bool alarm, CancellationToken cancellation)
    {
#if WINDOWS
        return App.Notifications.ShowWindowsToastQuestion(
            AppResources.ConfirmationTitle, alarm ? AppResources.ConfTextOnAlert : AppResources.ConfTextOnUnAlert,
            AppResources.Yes, AppResources.No, cancellation);
#else
        return Task.FromResult(false);
#endif
    }

    private void CancelQuestion()
    {
        questionCancellation?.Cancel();
        questionCancellation?.Dispose();
        questionCancellation = null;
        question = null;
    }

    private readonly record struct LessonOccurrence(DateOnly Date, int Index, TimeSpan Start, TimeSpan End, string Name);
}
