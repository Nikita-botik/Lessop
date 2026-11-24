
using System.Text.Json;

namespace Lessop
{

    public static class JSE
    {
        static string filepath = Path.Combine(FileSystem.AppDataDirectory, "Diary.json");
        // Метод для збереження словника у файл
        public static void SaveToFile(Lesson data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filepath, json);
        }

        // Метод для завантаження словника з файлу
        public static  Lesson LoadFromFile()
        {
            if (!File.Exists(filepath))
                return new Lesson(); // Якщо файлу немає, повертаємо пустий словник

            string json = File.ReadAllText(filepath);
            return JsonSerializer.Deserialize<Lesson>(json) ?? new Lesson();
        }
    }

    public class Lesson
    {
        public List<string> Monday { get; set; } = new();
        public List<string> Tuesday { get; set; } = new();
        public List<string> Wednesday { get; set; } = new();
        public List<string> Thursday { get; set; } = new();
        public List<string> Friday { get; set; } = new();
        public List<string> Saturday { get; set; } = new();
        public List<string> Sunday { get; set; } = new();
    }
}