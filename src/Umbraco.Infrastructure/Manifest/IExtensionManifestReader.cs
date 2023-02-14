namespace Umbraco.Cms.Core.Manifest;

public interface IExtensionManifestReader
{
    Task<IEnumerable<ExtensionManifest>> ReadManifestsAsync();
}
