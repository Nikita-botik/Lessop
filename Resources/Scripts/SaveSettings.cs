using System.Diagnostics;
using Lessop.Resources.Strings;
using Newtonsoft.Json;

namespace Lessop
{
    public static class SaveSettings
    {
        private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "settings.json");
        private static readonly object Sync = new();
        private static CancellationTokenSource version = new();

        public static SaveSettingse Load()
        {
            lock (Sync)
            {
                try
                {
                    return ReadFile();
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
                {
                    Debug.WriteLine(ex);
                    return new SaveSettingse { isStart = false };
                }
            }
        }

        internal static (SaveSettingse Settings, CancellationToken Version) GetAutomationSnapshot()
        {
            lock (Sync)
                return (ReadFile(), version.Token);
        }

        public static SaveSettingse Update(Action<SaveSettingse> update)
        {
            CancellationTokenSource previous;
            SaveSettingse settings;
            lock (Sync)
            {
                settings = ReadFile();
                update(settings);
                LocalDataFile.Write(FilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
                previous = version;
                version = new CancellationTokenSource();
            }

            previous.Cancel();
            return settings;
        }

        private static SaveSettingse ReadFile()
        {
            if (!File.Exists(FilePath))
                return new SaveSettingse();

            var settings = JsonConvert.DeserializeObject<SaveSettingse>(File.ReadAllText(FilePath))
                ?? throw new JsonSerializationException("The settings file is empty.");
            settings.SelectedRegion ??= new SaveSettingse().SelectedRegion;
            return settings;
        }
    }

    public class SaveSettingse
    {
        public bool ans { get; set; } = false;
        public bool OAO_Mode { get; set; } = false;
        public bool isRaidButton { get; set; } = false;
        public bool isStart { get; set; } = true;
        public OptionItem SelectedRegion { get; set; } = new OptionItem { Name = AppResources.Kyiv, Tag = "34" };
        public int LessonsCountMonday { get; set; } = 7;
        public int LessonsCountTuesday { get; set; } = 7;
        public int LessonsCountWednesday { get; set; } = 7;
        public int LessonsCountThursday { get; set; } = 7;
        public int LessonsCountFriday { get; set; } = 7;
        public int LessonsCountSaturday { get; set; } = 7;
        public int LessonsCountSunday { get; set; } = 7;
        public bool isAnswer { get; set; } = false;
        public bool unans { get; set; } = false;
    }
}
