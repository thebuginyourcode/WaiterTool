using WaiterTool.Mobile.Models;
using WaiterTool.Mobile.Services;

namespace WaiterTool.Mobile;

public partial class TableNumberPage : ContentPage
{
    private readonly string _waiterName;
    private readonly CameraService _camera;
    private readonly PersistenceService _persistence;
    private readonly Guid _entryId = Guid.NewGuid();

    private string? _capturedPhotoPath;

    public TaskCompletionSource<HistoryEntry?> Completion { get; } = new();

    public TableNumberPage(string waiterName, CameraService camera, PersistenceService persistence)
    {
        InitializeComponent();

        _waiterName = waiterName;
        _camera = camera;
        _persistence = persistence;

        HeaderLabel.Text = $"{waiterName}'s turn";

        if (!_camera.IsSupported)
        {
            CapturePhotoButton.Text = "Camera unavailable";
            CapturePhotoButton.IsEnabled = false;
        }
    }

    private async void OnCapturePhotoClicked(object? sender, EventArgs e)
    {
        var tempPath = await _camera.CapturePhotoAsync();
        if (tempPath is null)
            return;

        _capturedPhotoPath = _persistence.SavePhoto(tempPath, _entryId);

        PhotoPreviewBorder.IsVisible = true;
        PhotoPreviewImage.Source = ImageSource.FromFile(_capturedPhotoPath);
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        var tableNumber = TableNumberEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(tableNumber))
        {
            await DisplayAlert("Table Number Required", "Please enter a table number.", "OK");
            return;
        }

        var entry = new HistoryEntry
        {
            WaiterName = _waiterName,
            TableNumber = tableNumber,
            Skipped = false,
            Timestamp = DateTime.Now,
            PhotoPath = _capturedPhotoPath
        };

        await Navigation.PopModalAsync();
        Completion.TrySetResult(entry);
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
        Completion.TrySetResult(null);
    }
}
