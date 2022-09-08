namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Represents an artifact ie an object that can be transfered between environments.
/// </summary>
public interface IArtifact : IArtifactSignature
{
    string Name { get; }

    string? Alias { get; }
}
