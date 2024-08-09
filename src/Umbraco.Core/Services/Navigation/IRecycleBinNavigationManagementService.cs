namespace Umbraco.Cms.Core.Services.Navigation;

public interface IRecycleBinNavigationManagementService
{
    Task RebuildBinAsync();

    bool RemoveFromBin(Guid key);

    bool RestoreFromBin(Guid key, Guid? targetParentKey = null);
}
