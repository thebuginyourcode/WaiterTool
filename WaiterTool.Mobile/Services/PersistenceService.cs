using System.Text.Json;
using Microsoft.Maui.Storage;
using WaiterTool.Mobile.Models;

namespace WaiterTool.Mobile.Services;

/// <summary>
/// Reads/writes the waiter queue and history log to the app's sandboxed data
/// directory, and saves captured photos alongside them.
/// </summary>
public sealed class PersistenceService
{
    private readonly string _photosDir;
    private readonly string _queueFile;
    private readonly string _historyFile;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public PersistenceService()
    {
        var appDataDir = FileSystem.AppDataDirectory;
        _photosDir = Path.Combine(appDataDir, "Photos");
        _queueFile = Path.Combine(appDataDir, "queue.json");
        _historyFile = Path.Combine(appDataDir, "history.json");

        Directory.CreateDirectory(_photosDir);
    }

    public List<Waiter> LoadQueue()
    {
        if (!File.Exists(_queueFile))
            return new List<Waiter>();

        try
        {
            var json = File.ReadAllText(_queueFile);
            return JsonSerializer.Deserialize<List<Waiter>>(json) ?? new List<Waiter>();
        }
        catch
        {
            return new List<Waiter>();
        }
    }

    public void SaveQueue(IEnumerable<Waiter> queue)
    {
        var json = JsonSerializer.Serialize(queue, _jsonOptions);
        File.WriteAllText(_queueFile, json);
    }

    public List<HistoryEntry> LoadHistory()
    {
        if (!File.Exists(_historyFile))
            return new List<HistoryEntry>();

        try
        {
            var json = File.ReadAllText(_historyFile);
            return JsonSerializer.Deserialize<List<HistoryEntry>>(json) ?? new List<HistoryEntry>();
        }
        catch
        {
            return new List<HistoryEntry>();
        }
    }

    public void SaveHistory(IEnumerable<HistoryEntry> history)
    {
        var json = JsonSerializer.Serialize(history, _jsonOptions);
        File.WriteAllText(_historyFile, json);
    }

    public string SavePhoto(string sourceFilePath, Guid entryId)
    {
        var destPath = Path.Combine(_photosDir, $"{entryId}.jpg");
        File.Copy(sourceFilePath, destPath, overwrite: true);
        return destPath;
    }
}
