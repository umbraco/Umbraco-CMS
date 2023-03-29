namespace Umbraco.Cms.Core.ContentApi;

public interface ISelectorHandler : IQueryHandler
{
    SelectorOption? BuildSelectorOption(string selectorValueString);
}
