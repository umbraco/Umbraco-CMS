namespace Umbraco.Core.Components
{
    /// <summary>
    /// Represents a core <see cref="IComposer"/>.
    /// </summary>
    /// <remarks>
    /// <para>All core composers are required by (compose before) all user composers,
    /// and require (compose after) all runtime composers.</para>
    /// </remarks>
    [Require(typeof(IRuntimeComposer))]
    public interface ICoreComposer : IComposer
    { }
}