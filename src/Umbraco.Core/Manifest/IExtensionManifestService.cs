namespace Umbraco.Cms.Core.Manifest;

public interface IExtensionManifestService
{
    Task<IEnumerable<ExtensionManifest>> GetManifestsAsync();
}
