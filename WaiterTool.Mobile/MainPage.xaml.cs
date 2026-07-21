using System.Collections.ObjectModel;
using System.Linq;
using WaiterTool.Mobile.Models;
using WaiterTool.Mobile.Services;

namespace WaiterTool.Mobile;

public partial class MainPage : ContentPage
{
    private readonly PersistenceService _persistence = new();
    private readonly CameraService _camera = new();

    public ObservableCollection<Waiter> Queue { get; } = new();
    public ObservableCollection<HistoryEntry> History { get; } = new();

    public MainPage()
    {
        InitializeComponent();

        QueueCollectionView.ItemsSource = Queue;
        HistoryCollectionView.ItemsSource = History;

        foreach (var waiter in _persistence.LoadQueue())
            Queue.Add(waiter);

        foreach (var entry in _persistence.LoadHistory().OrderByDescending(h => h.Timestamp))
            History.Add(entry);

        UpdateCurrentWaiterDisplay();
    }

    private void OnAddWaiterClicked(object? sender, EventArgs e)
    {
        var name = NewWaiterEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;

        Queue.Add(new Waiter { Name = name });
        NewWaiterEntry.Text = string.Empty;

        _persistence.SaveQueue(Queue);
        UpdateCurrentWaiterDisplay();
    }

    private void OnRemoveWaiterClicked(object? sender, EventArgs e)
    {
        if (QueueCollectionView.SelectedItem is not Waiter selected)
            return;

        Queue.Remove(selected);
        _persistence.SaveQueue(Queue);
        UpdateCurrentWaiterDisplay();
    }

    private void UpdateCurrentWaiterDisplay()
    {
        CurrentWaiterLabel.Text = Queue.Count > 0 ? Queue[0].Name : "—";
        NextWaiterButton.IsEnabled = Queue.Count > 0;
    }

    private async void OnNextWaiterClicked(object? sender, EventArgs e)
    {
        if (Queue.Count == 0)
            return;

        var current = Queue[0];
        Queue.RemoveAt(0);

        var tablePage = new TableNumberPage(current.Name, _camera, _persistence);
        await Navigation.PushModalAsync(tablePage);
        var result = await tablePage.Completion.Task;

        if (result is not null)
        {
            // Cycle: the waiter returns to the back of the list for their next turn.
            Queue.Add(current);

            History.Insert(0, result);
            _persistence.SaveHistory(History);

            ShowLastCapture(result);
        }
        else
        {
            // Cancelled: put the waiter back at the front so their turn isn't lost.
            Queue.Insert(0, current);
        }

        _persistence.SaveQueue(Queue);
        UpdateCurrentWaiterDisplay();
    }

    private void ShowLastCapture(HistoryEntry entry)
    {
        NoCaptureLabel.IsVisible = false;
        LastCaptureInfoLabel.IsVisible = true;

        var hasPhoto = !string.IsNullOrWhiteSpace(entry.PhotoPath) && File.Exists(entry.PhotoPath);
        LastCaptureImageBorder.IsVisible = hasPhoto;

        if (hasPhoto)
        {
            LastCaptureImage.Source = ImageSource.FromFile(entry.PhotoPath!);
        }

        LastCaptureInfoLabel.Text = $"{entry.WaiterName} — {entry.TableLabel} — {entry.Timestamp:t}";
    }
}
