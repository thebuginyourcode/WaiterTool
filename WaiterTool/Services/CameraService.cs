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

    public bool IsOpen => _capture is not null && _capture.IsOpened();

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

    /// <summary>
    /// Opens the camera, reads a few frames to let exposure/focus settle, and returns
    /// the last good frame. Used for a quick one-shot capture with no live preview.
    /// Caller owns the returned Mat and must dispose it.
    /// </summary>
    public Mat? CaptureSingleFrame(int cameraIndex = 0, int warmupFrames = 5)
    {
        if (!TryOpen(cameraIndex))
            return null;

        try
        {
            using var frame = new Mat();
            Mat? result = null;

            for (var i = 0; i < warmupFrames; i++)
            {
                if (TryReadFrame(frame) && !frame.Empty())
                {
                    result?.Dispose();
                    result = frame.Clone();
                }
            }

            return result;
        }
        finally
        {
            Close();
        }
    }

    public void Dispose() => Close();
}
