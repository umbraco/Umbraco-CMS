namespace Umbraco.Cms.Core.Runtime;

/// <summary>
///     Defines a class which can generate a distinct key for a MainDom boundary.
/// </summary>
public interface IMainDomKeyGenerator
{
    /// <summary>
    ///     Returns a key that signifies a MainDom boundary.
    /// </summary>
    string GenerateKey();
}
