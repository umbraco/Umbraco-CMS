namespace Umbraco.Core.Components
{
    /// <summary>
    /// Represents a user <see cref="IComposer"/>.
    /// </summary>
    /// <remarks>
    /// <para>All user composers require (compose after) all core composers.</para>
    /// </remarks>
    [Require(typeof(ICoreComposer))]
    public interface IUserComposer : IComposer
    { }
}