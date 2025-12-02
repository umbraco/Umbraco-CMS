namespace Umbraco.Cms.Core.Persistence.Repositories;

[Obsolete("Installation logging is no longer supported and this interface will be removed in Umbraco 19.")]
public interface IInstallationRepository
{
    [Obsolete("This method no longer has any function and will be removed in Umbraco 19.")]
    Task SaveInstallLogAsync(InstallLog installLog);
}
