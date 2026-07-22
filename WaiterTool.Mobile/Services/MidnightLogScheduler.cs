using Microsoft.Maui.Storage;

namespace WaiterTool.Mobile.Services;

/// <summary>
/// Exports the day's History to a text log and clears it at local midnight.
/// Also catches up immediately on launch if the app wasn't running when
/// midnight passed (e.g. opened the next day).
/// </summary>
public sealed class MidnightLogScheduler
{
    private const string LastResetDateKey = "LastLogResetDate";

    private readonly WaiterState _state;
    private readonly PersistenceService _persistence;
    private IDispatcherTimer? _timer;

    public MidnightLogScheduler(WaiterState state, PersistenceService persistence)
    {
        _state = state;
        _persistence = persistence;
    }

    public void Start()
    {
        CatchUpIfNeeded();
        ScheduleNextMidnight();
    }

    private void CatchUpIfNeeded()
    {
        var lastReset = Preferences.Default.Get(LastResetDateKey, string.Empty);
        var today = DateTime.Now.ToString("yyyy-MM-dd");

        if (lastReset == today)
            return;

        if (_state.History.Count > 0)
        {
            _persistence.ExportHistoryToTextLog(_state.History);
            _state.History.Clear();
            _state.SaveHistory();
        }

        Preferences.Default.Set(LastResetDateKey, today);
    }

    private void ScheduleNextMidnight()
    {
        var now = DateTime.Now;
        var delay = now.Date.AddDays(1) - now;

        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer is null)
            return;

        _timer.Interval = delay;
        _timer.IsRepeating = false;
        _timer.Tick += (_, _) =>
        {
            CatchUpIfNeeded();
            ScheduleNextMidnight();
        };
        _timer.Start();
    }
}
