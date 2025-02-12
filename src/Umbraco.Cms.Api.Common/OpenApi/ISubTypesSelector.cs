namespace Umbraco.Cms.Api.Common.OpenApi;

public interface ISubTypesSelector
{
    IEnumerable<Type> SubTypes(Type type);
}
