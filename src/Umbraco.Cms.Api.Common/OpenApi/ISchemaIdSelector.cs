namespace Umbraco.Cms.Api.Common.OpenApi;

public interface ISchemaIdSelector
{
    string SchemaId(Type type);
}
