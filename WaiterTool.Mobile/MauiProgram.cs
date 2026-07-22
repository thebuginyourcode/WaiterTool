using CommunityToolkit.Maui;
using WaiterTool.Mobile.Services;

namespace WaiterTool.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitCamera();

        builder.Services.AddSingleton<PersistenceService>();
        builder.Services.AddSingleton<WaiterState>();
        builder.Services.AddSingleton<MidnightLogScheduler>();

        builder.Services.AddTransient<CurrentTurnPage>();
        builder.Services.AddTransient<WaitersPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}
