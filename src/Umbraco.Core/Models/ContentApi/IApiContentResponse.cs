namespace Umbraco.Cms.Core.Models.ContentApi;

public interface IApiContentResponse : IApiContent
{
    IDictionary<string, IApiContentRoute> Cultures { get; }
}
