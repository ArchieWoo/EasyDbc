using System.Windows.Controls;

namespace EasyDbc.Demo.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);

    Page GetPage(string key);
}
