namespace Umbraco.Cms.Core.Manifest;

internal sealed class ExtensionManifestService : IExtensionManifestService
{
    private readonly IExtensionManifestReader _extensionManifestReader;

    public ExtensionManifestService(IExtensionManifestReader extensionManifestReader)
        => _extensionManifestReader = extensionManifestReader;

    // TODO: cache manifests for the app lifetime
    public async Task<IEnumerable<ExtensionManifest>> GetManifestsAsync() => await _extensionManifestReader.ReadManifestsAsync();
}
