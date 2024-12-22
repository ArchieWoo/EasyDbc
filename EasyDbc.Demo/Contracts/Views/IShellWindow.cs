using System.Windows.Controls;

namespace EasyDbc.Demo.Contracts.Views;

public interface IShellWindow
{
    Frame GetNavigationFrame();

    void ShowWindow();

    void CloseWindow();
}
