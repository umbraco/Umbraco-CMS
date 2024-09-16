namespace Umbraco.Cms.Api.Common.OpenApi;

public interface ISchemaIdHandler
{
    [Obsolete("Use CanHandle(Type type, string documentName) instead. Will be removed in v16.")]
    bool CanHandle(Type type);

#pragma warning disable CS0618 // Type or member is obsolete
    bool CanHandle(Type type, string documentName) => CanHandle(type);
#pragma warning restore CS0618 // Type or member is obsolete

    string Handle(Type type);
}
