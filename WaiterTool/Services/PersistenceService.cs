using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenCvSharp;
using WaiterTool.Models;

namespace WaiterTool.Services;

/// <summary>
/// Reads/writes the waiter queue and history log to disk under
/// %AppData%\WaiterTool, and saves captured photos alongside them.
/// </summary>
public sealed class PersistenceService
{
    private readonly string _photosDir;
    private readonly string _queueFile;
    private readonly string _historyFile;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public PersistenceService()
    {
        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WaiterTool");
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

    public string SavePhoto(Mat frame, Guid entryId)
    {
        var fullPath = Path.Combine(_photosDir, $"{entryId}.jpg");
        Cv2.ImWrite(fullPath, frame);
        return fullPath;
    }
}
