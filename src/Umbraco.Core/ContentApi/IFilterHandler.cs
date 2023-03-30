namespace Umbraco.Cms.Core.ContentApi;

public interface IFilterHandler : IQueryHandler
{
    FilterOption BuildFilterOption(string filterValueString);
}
