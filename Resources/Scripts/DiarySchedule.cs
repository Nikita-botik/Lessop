using System.Text.Json;

namespace Lessop;

internal sealed record ScheduledLesson(string? Name, TimeSpan? Start, TimeSpan? End)
{
    public bool IsBlank => string.IsNullOrEmpty(Name) && Start is null && End is null;
    public bool HasValidTime => Start is { } start && End is { } end &&
        start >= TimeSpan.Zero && start < end && end < TimeSpan.FromDays(1);
}

internal static class DiarySchedule
{
    private static readonly string PathName = Path.Combine(FileSystem.AppDataDirectory, "Schedule.json");
    private static readonly object Sync = new();
    private static readonly DayOfWeek[] Days = Enum.GetValues<DayOfWeek>();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IgnoreReadOnlyProperties = true
    };

    public static IReadOnlyList<ScheduledLesson> LoadDay(DayOfWeek day)
    {
        lock (Sync)
            return GetRows(Load(), day);
    }

    public static void SaveDay(DayOfWeek day, List<ScheduledLesson> rows, bool applyTimesToAllDays)
    {
        lock (Sync)
        {
            var schedule = Load();
            schedule[day] = rows;
            if (applyTimesToAllDays)
            {
                foreach (var otherDay in Days)
                {
                    if (otherDay == day)
                        continue;

                    var existing = GetRows(schedule, otherDay);
                    var updated = new List<ScheduledLesson>(Math.Max(existing.Count, rows.Count));
                    for (int i = 0; i < rows.Count; i++)
                    {
                        string? name = i < existing.Count ? existing[i].Name : null;
                        updated.Add(new ScheduledLesson(name, rows[i].Start, rows[i].End));
                    }

                    // Keep lesson assignments that have no corresponding time in the source day.
                    int lastNamed = existing.FindLastIndex(lesson => !string.IsNullOrEmpty(lesson.Name));
                    for (int i = rows.Count; i <= lastNamed; i++)
                        updated.Add(existing[i] with { Start = null, End = null });

                    schedule[otherDay] = updated;
                }
            }
            LocalDataFile.Write(PathName, JsonSerializer.Serialize(schedule, JsonOptions));
        }
    }

    private static List<ScheduledLesson> GetRows(
        Dictionary<DayOfWeek, List<ScheduledLesson>> schedule, DayOfWeek day) =>
        schedule.GetValueOrDefault(day) ?? new List<ScheduledLesson>();

    private static Dictionary<DayOfWeek, List<ScheduledLesson>> Load()
    {
        if (File.Exists(PathName))
        {
            return JsonSerializer.Deserialize<Dictionary<DayOfWeek, List<ScheduledLesson>>>(
                File.ReadAllText(PathName)) ?? throw new JsonException("The schedule is empty.");
        }

        var settings = SaveSettings.Load();
        var names = JSE.LoadFromFile();
        var times = SaveLessonTime.LoadFromFile();
        var schedule = new Dictionary<DayOfWeek, List<ScheduledLesson>>();
        foreach (var day in Days)
        {
            string key = day.ToString();
            var dayNames = (List<string>?)typeof(Lesson).GetProperty(key)?.GetValue(names) ?? new();
            var dayTimes = (List<List<TimeSpan>>?)typeof(LessonTime).GetProperty(key)?.GetValue(times);
            var starts = dayTimes?.ElementAtOrDefault(0) ?? new List<TimeSpan>();
            var ends = dayTimes?.ElementAtOrDefault(1) ?? new List<TimeSpan>();
            int count = (int)(typeof(SaveSettingse).GetProperty("LessonsCount" + key)?.GetValue(settings) ?? 0);
            count = Math.Max(count, Math.Max(dayNames.Count, Math.Max(starts.Count, ends.Count)));

            var rows = new List<ScheduledLesson>();
            for (int i = 0; i < count; i++)
            {
                string? name = dayNames.ElementAtOrDefault(i);
                TimeSpan? start = i < starts.Count ? starts[i] : null;
                TimeSpan? end = i < ends.Count ? ends[i] : null;
                if (string.IsNullOrEmpty(name) && start == TimeSpan.Zero && end == TimeSpan.Zero)
                    (start, end) = (null, null);

                rows.Add(new ScheduledLesson(name, start, end));
            }
            schedule.Add(day, rows);
        }
        return schedule;
    }
}
