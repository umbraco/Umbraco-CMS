namespace Umbraco.Cms.Api.Common.OpenApi;

public interface ISubTypeSelector
{
    IEnumerable<Type> SubTypes(Type type);
}
