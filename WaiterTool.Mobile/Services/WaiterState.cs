using System.Collections.ObjectModel;
using System.Linq;
using WaiterTool.Mobile.Models;

namespace WaiterTool.Mobile.Services;

/// <summary>
/// Shared waiter queue/history state so the Waiters, Current Turn, and History
/// tabs all see the same live data instead of each owning its own copy.
/// </summary>
public sealed class WaiterState
{
    private readonly PersistenceService _persistence;

    public ObservableCollection<Waiter> Queue { get; } = new();
    public ObservableCollection<HistoryEntry> History { get; } = new();

    public WaiterState(PersistenceService persistence)
    {
        _persistence = persistence;

        foreach (var waiter in _persistence.LoadQueue())
            Queue.Add(waiter);

        foreach (var entry in _persistence.LoadHistory().OrderByDescending(h => h.Timestamp))
            History.Add(entry);
    }

    public void SaveQueue() => _persistence.SaveQueue(Queue);

    public void SaveHistory() => _persistence.SaveHistory(History);
}
