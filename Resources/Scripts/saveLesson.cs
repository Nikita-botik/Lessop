using System.Text.Json;
using Lessop;

namespace AutoMeetings;

public static class saveLesson
{
    private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "Lessons.json");
    private static readonly object Sync = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static event Action? LessonsUpdated;

    public static void AddOrUpdateUser(string name, ConfLessonSave lesson)
    {
        lock (Sync)
        {
            var lessons = ReadFile();
            lessons[name] = lesson;
            WriteFile(lessons);
        }
        LessonsUpdated?.Invoke();
    }

    public static void Delete(string name)
    {
        lock (Sync)
        {
            var lessons = ReadFile();
            if (!lessons.Remove(name))
                return;

            WriteFile(lessons);
        }
        LessonsUpdated?.Invoke();
    }

    public static Dictionary<string, ConfLessonSave> LoadFromFile()
    {
        lock (Sync)
            return ReadFile();
    }

    private static Dictionary<string, ConfLessonSave> ReadFile()
    {
        if (!File.Exists(FilePath))
            return new();

        return JsonSerializer.Deserialize<Dictionary<string, ConfLessonSave>>(
            File.ReadAllText(FilePath)) ?? throw new JsonException("The lesson catalog is empty.");
    }

    private static void WriteFile(Dictionary<string, ConfLessonSave> lessons) =>
        LocalDataFile.Write(FilePath, JsonSerializer.Serialize(lessons, JsonOptions));
}

public class ConfLessonSave
{
    public string URL { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string NameOfConf { get; set; } = string.Empty;
}
