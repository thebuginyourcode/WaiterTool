using WaiterTool.Mobile.Models;
using WaiterTool.Mobile.Services;

namespace WaiterTool.Mobile;

public partial class CurrentTurnPage : ContentPage
{
    private readonly WaiterState _state;
    private readonly CameraService _camera;
    private readonly PersistenceService _persistence;

    public CurrentTurnPage(WaiterState state, CameraService camera, PersistenceService persistence)
    {
        InitializeComponent();

        _state = state;
        _camera = camera;
        _persistence = persistence;

        _state.Queue.CollectionChanged += (_, _) => UpdateCurrentWaiterDisplay();
        UpdateCurrentWaiterDisplay();
    }

    private void UpdateCurrentWaiterDisplay()
    {
        CurrentWaiterLabel.Text = _state.Queue.Count > 0 ? _state.Queue[0].Name : "—";
        NextWaiterButton.IsEnabled = _state.Queue.Count > 0;
        SkipButton.IsEnabled = _state.Queue.Count > 0;
    }

    private async void OnNextWaiterClicked(object? sender, EventArgs e)
    {
        if (_state.Queue.Count == 0)
            return;

        var current = _state.Queue[0];
        _state.Queue.RemoveAt(0);

        var tablePage = new TableNumberPage(current.Name, _camera, _persistence);
        await Navigation.PushModalAsync(tablePage);
        var result = await tablePage.Completion.Task;

        if (result is not null)
        {
            // Cycle: the waiter returns to the back of the list for their next turn.
            _state.Queue.Add(current);

            _state.History.Insert(0, result);
            _state.SaveHistory();

            ShowLastCapture(result);
        }
        else
        {
            // Cancelled: put the waiter back at the front so their turn isn't lost.
            _state.Queue.Insert(0, current);
        }

        _state.SaveQueue();
    }

    private async void OnSkipClicked(object? sender, EventArgs e)
    {
        if (_state.Queue.Count == 0)
            return;

        var current = _state.Queue[0];
        _state.Queue.RemoveAt(0);

        string? photoPath = null;
        var tempPath = await _camera.CapturePhotoAsync();
        if (tempPath is not null)
        {
            photoPath = _persistence.SavePhoto(tempPath, Guid.NewGuid());
        }

        // Cycle: the waiter returns to the back of the list for their next turn.
        _state.Queue.Add(current);

        var entry = new HistoryEntry
        {
            WaiterName = current.Name,
            TableNumber = string.Empty,
            Skipped = true,
            Timestamp = DateTime.Now,
            PhotoPath = photoPath
        };

        _state.History.Insert(0, entry);
        _state.SaveHistory();
        ShowLastCapture(entry);

        _state.SaveQueue();
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
