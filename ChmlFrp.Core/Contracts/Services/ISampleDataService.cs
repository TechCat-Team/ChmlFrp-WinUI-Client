using ChmlFrp.Core.Models;

namespace ChmlFrp.Core.Contracts.Services;

// Remove this class once your pages/features are using your data.
public interface ISampleDataService
{
    Task<IEnumerable<TunnelInfo>> GetContentGridDataAsync();
}
