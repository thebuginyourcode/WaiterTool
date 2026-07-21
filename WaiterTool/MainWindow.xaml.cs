using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WaiterTool.Models;
using WaiterTool.Services;
using WaiterTool.Windows;

namespace WaiterTool;

public partial class MainWindow : Window
{
    private readonly PersistenceService _persistence = new();

    public ObservableCollection<Waiter> Queue { get; } = new();
    public ObservableCollection<HistoryEntry> History { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        foreach (var waiter in _persistence.LoadQueue())
            Queue.Add(waiter);

        foreach (var entry in _persistence.LoadHistory().OrderByDescending(h => h.Timestamp))
            History.Add(entry);

        UpdateCurrentWaiterDisplay();
    }

    private void NewWaiterTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            AddWaiter_Click(sender, e);
    }

    private void AddWaiter_Click(object sender, RoutedEventArgs e)
    {
        var name = NewWaiterTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;

        Queue.Add(new Waiter { Name = name });
        NewWaiterTextBox.Clear();
        NewWaiterTextBox.Focus();

        _persistence.SaveQueue(Queue);
        UpdateCurrentWaiterDisplay();
    }

    private void RemoveWaiter_Click(object sender, RoutedEventArgs e)
    {
        if (QueueListBox.SelectedItem is not Waiter selected)
            return;

        Queue.Remove(selected);
        _persistence.SaveQueue(Queue);
        UpdateCurrentWaiterDisplay();
    }

    private void UpdateCurrentWaiterDisplay()
    {
        CurrentWaiterText.Text = Queue.Count > 0 ? Queue[0].Name : "—";
        NextWaiterButton.IsEnabled = Queue.Count > 0;
        SkipButton.IsEnabled = Queue.Count > 0;
    }

    private void NextWaiter_Click(object sender, RoutedEventArgs e)
    {
        if (Queue.Count == 0)
            return;

        var current = Queue[0];
        Queue.RemoveAt(0);

        var dialog = new TableNumberDialog(current.Name) { Owner = this };
        var confirmed = dialog.ShowDialog();

        if (confirmed == true)
        {
            // Cycle: the waiter returns to the back of the list for their next turn.
            Queue.Add(current);

            var entry = new HistoryEntry
            {
                WaiterName = current.Name,
                TableNumber = dialog.TableNumber,
                Skipped = false,
                Timestamp = DateTime.Now,
                PhotoPath = dialog.CapturedPhotoPath
            };

            History.Insert(0, entry);
            _persistence.SaveHistory(History);

            ShowLastCapture(entry);
        }
        else
        {
            // Cancelled: put the waiter back at the front so their turn isn't lost.
            Queue.Insert(0, current);
        }

        _persistence.SaveQueue(Queue);
        UpdateCurrentWaiterDisplay();
    }

    private void Skip_Click(object sender, RoutedEventArgs e)
    {
        if (Queue.Count == 0)
            return;

        var current = Queue[0];
        Queue.RemoveAt(0);

        string? photoPath = null;
        using (var camera = new CameraService())
        using (var frame = camera.CaptureSingleFrame())
        {
            if (frame is not null)
            {
                photoPath = _persistence.SavePhoto(frame, Guid.NewGuid());
            }
        }

        // Cycle: the waiter returns to the back of the list for their next turn.
        Queue.Add(current);

        var entry = new HistoryEntry
        {
            WaiterName = current.Name,
            TableNumber = string.Empty,
            Skipped = true,
            Timestamp = DateTime.Now,
            PhotoPath = photoPath
        };

        History.Insert(0, entry);
        _persistence.SaveHistory(History);
        ShowLastCapture(entry);

        _persistence.SaveQueue(Queue);
        UpdateCurrentWaiterDisplay();
    }

    private void ShowLastCapture(HistoryEntry entry)
    {
        NoCaptureText.Visibility = Visibility.Collapsed;
        LastCapturePanel.Visibility = Visibility.Visible;

        if (!string.IsNullOrWhiteSpace(entry.PhotoPath) && File.Exists(entry.PhotoPath))
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(entry.PhotoPath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            LastCaptureImage.Source = bitmap;
        }
        else
        {
            LastCaptureImage.Source = null;
        }

        LastCaptureInfoText.Text = $"{entry.WaiterName} — {entry.TableLabel} — {entry.Timestamp:t}";
    }
}
