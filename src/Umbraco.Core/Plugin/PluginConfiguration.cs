namespace Umbraco.Cms.Core.Plugin;

public class PluginConfiguration
{
    public required string Name { get; set; }

    public string? Version { get; set; }

    public bool AllowTelemetry { get; set; } = true;

    public required object[] Extensions { get; set; }
}
