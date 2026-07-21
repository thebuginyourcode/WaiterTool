using System;

namespace WaiterTool.Models;

public class Waiter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}
