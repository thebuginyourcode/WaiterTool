using WaiterTool.Mobile.Models;
using WaiterTool.Mobile.Services;

namespace WaiterTool.Mobile;

public partial class WaitersPage : ContentPage
{
    private readonly WaiterState _state;

    public WaitersPage(WaiterState state)
    {
        InitializeComponent();

        _state = state;
        QueueCollectionView.ItemsSource = _state.Queue;
    }

    private void OnAddWaiterClicked(object? sender, EventArgs e)
    {
        var name = NewWaiterEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;

        _state.Queue.Add(new Waiter { Name = name });
        NewWaiterEntry.Text = string.Empty;
        _state.SaveQueue();
    }

    private void OnRemoveWaiterClicked(object? sender, EventArgs e)
    {
        if (QueueCollectionView.SelectedItem is not Waiter selected)
            return;

        _state.Queue.Remove(selected);
        _state.SaveQueue();
    }
}
