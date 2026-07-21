using System;
using System.Text.Json.Serialization;

namespace WaiterTool.Models;

public class HistoryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string WaiterName { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
    public bool Skipped { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? PhotoPath { get; set; }

    [JsonIgnore]
    public string TableLabel => Skipped ? "Skipped" : $"Table {TableNumber}";
}
