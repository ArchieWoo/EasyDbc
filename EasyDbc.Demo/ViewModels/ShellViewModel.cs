using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using EasyDbc.Demo.Contracts.Services;
using EasyDbc.Demo.Properties;

using MahApps.Metro.Controls;

namespace EasyDbc.Demo.ViewModels;

public class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private HamburgerMenuItem _selectedMenuItem;
    private RelayCommand _goBackCommand;
    private ICommand _menuItemInvokedCommand;
    private ICommand _loadedCommand;
    private ICommand _unloadedCommand;

    public HamburgerMenuItem SelectedMenuItem
    {
        get { return _selectedMenuItem; }
        set { SetProperty(ref _selectedMenuItem, value); }
    }

    // TODO: Change the icons and titles for all HamburgerMenuItems here.
    public ObservableCollection<HamburgerMenuItem> MenuItems { get; } = new ObservableCollection<HamburgerMenuItem>()
    {
        new HamburgerMenuGlyphItem() { Label = Resources.ShellMainPage, Glyph = "\uE8A5", TargetPageType = typeof(MainViewModel) },
    };

    public RelayCommand GoBackCommand => _goBackCommand ?? (_goBackCommand = new RelayCommand(OnGoBack, CanGoBack));

    public ICommand MenuItemInvokedCommand => _menuItemInvokedCommand ?? (_menuItemInvokedCommand = new RelayCommand(OnMenuItemInvoked));

    public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

    public ICommand UnloadedCommand => _unloadedCommand ?? (_unloadedCommand = new RelayCommand(OnUnloaded));

    public ShellViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    private void OnLoaded()
    {
        _navigationService.Navigated += OnNavigated;
    }

    private void OnUnloaded()
    {
        _navigationService.Navigated -= OnNavigated;
    }

    private bool CanGoBack()
        => _navigationService.CanGoBack;

    private void OnGoBack()
        => _navigationService.GoBack();

    private void OnMenuItemInvoked()
        => NavigateTo(SelectedMenuItem.TargetPageType);

    private void NavigateTo(Type targetViewModel)
    {
        if (targetViewModel != null)
        {
            _navigationService.NavigateTo(targetViewModel.FullName);
        }
    }

    private void OnNavigated(object sender, string viewModelName)
    {
        var item = MenuItems
                    .OfType<HamburgerMenuItem>()
                    .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
        if (item != null)
        {
            SelectedMenuItem = item;
        }

        GoBackCommand.NotifyCanExecuteChanged();
    }
}
