using WaiterTool.Mobile.Services;

namespace WaiterTool.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddSingleton<PersistenceService>();
        builder.Services.AddSingleton<CameraService>();
        builder.Services.AddSingleton<WaiterState>();

        builder.Services.AddTransient<CurrentTurnPage>();
        builder.Services.AddTransient<WaitersPage>();
        builder.Services.AddTransient<HistoryPage>();

        return builder.Build();
    }
}
