using System.Windows.Controls;

using EasyDbc.Demo.Contracts.Views;
using EasyDbc.Demo.ViewModels;

using MahApps.Metro.Controls;

namespace EasyDbc.Demo.Views;

public partial class ShellWindow : MetroWindow, IShellWindow
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public Frame GetNavigationFrame()
        => shellFrame;

    public void ShowWindow()
        => Show();

    public void CloseWindow()
        => Close();
}
