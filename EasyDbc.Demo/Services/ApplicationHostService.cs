using EasyDbc.Demo.Contracts.Activation;
using EasyDbc.Demo.Contracts.Services;
using EasyDbc.Demo.Contracts.Views;
using EasyDbc.Demo.ViewModels;

using Microsoft.Extensions.Hosting;

namespace EasyDbc.Demo.Services;

public class ApplicationHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private IShellWindow _shellWindow;
    private bool _isInitialized;

    public ApplicationHostService(IServiceProvider serviceProvider, IEnumerable<IActivationHandler> activationHandlers, INavigationService navigationService)
    {
        _serviceProvider = serviceProvider;
        _activationHandlers = activationHandlers;
        _navigationService = navigationService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Initialize services that you need before app activation
        await InitializeAsync();

        await HandleActivationAsync();

        // Tasks after activation
        await StartupAsync();
        _isInitialized = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            await Task.CompletedTask;
        }
    }

    private async Task StartupAsync()
    {
        if (!_isInitialized)
        {
            await Task.CompletedTask;
        }
    }

    private async Task HandleActivationAsync()
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle());

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync();
        }

        await Task.CompletedTask;

        if (App.Current.Windows.OfType<IShellWindow>().Count() == 0)
        {
            // Default activation that navigates to the apps default page
            _shellWindow = _serviceProvider.GetService(typeof(IShellWindow)) as IShellWindow;
            _navigationService.Initialize(_shellWindow.GetNavigationFrame());
            _shellWindow.ShowWindow();
            _navigationService.NavigateTo(typeof(MainViewModel).FullName);
            await Task.CompletedTask;
        }
    }
}
