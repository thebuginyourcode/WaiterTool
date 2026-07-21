using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using WaiterTool.Services;

namespace WaiterTool.Windows;

/// <summary>
/// Prompts for a table number while showing a live webcam preview, and
/// captures the current frame as the confirmation photo.
/// </summary>
public partial class TableNumberDialog : Window
{
    private static readonly Regex DigitsOnly = new("^[0-9]+$");

    private readonly CameraService _camera = new();
    private readonly PersistenceService _persistence = new();
    private readonly DispatcherTimer _previewTimer;
    private readonly Mat _latestFrame = new();
    private readonly Guid _entryId = Guid.NewGuid();

    public string TableNumber { get; private set; } = string.Empty;
    public string? CapturedPhotoPath { get; private set; }

    public TableNumberDialog(string waiterName)
    {
        InitializeComponent();
        HeaderText.Text = $"{waiterName}'s turn";

        _previewTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(66) // ~15 fps
        };
        _previewTimer.Tick += PreviewTimer_Tick;

        Loaded += TableNumberDialog_Loaded;
    }

    private void TableNumberDialog_Loaded(object sender, RoutedEventArgs e)
    {
        if (_camera.TryOpen())
        {
            _previewTimer.Start();
        }
        else
        {
            CameraStatusText.Text = "Camera unavailable. You can still confirm the table number without a photo.";
            CameraStatusText.Visibility = Visibility.Visible;
        }

        TableNumberTextBox.Focus();
    }

    private void PreviewTimer_Tick(object? sender, EventArgs e)
    {
        if (_camera.TryReadFrame(_latestFrame))
        {
            PreviewImage.Source = _latestFrame.ToBitmapSource();
        }
    }

    private void TableNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !DigitsOnly.IsMatch(e.Text);
    }

    private void TableNumberTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            Confirm_Click(sender, e);
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        var tableNumber = TableNumberTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(tableNumber))
        {
            MessageBox.Show(this, "Please enter a table number.", "Table Number Required",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        TableNumber = tableNumber;

        if (_camera.IsOpen && !_latestFrame.Empty())
        {
            CapturedPhotoPath = _persistence.SavePhoto(_latestFrame, _entryId);
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _previewTimer.Stop();
        _camera.Dispose();
        _latestFrame.Dispose();
    }
}
