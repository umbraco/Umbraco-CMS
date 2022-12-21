namespace Umbraco.Cms.Core.Models.ContentApi;

public interface IApiContent : IApiElement
{
    string? Name { get; }

    string Url { get; }
}
