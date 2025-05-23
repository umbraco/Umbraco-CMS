namespace Umbraco.Cms.Api.Common.OpenApi;

public class SchemaIdSelector : ISchemaIdSelector
{
    private readonly IEnumerable<ISchemaIdHandler> _schemaIdHandlers;

    public SchemaIdSelector(IEnumerable<ISchemaIdHandler> schemaIdHandlers)
        => _schemaIdHandlers = schemaIdHandlers;

    public virtual string SchemaId(Type type)
    {
        ISchemaIdHandler? handler = _schemaIdHandlers.FirstOrDefault(h => h.CanHandle(type));
        return handler?.Handle(type) ?? type.Name;
    }
}
