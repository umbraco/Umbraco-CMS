namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Represents a core <see cref="IComposer"/>.
    /// </summary>
    /// <remarks>
    /// <para>Core composers compose after the initial composer, and before user composers.</para>
    /// </remarks>
    public interface ICoreComposer : IComposer
    { }
}
