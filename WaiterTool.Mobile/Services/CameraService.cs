using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace WaiterTool.Mobile.Services;

/// <summary>
/// Launches the device's native camera app to capture a photo, since embedding a
/// continuous live preview requires a separate camera plugin. The caller is
/// responsible for moving the returned temp file into permanent storage.
/// </summary>
public sealed class CameraService
{
    public bool IsSupported => MediaPicker.Default.IsCaptureSupported;

    public async Task<string?> CapturePhotoAsync()
    {
        if (!IsSupported)
            return null;

        var photo = await MediaPicker.Default.CapturePhotoAsync();
        if (photo is null)
            return null;

        var tempPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
        await using var sourceStream = await photo.OpenReadAsync();
        await using var tempStream = File.Create(tempPath);
        await sourceStream.CopyToAsync(tempStream);

        return tempPath;
    }
}
