namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Represents a user <see cref="IComposer"/>.
    /// </summary>
    /// <remarks>
    /// <para>User composers compose after core composers, and before the final composer.</para>
    /// </remarks>
    [ComposeAfter(typeof(ICoreComposer))]
    public interface IUserComposer : IComposer
    { }
}
