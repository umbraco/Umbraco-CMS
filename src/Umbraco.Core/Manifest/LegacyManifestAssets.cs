namespace Umbraco.Cms.Core.Manifest;

public class LegacyManifestAssets
{
    public LegacyManifestAssets(string? packageName, IReadOnlyList<string> assets)
    {
        PackageName = packageName ?? throw new ArgumentNullException(nameof(packageName));
        Assets = assets ?? throw new ArgumentNullException(nameof(assets));
    }

    public string PackageName { get; }

    public IReadOnlyList<string> Assets { get; }
}
