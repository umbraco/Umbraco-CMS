namespace Umbraco.Cms.Api.Common.OpenApi;

public interface ISchemaIdHandler
{
    bool CanHandle(Type type);

    string Handle(Type type);
}
