namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Represents a runtime <see cref="IComposer"/>.
    /// </summary>
    /// <remarks>
    /// <para>All runtime composers are required by (compose before) all core composers</para>
    /// </remarks>
    public interface IRuntimeComposer : IComposer
    { }
}