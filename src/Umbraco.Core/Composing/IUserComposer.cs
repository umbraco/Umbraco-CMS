namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Represents a user <see cref="IComposer"/>.
    /// </summary>
    /// <remarks>
    /// <para>All user composers require (compose after) all core composers.</para>
    /// </remarks>
    [ComposeAfter(typeof(ICoreComposer))]
    public interface IUserComposer : IComposer
    { }
}