using System.Threading;
using CommunityToolkit.Maui.Storage;
using WaiterTool.Mobile.Services;

namespace WaiterTool.Mobile;

public partial class SettingsPage : ContentPage
{
    private readonly PersistenceService _persistence;

    public SettingsPage(PersistenceService persistence)
    {
        InitializeComponent();

        _persistence = persistence;
        RefreshLogFolderLabel();
    }

    private void RefreshLogFolderLabel()
    {
        LogFolderLabel.Text = $"Current log folder:\n{_persistence.GetLogFolder()}";
    }

    private async void OnChooseFolderClicked(object? sender, EventArgs e)
    {
        var result = await FolderPicker.Default.PickAsync(CancellationToken.None);
        if (!result.IsSuccessful)
            return;

        _persistence.SetLogFolder(result.Folder.Path);
        RefreshLogFolderLabel();
    }
}
