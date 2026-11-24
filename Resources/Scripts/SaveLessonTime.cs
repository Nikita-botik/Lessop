using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Lessop
{

    public static class SaveLessonTime
    {
        static string filepath = Path.Combine(FileSystem.AppDataDirectory, "DiaryTime.json");
        // Метод для збереження словника у файл
        public static void SaveToFile(LessonTime data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filepath, json);
        }

        // Метод для завантаження словника з файлу
        public static LessonTime LoadFromFile()
        {
            if (!File.Exists(filepath))
                return new LessonTime(); // Якщо файлу немає, повертаємо пустий словник

            string json = File.ReadAllText(filepath);
            return JsonSerializer.Deserialize<LessonTime>(json) ?? new LessonTime();
        }
    }

    public class LessonTime
    {
        public List<List<TimeSpan>> Monday { get; set; } = new() { new List<TimeSpan> { }, new List<TimeSpan> { } };
        public List<List<TimeSpan>> Tuesday { get; set; } = new() { new List<TimeSpan> { }, new List<TimeSpan> { } };
        public List<List<TimeSpan>> Wednesday { get; set; } = new() { new List<TimeSpan> { }, new List<TimeSpan> { } };
        public List<List<TimeSpan>> Thursday { get; set; } = new() { new List<TimeSpan> { }, new List<TimeSpan> { } };
        public List<List<TimeSpan>> Friday { get; set; } = new() { new List<TimeSpan> { }, new List<TimeSpan> { } };
        public List<List<TimeSpan>> Saturday { get; set; } = new() { new List<TimeSpan> { }, new List<TimeSpan> { } };
        public List<List<TimeSpan>> Sunday { get; set; } = new() { new List<TimeSpan> { }, new List<TimeSpan> { } };
    }
}