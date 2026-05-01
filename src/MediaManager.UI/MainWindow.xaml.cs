using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using MediaManager.UI.ViewModels;

namespace MediaManager.UI;

public partial class MainWindow : HandyControl.Controls.Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
