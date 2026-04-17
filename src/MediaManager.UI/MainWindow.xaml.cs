using MediaManager.UI.ViewModels;
using System.Windows;

namespace MediaManager.UI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.ContentFrame = ContentFrame;
    }
}
