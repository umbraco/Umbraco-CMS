namespace Umbraco.Cms.Core.IO;

/// <summary>
/// Represents a shadow file systems scope that can be completed or disposed.
/// </summary>
/// <remarks>
/// Shadow filesystems are used to temporarily capture file system changes within a scope,
/// and either apply or discard them when the scope completes.
/// </remarks>
internal sealed class ShadowFileSystems : ICompletable
{
    private readonly FileSystems _fileSystems;
    private bool _completed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowFileSystems"/> class.
    /// </summary>
    /// <param name="fileSystems">The file systems to shadow.</param>
    /// <param name="id">The unique identifier for this shadow scope.</param>
    /// <remarks>Invoked by the filesystems when shadowing.</remarks>
    public ShadowFileSystems(FileSystems fileSystems, string id)
    {
        _fileSystems = fileSystems;
        Id = id;

        _fileSystems.BeginShadow(id);
    }

    /// <summary>
    /// Gets the unique identifier for this shadow scope.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Marks the shadow scope as completed, indicating that changes should be applied.
    /// </summary>
    /// <remarks>Invoked by the scope when exiting, if completed.</remarks>
    public void Complete() => _completed = true;

    /// <summary>
    /// Disposes the shadow scope, applying or discarding changes based on completion status.
    /// </summary>
    /// <remarks>Invoked by the scope when exiting.</remarks>
    public void Dispose() => _fileSystems.EndShadow(Id, _completed);
}
