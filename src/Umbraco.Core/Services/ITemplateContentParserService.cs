namespace Umbraco.Cms.Core.Services;

public interface ITemplateContentParserService
{
    string? MasterTemplateAlias(string? viewContent);
}
