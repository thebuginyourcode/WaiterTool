using WaiterTool.Mobile.Services;

namespace WaiterTool.Mobile;

public partial class App : Application
{
    public App(MidnightLogScheduler midnightLogScheduler)
    {
        InitializeComponent();
        midnightLogScheduler.Start();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
