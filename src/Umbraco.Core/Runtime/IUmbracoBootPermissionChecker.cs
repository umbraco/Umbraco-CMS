namespace Umbraco.Core.Runtime
{
    public interface IUmbracoBootPermissionChecker
    {
        void ThrowIfNotPermissions();
    }
}
