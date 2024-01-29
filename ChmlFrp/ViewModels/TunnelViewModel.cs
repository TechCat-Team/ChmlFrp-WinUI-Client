using System.Collections.ObjectModel;

using ChmlFrp.Contracts.Services;
using ChmlFrp.Contracts.ViewModels;
using ChmlFrp.Core.Contracts.Services;
using ChmlFrp.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace ChmlFrp.ViewModels;

public partial class TunnelViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly ISampleDataService _sampleDataService;

    public ObservableCollection<TunnelInfo> Source { get; } = new ObservableCollection<TunnelInfo>();

    public TunnelViewModel(INavigationService navigationService, ISampleDataService sampleDataService)
    {
        _navigationService = navigationService;
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // TODO: Replace with real data.
        var data = await _sampleDataService.GetContentGridDataAsync();
        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }

}
