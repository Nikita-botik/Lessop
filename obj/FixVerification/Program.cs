using System.Reflection;
using AutoMeetings;
using AutoMeetingsPort;

static void Require(bool condition, string scenario)
{
    if (!condition)
        throw new InvalidOperationException(scenario);
    Console.WriteLine("PASS " + scenario);
}

var start = TimeSpan.FromHours(8);
var end = TimeSpan.FromHours(9);
Directory.CreateDirectory(FileSystem.AppDataDirectory);
JSE.SaveToFile(new Lesson { Monday = ["Math"], Tuesday = ["Science", "", "History"] });
SaveLessonTime.SaveToFile(new LessonTime { Monday = [[start, end], [end]] });
var monday = DiarySchedule.LoadDay(DayOfWeek.Monday);
Require(monday[0] == new ScheduledLesson("Math", start, end) && monday[1].End is null, "legacy names and unbalanced time lists");
DiarySchedule.SaveDay(DayOfWeek.Monday, [monday[0]], true);
var tuesday = DiarySchedule.LoadDay(DayOfWeek.Tuesday);
Require(tuesday[0] == new ScheduledLesson("Science", start, end) && tuesday[2].Name == "History", "copying times preserves other days' assignments and positions");
DiarySchedule.SaveDay(DayOfWeek.Monday, [], false);
Require(DiarySchedule.LoadDay(DayOfWeek.Monday).Count == 0 && DiarySchedule.LoadDay(DayOfWeek.Tuesday)[0].End == end,
    "day saves do not mutate other days");
Require(!new ScheduledLesson("Math", null, end).HasValidTime && !new ScheduledLesson("Math", end, end).HasValidTime,
    "missing and non-increasing times rejected");

var before = SaveSettings.GetAutomationSnapshot();
SaveSettings.Update(settings => settings.SelectedRegion = new OptionItem { Tag = "78" });
SaveSettings.Update(settings => settings.isStart = false);
Require(before.Version.IsCancellationRequested && SaveSettings.Load().SelectedRegion.Tag == "78" && !SaveSettings.Load().isStart,
    "settings edits preserve other fields and cancel pending checks");

int events = 0;
saveLesson.LessonsUpdated += () => events++;
saveLesson.AddOrUpdateUser("Math", new() { URL = "https://example.test/lesson" });
saveLesson.AddOrUpdateUser("History", new() { URL = "https://example.test/history" });
saveLesson.Delete("History");
saveLesson.Delete("Missing");
Require(events == 3 && !saveLesson.LoadFromFile().ContainsKey("History"), "lesson save and delete share a single catalog event");
string catalogFile = Path.Combine(FileSystem.AppDataDirectory, "Lessons.json");
string validCatalog = File.ReadAllText(catalogFile);
File.WriteAllText(catalogFile, "{corrupt");
try
{
    saveLesson.Delete("Math");
    throw new InvalidOperationException("Corrupt catalog was accepted");
}
catch (System.Text.Json.JsonException)
{
    Require(File.ReadAllText(catalogFile) == "{corrupt", "a corrupt catalog is not overwritten");
}
File.WriteAllText(catalogFile, validCatalog);

var now = DateTime.Now;
DiarySchedule.SaveDay(now.DayOfWeek, [new ScheduledLesson("Math", TimeSpan.Zero, TimeSpan.FromDays(1).Add(TimeSpan.FromTicks(-1)))], false);
SaveSettings.Update(settings => { settings.isStart = true; settings.isRaidButton = true; });

var checkMethod = typeof(Programs).GetMethod("CheckLessonAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
async Task Check(Programs programs)
{
    var (settings, token) = SaveSettings.GetAutomationSnapshot();
    await (Task)checkMethod.Invoke(programs, [settings, token, token, CancellationToken.None])!;
}

var runner = new Programs();
ReadFromURL.Alarm = true;
await Check(runner);
Require(App.Notifications.Description == "Alert" && Launcher.Opened == 0, "active alert asks before opening");
App.Notifications.Question!.SetResult(false);
await Check(runner);
Require(Launcher.Opened == 0, "declining an active alert blocks the lesson");
ReadFromURL.Alarm = false;
await Check(runner);
Require(App.Notifications.Description == "All clear", "all-clear asks again after declining an active alert");
App.Notifications.Question!.SetResult(true);
await Check(runner);
await Check(runner);
Require(Launcher.Opened == 1, "latest answer opens the lesson only once");

ReadFromURL.Alarm = true;
var stopped = new Programs();
await Check(stopped);
var lateResponse = App.Notifications.Question!;
SaveSettings.Update(settings => settings.isStart = false);
lateResponse.TrySetResult(true);
await Check(stopped);
Require(Launcher.Opened == 1, "stopping during a prompt blocks a late approval");

SaveSettings.Update(settings => settings.isStart = true);
ReadFromURL.Unavailable = true;
var offline = new Programs();
await Check(offline);
await Check(offline);
Require(Launcher.Opened == 1 && App.Notifications.Errors == 1, "unavailable status blocks opening and reports one error");
