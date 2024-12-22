using System.Windows.Controls;

using EasyDbc.Demo.ViewModels;

namespace EasyDbc.Demo.Views;

public partial class MainPage : Page
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
