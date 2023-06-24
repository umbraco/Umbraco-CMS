using Umbraco.Search;
using Umbraco.Search.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IIndexPresentationFactory
{
    IndexResponseModel Create(string index);
}
