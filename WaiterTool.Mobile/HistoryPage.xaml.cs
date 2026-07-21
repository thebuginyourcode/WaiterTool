using WaiterTool.Mobile.Services;

namespace WaiterTool.Mobile;

public partial class HistoryPage : ContentPage
{
    public HistoryPage(WaiterState state)
    {
        InitializeComponent();

        HistoryCollectionView.ItemsSource = state.History;
    }
}
