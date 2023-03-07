namespace Umbraco.Cms.Core.Services;

public interface IBackOfficeUserStoreAccessor
{
    IBackofficeUserStore? BackOfficeUserStore { get; }
}
