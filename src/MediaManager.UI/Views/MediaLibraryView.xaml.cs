using MediaManager.UI.ViewModels;

namespace MediaManager.UI.Views;

public partial class MediaLibraryView : System.Windows.Controls.UserControl
{
    public MediaLibraryView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is MediaLibraryViewModel vm)
                await vm.LoadLibrariesCommand.ExecuteAsync(null);
        };
    }
}
