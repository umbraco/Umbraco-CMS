using System.Diagnostics;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents a published data type.
/// </summary>
/// <remarks>
///     <para>
///         Instances of the <see cref="PublishedDataType" /> class are immutable, ie
///         if the data type changes, then a new class needs to be created.
///     </para>
///     <para>These instances should be created by an <see cref="IPublishedContentTypeFactory" />.</para>
/// </remarks>
[DebuggerDisplay("{EditorAlias}")]
public class PublishedDataType
{
    private readonly Lazy<object?> _lazyConfiguration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedDataType" /> class.
    /// </summary>
    public PublishedDataType(int id, string editorAlias, Lazy<object?> lazyConfiguration)
    {
        _lazyConfiguration = lazyConfiguration;

        Id = id;
        EditorAlias = editorAlias;
    }

    /// <summary>
    ///     Gets the datatype identifier.
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     Gets the data type editor alias.
    /// </summary>
    public string EditorAlias { get; }

    /// <summary>
    ///     Gets the data type configuration.
    /// </summary>
    public object? Configuration => _lazyConfiguration?.Value;

    /// <summary>
    ///     Gets the configuration object.
    /// </summary>
    /// <typeparam name="T">The expected type of the configuration object.</typeparam>
    /// <exception cref="InvalidCastException">When the datatype configuration is not of the expected type.</exception>
    public T? ConfigurationAs<T>()
        where T : class
    {
        switch (Configuration)
        {
            case null:
                return null;
            case T configurationAsT:
                return configurationAsT;
        }

        throw new InvalidCastException(
            $"Cannot cast dataType configuration, of type {Configuration.GetType().Name}, to {typeof(T).Name}.");
    }
}
