using System.Diagnostics;
using Lessop.Resources.Strings;
using Newtonsoft.Json;

namespace Lessop;

public partial class MainPage : ContentPage
{
    private bool initializing = true;

    public MainPage()
    {
        InitializeComponent();
        RefreshSettings();
    }

    private void RefreshSettings()
    {
        initializing = true;
        var settings = SaveSettings.Load();
        BindingContext = new MainViewModel(settings.SelectedRegion.Tag);
        isRaidButton.IsChecked = settings.isRaidButton;
        OAOM.IsChecked = settings.OAO_Mode;
        RegionSelect.IsVisible = settings.isRaidButton;
        UpdateButtons(settings);
        initializing = false;
    }

    private async void OnSelectedRegionChanged(object sender, EventArgs e)
    {
        if (!initializing && RegionSelect.SelectedItem is OptionItem selected)
            await UpdateSettingsAsync(settings => settings.SelectedRegion = selected);
    }

    private async void IsAlarmOrNotChanged(object sender, EventArgs e)
    {
        if (initializing)
            return;

        bool enabled = isRaidButton.IsChecked;
        await UpdateSettingsAsync(settings => settings.isRaidButton = enabled);
    }

    private async void OAOMChanged(object sender, EventArgs e)
    {
        if (initializing)
            return;

        bool manual = OAOM.IsChecked;
        await UpdateSettingsAsync(settings =>
        {
            settings.OAO_Mode = manual;
            if (!manual)
                settings.isStart = true;
        });
    }

    private async void StartButton_Click(object sender, EventArgs e) =>
        await UpdateSettingsAsync(settings => settings.isStart = true);

    private async void StopButton_Click(object sender, EventArgs e) =>
        await UpdateSettingsAsync(settings => settings.isStart = false);

    private async Task UpdateSettingsAsync(Action<SaveSettingse> update)
    {
        try
        {
            var settings = SaveSettings.Update(update);
            RegionSelect.IsVisible = settings.isRaidButton;
            UpdateButtons(settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Debug.WriteLine(ex);
            RefreshSettings();
            await DisplayAlertAsync(AppResources.ErrorTitle, AppResources.SaveDataError, "OK");
        }
    }

    private void UpdateButtons(SaveSettingse settings)
    {
        StartButton.IsEnabled = settings.OAO_Mode && !settings.isStart;
        StopButton.IsEnabled = settings.OAO_Mode && settings.isStart;
        StartButton.BackgroundColor = Color.FromArgb(StartButton.IsEnabled ? "#9880e5" : "#FF878787");
        StopButton.BackgroundColor = Color.FromArgb(StopButton.IsEnabled ? "#9880e5" : "#FF878787");
    }
}

public class OptionItem
{
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
}
