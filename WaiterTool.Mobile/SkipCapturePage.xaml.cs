using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core;
using WaiterTool.Mobile.Models;
using WaiterTool.Mobile.Services;

namespace WaiterTool.Mobile;

public partial class SkipCapturePage : ContentPage
{
    private readonly string _waiterName;
    private readonly PersistenceService _persistence;
    private readonly Guid _entryId = Guid.NewGuid();

    private string? _capturedPhotoPath;
    private bool _isShowingCapturedPhoto;

    public TaskCompletionSource<HistoryEntry?> Completion { get; } = new();

    public SkipCapturePage(string waiterName, PersistenceService persistence)
    {
        InitializeComponent();

        _waiterName = waiterName;
        _persistence = persistence;

        HeaderLabel.Text = $"Skip {waiterName}'s turn";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await StartFrontCameraPreviewAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CameraPreview.StopCameraPreview();
    }

    private async Task StartFrontCameraPreviewAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
            return;

        var cameras = await CameraPreview.GetAvailableCameras(CancellationToken.None);
        var frontCamera = cameras.FirstOrDefault(c => c.Position == CameraPosition.Front) ?? cameras.FirstOrDefault();
        if (frontCamera is null)
            return;

        CameraPreview.SelectedCamera = frontCamera;
        await CameraPreview.StartCameraPreview(CancellationToken.None);
    }

    private async void OnCaptureClicked(object? sender, EventArgs e)
    {
        if (_isShowingCapturedPhoto)
        {
            // Retake: go back to the live preview.
            CapturedPhotoImage.IsVisible = false;
            CameraPreview.IsVisible = true;
            CaptureButton.Text = "Capture Photo";
            _isShowingCapturedPhoto = false;
            return;
        }

        Stream stream;
        try
        {
            stream = await CameraPreview.CaptureImage(CancellationToken.None);
        }
        catch
        {
            return;
        }

        var tempPath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.jpg");
        await using (stream)
        await using (var fileStream = File.Create(tempPath))
        {
            await stream.CopyToAsync(fileStream);
        }

        _capturedPhotoPath = _persistence.SavePhoto(tempPath, _entryId);
        CapturedPhotoImage.Source = ImageSource.FromFile(_capturedPhotoPath);
        CapturedPhotoImage.IsVisible = true;
        CameraPreview.IsVisible = false;
        CaptureButton.Text = "Retake";
        _isShowingCapturedPhoto = true;
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        var entry = new HistoryEntry
        {
            WaiterName = _waiterName,
            TableNumber = string.Empty,
            Skipped = true,
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
