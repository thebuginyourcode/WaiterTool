using System;
using OpenCvSharp;

namespace WaiterTool.Services;

/// <summary>
/// Thin wrapper around an OpenCvSharp VideoCapture for grabbing webcam frames.
/// Opened/closed per use (e.g. while the table-number dialog is showing) so the
/// camera is not held open for the lifetime of the app.
/// </summary>
public sealed class CameraService : IDisposable
{
    private VideoCapture? _capture;

    public bool IsOpen => _capture is { IsOpened: true };

    public bool TryOpen(int cameraIndex = 0)
    {
        try
        {
            // DShow is the more reliable backend for webcam capture on Windows.
            _capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
            if (!_capture.IsOpened())
            {
                _capture.Dispose();
                _capture = null;
                return false;
            }

            return true;
        }
        catch
        {
            _capture?.Dispose();
            _capture = null;
            return false;
        }
    }

    public bool TryReadFrame(Mat frame)
    {
        if (_capture is null || !_capture.IsOpened())
            return false;

        try
        {
            return _capture.Read(frame) && !frame.Empty();
        }
        catch
        {
            return false;
        }
    }

    public void Close()
    {
        _capture?.Release();
        _capture?.Dispose();
        _capture = null;
    }

    public void Dispose() => Close();
}
