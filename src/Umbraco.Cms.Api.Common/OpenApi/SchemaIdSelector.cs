namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
///     Selects a schema ID for a type using registered handlers.
/// </summary>
public class SchemaIdSelector : ISchemaIdSelector
{
    private readonly IEnumerable<ISchemaIdHandler> _schemaIdHandlers;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SchemaIdSelector"/> class.
    /// </summary>
    /// <param name="schemaIdHandlers">The registered schema ID handlers.</param>
    public SchemaIdSelector(IEnumerable<ISchemaIdHandler> schemaIdHandlers)
        => _schemaIdHandlers = schemaIdHandlers;

    /// <inheritdoc/>
    public virtual string SchemaId(Type type)
    {
        ISchemaIdHandler? handler = _schemaIdHandlers.FirstOrDefault(h => h.CanHandle(type));
        return handler?.Handle(type) ?? type.Name;
    }
}
