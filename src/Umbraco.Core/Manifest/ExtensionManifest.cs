namespace Umbraco.Cms.Core.Manifest;

public class ExtensionManifest
{
    public required string Name { get; set; }

    public string? Version { get; set; }

    public bool AllowTelemetry { get; set; } = true;

    public required object[] Extensions { get; set; }
}
