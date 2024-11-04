namespace Umbraco.Cms.Api.Common.OpenApi;

public interface ISubTypesHandler
{
    bool CanHandle(Type type, string documentName);

    IEnumerable<Type> Handle(Type type);
}
