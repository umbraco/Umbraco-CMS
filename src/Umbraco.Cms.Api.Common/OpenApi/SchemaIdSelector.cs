namespace Umbraco.Cms.Api.Common.OpenApi;

public class SchemaIdSelector : ISchemaIdSelector
{
    private readonly IEnumerable<ISchemaIdHandler> _schemaIdHandlers;

    public SchemaIdSelector(IEnumerable<ISchemaIdHandler> schemaIdHandlers)
        => _schemaIdHandlers = schemaIdHandlers;

    public virtual string SchemaId(Type type)
    {
        // Unwrap nullable types so handlers receive the underlying type
        Type targetType = Nullable.GetUnderlyingType(type) ?? type;

        ISchemaIdHandler? handler = _schemaIdHandlers.FirstOrDefault(h => h.CanHandle(targetType));
        return handler?.Handle(targetType) ?? targetType.Name;
    }
}
