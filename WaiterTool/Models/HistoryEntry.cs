using System;

namespace WaiterTool.Models;

public class HistoryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string WaiterName { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? PhotoPath { get; set; }
}
