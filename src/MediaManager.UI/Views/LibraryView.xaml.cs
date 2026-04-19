using MediaManager.UI.DisplayModels;
using MediaManager.UI.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace MediaManager.UI.Views;

public partial class LibraryView : UserControl
{
    public LibraryView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is LibraryViewModel vm)
                await vm.LoadCommand.ExecuteAsync(null);
        };
    }

    private void Card_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.FrameworkElement fe &&
            fe.DataContext is MediaFileDisplayItem item &&
            DataContext is LibraryViewModel vm)
        {
            vm.SelectedItem = item;
        }
    }
}
