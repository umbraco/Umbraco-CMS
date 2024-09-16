namespace Umbraco.Cms.Api.Common.OpenApi;

public interface ISubTypeHandler
{
    bool CanHandle(Type type, string documentName);

    IEnumerable<Type> Handle(Type type);
}
