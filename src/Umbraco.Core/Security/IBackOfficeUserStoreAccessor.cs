namespace Umbraco.Cms.Core.Security;

public interface IBackOfficeUserStoreAccessor
{
    IBackofficeUserStore? BackOfficeUserStore { get; }
}
