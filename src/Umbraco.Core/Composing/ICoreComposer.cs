namespace Umbraco.Cms.Core.Composing
{
    /// <summary>
    /// Represents a core <see cref="IComposer"/>.
    /// </summary>
    /// <remarks>
    /// <para>Core composers compose after the initial composer, and before user composers.</para>
    /// </remarks>
    public interface ICoreComposer : IComposer
    {
        // TODO: This should die, there should be exactly zero core composers.
    }
}
