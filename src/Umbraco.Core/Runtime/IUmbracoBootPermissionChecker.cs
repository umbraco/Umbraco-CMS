namespace Umbraco.Cms.Core.Runtime;

public interface IUmbracoBootPermissionChecker
{
    void ThrowIfNotPermissions();
}
