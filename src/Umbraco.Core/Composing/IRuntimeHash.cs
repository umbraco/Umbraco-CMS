namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Used to create a hash value of the current runtime
/// </summary>
/// <remarks>
///     This is used to detect if the runtime itself has changed, like a DLL has changed or another dynamically compiled
///     part of the application has changed. This is used to detect if we need to re-type scan.
/// </remarks>
public interface IRuntimeHash
{
    string GetHashValue();
}
