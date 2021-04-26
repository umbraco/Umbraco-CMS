namespace Umbraco.Cms.Core.Composing
{
    /// <summary>
    /// Represents a user <see cref="IComposer"/>.
    /// </summary>
    /// <remarks>
    /// <para>User composers compose after core composers, and before the final composer.</para>
    /// </remarks>
    [ComposeAfter(typeof(IComposer))]
    public interface IUserComposer : IComposer
    { }
}
